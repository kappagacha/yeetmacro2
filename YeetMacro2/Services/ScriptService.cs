using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;
using Jint;
using System.Dynamic;
using Jint.Runtime.Interop;
using OneOf;
using System.Text.Json;
using YeetMacro2.ViewModels.NodeViewModels;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Services;

public interface IScriptService
{
    bool InDebugMode { get; set; }
    void RunScript(ScriptNode targetScript, ScriptNodeManagerViewModel scriptNodeManger, MacroSet macroSet, PatternNodeManagerViewModel patternNodeManager, SettingNodeManagerViewModel settingNodeManager, Action<string> onScriptFinished);
    void Stop();
}

public class ScriptService : IScriptService
{
    ILogger _logger;
    IScreenService _screenService;
    IToastService _toastService;
    Engine _engine;
    Random _random;
    Dictionary<string, PatternNode> _jsonValueToPatternNode;
    public bool InDebugMode { get; set; }
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
            .SetTypeConverter(e => new CustomJintTypeConverter(e))
        );

        Task.Run(InitJSContext);
    }

    public void Sleep(int ms)
    {
        new System.Threading.ManualResetEvent(false).WaitOne(ms);
    }

    public void RunScript(ScriptNode targetScript, ScriptNodeManagerViewModel scriptNodeManger, MacroSet macroSet, PatternNodeManagerViewModel patternNodeManager, SettingNodeManagerViewModel settingNodeManager, Action<string> onScriptFinished)
    {
        if (_macroService.IsRunning) return;

        // https://github.com/sebastienros/jint
        _macroService.IsRunning = true;
        _macroService.InDebugMode = InDebugMode;
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
            _engine.Execute("result = undefined");
            _engine.Execute($"patterns = {patternNodeManager.ToJson()}; settings = {settingNodeManager.ToJson()}; resolvePath({{ $isParent: true, ...patterns }});");
            _engine.Execute($"{{\n{targetScript.Text}\n}}");

            _toastService.Show(_macroService.IsRunning ? "Script finished..." : "Script stopped...");
        }
        catch (Exception ex)
        {
            _toastService.Show("Error: " + ex.Message);
            _logger.LogError(ex, $"Script Error: {ex.Message}");
            _engine.SetValue("result", $"{ex.Message}: \n\t{ex.StackTrace}");
        }
        finally
        {
            string result = String.Empty;
            var jsResult = _engine.GetValue("result");
            if (jsResult is Jint.Native.JsObject jsObjectResult)
            {
                result = _engine.Evaluate("JSON.stringify(result, null, 2)").AsString();
            }
            else if (jsResult is IObjectWrapper objectWrapper)
            {
                result = JsonSerializer.Serialize(objectWrapper.Target, new JsonSerializerOptions()
                {
                    WriteIndented = true
                });
            }
            else if (jsResult != Jint.Native.JsValue.Null && jsResult != Jint.Native.JsValue.Undefined)
            {
                result = jsResult.ToString();
            }
            onScriptFinished?.Invoke(result);
            _macroService.IsRunning = false;
            _macroService.Reset();
        }

    }

    public void Stop()
    {
        _macroService.IsRunning = false;
    }

    public void InitJSContext()
    {
        _engine.SetValue("sleep", new Action<int>((ms) => Sleep(ms)));
        dynamic logger = new ExpandoObject();
        logger.info = new Action<string>((msg) => _logger.LogInformation(msg));
        logger.debug = new Action<string>((msg) => _logger.LogDebug(msg));
        _engine.SetValue("logger", logger);
        _engine.SetValue("macroService", _macroService);

        var initScript = ServiceHelper.GetAssetContent("initJavaScriptContext.js");
        _engine.Execute(initScript);
    }
}

// https://github.com/sebastienros/jint/blob/d48ebd50ba5af240f484a3763227d2a53999a365/Jint.Tests/Runtime/EngineTests.cs#L3079
public class CustomJintTypeConverter : DefaultTypeConverter
{
    public CustomJintTypeConverter(Engine engine) : base(engine)
    {

    }

    public override bool TryConvert(object value, Type type, IFormatProvider formatProvider, out object converted)
    {
        if (type == typeof(PatternNode))
        {
            converted = PatternNodeManagerViewModel.FromJsonNode(System.Text.Json.JsonSerializer.Serialize(value));
            return true;
        }
        else if (type == typeof(OneOf<PatternNode, PatternNode[]>))
        {
            converted = ToOneOfPatternNode(value);
            return true;
        }
        else if (type == typeof(PollPatternFindOptions))
        {
            var opts = JsonSerializer.Deserialize<PollPatternFindOptions>(System.Text.Json.JsonSerializer.Serialize(value));
            var dict = value as IDictionary<string, object>;
            opts.PredicatePattern = dict.ContainsKey("PredicatePattern") ? (OneOf<PatternNode, PatternNode[]>?)ToOneOfPatternNode(dict["PredicatePattern"]) : null;
            opts.ClickPattern = dict.ContainsKey("ClickPattern") ? (OneOf<PatternNode, PatternNode[]>?)ToOneOfPatternNode(dict["ClickPattern"]) : null;
            opts.InversePredicatePattern = dict.ContainsKey("InversePredicatePattern") ? (OneOf<PatternNode, PatternNode[]>?)ToOneOfPatternNode(dict["InversePredicatePattern"]) : null;

            converted = opts;
            return true;
        }

        return base.TryConvert(value, type, formatProvider, out converted);
    }

    private OneOf<PatternNode, PatternNode[]> ToOneOfPatternNode(object value)
    {
        if (value is dynamic[] arr)
        {
            var patternNodes = new PatternNode[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                patternNodes[i] = PatternNodeManagerViewModel.FromJsonNode(System.Text.Json.JsonSerializer.Serialize(arr[i]));
            }
            return OneOf<PatternNode, PatternNode[]>.FromT1(patternNodes);
        }
        else
        {
            return OneOf<PatternNode, PatternNode[]>.FromT0(PatternNodeManagerViewModel.FromJsonNode(System.Text.Json.JsonSerializer.Serialize(value)));
        }
    }
}
