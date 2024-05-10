using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;
using Jint;
using System.Dynamic;
using Jint.Runtime.Interop;
using System.Text.Json;
using YeetMacro2.ViewModels.NodeViewModels;
using YeetMacro2.ViewModels;
using OneOf;
using Jint.Native;
using Jint.Runtime;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace YeetMacro2.Services;

public interface IScriptService
{
    string RunScript(ScriptNode targetScript, ScriptNodeManagerViewModel scriptNodeManager, 
        MacroSet macroSet, PatternNodeManagerViewModel patternNodeManager, 
        SettingNodeManagerViewModel settingNodeManager, DailyNodeManagerViewModel dailyNodeManager,
        WeeklyNodeManagerViewModel weeklyNodeManager);
    void Stop();
}

public class ScriptService: IScriptService
{
    ILogger _logger;
    IScreenService _screenService;
    IToastService _toastService;
    Engine _engine;
    Random _random;
    Dictionary<string, PatternNode> _jsonValueToPatternNode;
    MacroService _macroService;

    public ScriptService(ILogger<ScriptService> logger, IScreenService screenService, IToastService toastService, MacroService macroService)
    {
        _logger = logger;
        _screenService = screenService;
        _toastService = toastService;
        _random = new Random();
        _jsonValueToPatternNode = new Dictionary<string, PatternNode>();
        _macroService = macroService;
        _engine = new Engine(options => options
            .SetTypeConverter(e => new JsToDotNetConverter(e))
            .AddObjectConverter(new DotNetToJsConverter())
            .AddExtensionMethods(typeof(System.Linq.Enumerable))
        );
        _engine.SetValue("sleep", new Action<int>((ms) => Sleep(ms)));
        dynamic engineLogger = new ExpandoObject();
        engineLogger.info = new Action<string>((msg) => _logger.LogInformation(msg));
        engineLogger.debug = new Action<string>((msg) => _logger.LogDebug(msg));
        _engine.SetValue("logger", engineLogger);
        _engine.SetValue("macroService", _macroService);
    }

    public void Sleep(int ms)
    {
        Thread.Sleep(ms);
    }

    public string RunScript(ScriptNode targetScript, ScriptNodeManagerViewModel scriptNodeManger, 
        MacroSet macroSet, PatternNodeManagerViewModel patternNodeManager, 
        SettingNodeManagerViewModel settingNodeManager, DailyNodeManagerViewModel dailyNodeManager,
        WeeklyNodeManagerViewModel weeklyNodeManager)
    {
        string result = String.Empty;
        if (_macroService.IsRunning) return result;
        _engine.SetValue("result", JsValue.Null);

        // https://github.com/sebastienros/jint
        _macroService.IsRunning = true;
        try
        {
            foreach (var script in scriptNodeManger.Root.Nodes)
            {
                if (script.Text.StartsWith("// @raw-script"))
                {
                    _engine.Execute(script.Text);
                }
                else
                {
                    _engine.Execute($"function {script.Name}() {{ {script.Text} }}");
                }
            }
            _engine.SetValue("patterns", patternNodeManager.Root);
            _engine.SetValue("settings", settingNodeManager.Root);
            _engine.SetValue("dailyManager", dailyNodeManager);
            _engine.SetValue("weeklyManager", weeklyNodeManager);

            var jsResult = _engine.Evaluate($"{{\n{targetScript.Text}\n {(targetScript.Text.StartsWith("// @raw-script") ? targetScript.Name + "()\n" : "")}}}");

            _toastService.Show(_macroService.IsRunning ? "Script finished..." : "Script stopped...");

            if (jsResult is JsObject || jsResult is JsArray)
            {
                _engine.SetValue("result", jsResult);
                return _engine.Evaluate("JSON.stringify(result, null, 2)").AsString();
            }
            else if (jsResult is IObjectWrapper objectWrapper)
            {
                return JsonSerializer.Serialize(objectWrapper.Target, new JsonSerializerOptions()
                {
                    WriteIndented = true
                });
            }
            else if (jsResult != JsValue.Null && jsResult != JsValue.Undefined)
            {
                return jsResult.ToString();
            }
            else
            {
                return String.Empty;
            }
        }
        catch (JavaScriptException jex)
        {
            _engine.SetValue("error", jex.Error);
            return _engine.Evaluate("JSON.stringify({ ...error, message: error.message }, null, 2)").AsString();
        }
        catch (Exception ex)
        {
            _toastService.Show("Error: " + ex.Message);
            _logger.LogError(ex, $"Script Error: {ex.Message}");
            return $"{ex.Message}: \n\t{ex.StackTrace}";
        }
        finally
        {
            _macroService.IsRunning = false;
            _macroService.Reset();
        }
    }

    public void Stop()
    {
        _macroService.IsRunning = false;
    }
}

// https://github.com/sebastienros/jint/blob/d48ebd50ba5af240f484a3763227d2a53999a365/Jint.Tests/Runtime/EngineTests.cs#L3079
public class JsToDotNetConverter : DefaultTypeConverter
{
    JsonSerializerOptions _jsonOpts = new JsonSerializerOptions()
    {
        Converters = { new JsonStringEnumConverter() }
    };
    public JsToDotNetConverter(Engine engine) : base(engine)
    {

    }

    public override object Convert(object value, Type type, IFormatProvider formatProvider)
    {
        if (type == typeof(Rect))
        {
            return JsonSerializer.Deserialize<Rect>(JsonSerializer.Serialize(value));
        }
        else if (type.IsEnum)
        {
            return Enum.Parse(type, value.ToString());
        }
        else if (type == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(value.ToString());
        }


        return base.Convert(value, type, formatProvider);
    }

    public override bool TryConvert(object value, Type type, IFormatProvider formatProvider, out object converted)
    {
        if (type == typeof(OneOf<PatternNode, PatternNode[]>))
        {
            if (value is object[] arr)
            {
                converted = OneOf<PatternNode, PatternNode[]>.FromT1(arr.Cast<PatternNode>().ToArray());
                return true;
            }
            else
            {
                converted = OneOf<PatternNode, PatternNode[]>.FromT0(value as PatternNode);
                return true;
            }
        }
        else if (type == typeof(PollPatternFindOptions))
        {
            var opts = JsonSerializer.Deserialize<PollPatternFindOptions>(JsonSerializer.Serialize(value));
            var dict = value as IDictionary<string, object>;
            opts.PredicatePattern = dict.ContainsKey("PredicatePattern") ? (OneOf<PatternNode, PatternNode[]>?)ToOneOfPatternNode(dict["PredicatePattern"]) : null;
            opts.ClickPattern = dict.ContainsKey("ClickPattern") ? (OneOf<PatternNode, PatternNode[]>?)ToOneOfPatternNode(dict["ClickPattern"]) : null;
            opts.InversePredicatePattern = dict.ContainsKey("InversePredicatePattern") ? (OneOf<PatternNode, PatternNode[]>?)ToOneOfPatternNode(dict["InversePredicatePattern"]) : null;

            converted = opts;
            return true;
        }
        else if (type == typeof(SwipePollPatternFindOptions))
        {
            var opts = JsonSerializer.Deserialize<SwipePollPatternFindOptions>(JsonSerializer.Serialize(value));
            converted = opts;
            return true;
        }
        else if (type == typeof(FindOptions))
        {
            var opts = JsonSerializer.Deserialize<FindOptions>(JsonSerializer.Serialize(value));
            converted = opts;
            return true;
        }
        else if (type == typeof(CloneOptions))
        {
            var opts = JsonSerializer.Deserialize<CloneOptions>(JsonSerializer.Serialize(value, _jsonOpts), _jsonOpts);
            converted = opts;
            return true;
        }
        else if (type == typeof(Point))
        {
            converted = JsonSerializer.Deserialize<Point>(JsonSerializer.Serialize(value));
            return true;
        }
        else if (type == typeof(JsonObject))
        {
            converted = JsonObject.Parse(JsonSerializer.Serialize(value));
            return true;
        }

        return base.TryConvert(value, type, formatProvider, out converted);
    }

    public OneOf<PatternNode, PatternNode[]> ToOneOfPatternNode(dynamic value)
    {
        if (value is object[] arr)
        {
            return arr.Cast<PatternNode>().ToArray();
        }
        else
        {
            return value as PatternNode;
        }
    }
}

// https://github.com/sebastienros/jint/discussions/1154
// https://www.appsloveworld.com/csharp/100/315/how-to-enumerate-a-dictionary-in-jint?expand_article=1
// Wrapping ObjectWrapper was considered when I thought I need to customize it
public class DotNetToJsConverter : Jint.Runtime.Interop.IObjectConverter
{
    public bool TryConvert(Engine engine, object value, out JsValue result)
    {
        // If you want object properties/methods to work in JavaScript for your type, add them here
        if (value is SettingNode || value is TodoJsonElementViewModel || value is DateTimeOffset)
        {
            result = new ObjectWrapper(engine, value);
            return true;
        }
        else if (value is JsonObject jsonObject)
        {
            result = engine.Evaluate($"return {jsonObject.ToJsonString()}");
            return true;
        }

        result = JsValue.Null;
        return false;
    }
}

