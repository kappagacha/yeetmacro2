using Microsoft.Extensions.Logging;
using YeetMacro2.ViewModels;
using YantraJS.Core;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services;

public interface IScriptService
{
    bool InDebugMode { get; set; }
    void RunScript(string script, string jsonPatterns, string jsonOptions);
    void Stop();
}

public class ScriptService : IScriptService
{
    ILogger _logger;
    IScreenService _screenService;
    IToastService _toastService;
    bool _isRunning;
    JSContext _jsContext;
    Dictionary<object, PatternNode> _jsonValueToPatternNode;
    public bool InDebugMode { get; set; }

    public ScriptService(ILogger<ScriptService> logger, IScreenService screenService, IToastService toastService)
    {
        _logger = logger;
        _screenService = screenService;
        _toastService = toastService;
        _jsonValueToPatternNode = new Dictionary<object, PatternNode>();
        var synchronizationContext = new SynchronizationContext();
        _jsContext = new JSContext(synchronizationContext);

        Task.Run(InitJSContext);
    }

    public void RunScript(string script, string jsonPatterns, string jsonSettings)
    {
        if (_isRunning) return;

        Task.Run(async () =>
        {
            _isRunning = true;
            try
            {
                await _jsContext.ExecuteAsync($"patterns = {jsonPatterns}; settings = {jsonSettings}; resolvePath({{ $isParent: true, ...patterns }});");
                await _jsContext.ExecuteAsync(script);
                _isRunning = false;
                _jsonValueToPatternNode.Clear();
                //_toastService.Show("Script finished...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _toastService.Show("Error: " + ex.Message);
            }
        });
    }

    public void Stop()
    {
        _isRunning = false;
        _toastService.Show("Script stopped...");
    }

    public async Task InitJSContext()
    {
        _jsContext["state"] = new JSObject(new List<JSProperty>()
        {
            new JSProperty("isRunning", new JSFunction((in Arguments a) =>
            {
                return _isRunning ? JSBoolean.True : JSBoolean.False;
            }), JSPropertyAttributes.ReadonlyProperty)
        });
        _jsContext["logger"] = new JSObject(new List<JSProperty>()
        {
            new JSProperty("info", new JSFunction((in Arguments a) =>
            {
                _logger.LogInformation(a[0].ToString());
                return JSNull.Value;
            }), JSPropertyAttributes.ReadonlyValue),
            new JSProperty("debug", new JSFunction((in Arguments a) =>
            {
                _logger.LogDebug(a[0].ToString());
                return JSNull.Value;
            }), JSPropertyAttributes.ReadonlyValue)
        });
        _jsContext["screenService"] = new JSObject(new List<JSProperty>()
        {
            new JSProperty("debugClear", new JSFunction((in Arguments a) =>
            {
                _screenService.DebugClear();
                return JSNull.Value;
            }), JSPropertyAttributes.ReadonlyValue),
            new JSProperty("doClick", new JSFunction((in Arguments a) =>
            {
                var jsPoint = a[0];
                var x = jsPoint["x"].DoubleValue;
                var y = jsPoint["y"].DoubleValue;
                _screenService.DoClick(new Point(x, y));
                return JSNull.Value;
            }), JSPropertyAttributes.ReadonlyValue),
            new JSProperty("doSwipe", new JSFunction((in Arguments a) =>
            {
                var jsPointStart = a[0];
                var jsPointEnd = a[1];
                _screenService.DoSwipe(
                    new Point(jsPointStart["x"].DoubleValue, jsPointStart["y"].DoubleValue),
                    new Point(jsPointEnd["x"].DoubleValue, jsPointEnd["y"].DoubleValue));
                return JSNull.Value;
            }), JSPropertyAttributes.ReadonlyValue)
        });
        _jsContext["macroService"] = new JSObject(new List<JSProperty>()
        {
            new JSProperty("findPattern", new JSFunction((in Arguments a) =>
            {
                if (a.Length < 1) throw new ArgumentException("macroService.findPattern requires at least one argument");

                var jsPattern = a[0];
                var jsOptions = a[1];
                var pathToPatternNode = new Dictionary<string, PatternNode>();
                if (jsPattern is JSArray jsArray)
                {
                    var elements = jsArray.GetArrayElements().ToArray();
                    foreach (var elem in elements)
                    {
                        if (!_jsonValueToPatternNode.ContainsKey(elem))
                        {
                            _jsonValueToPatternNode.Add(elem, PatternNodeViewModel.FromJsonNode(JSJSON.Stringify(elem.value)));
                        }
                        var path = elem.value["properties"]["path"].ToString();
                        pathToPatternNode.Add(path, _jsonValueToPatternNode[elem]);
                    }
                }
                else
                {
                    if (!_jsonValueToPatternNode.ContainsKey(jsPattern))
                    {
                        _jsonValueToPatternNode.Add(jsPattern, PatternNodeViewModel.FromJsonNode(JSJSON.Stringify(jsPattern)));
                    }
                    var path = jsPattern["properties"]["path"].ToString();
                    pathToPatternNode.Add(path, _jsonValueToPatternNode[jsPattern]);
                }

                var limit = jsOptions["limit"].IsUndefined ? 1 : jsOptions["limit"].IntValue;
                var variancePct = jsOptions["variancePct"].IsUndefined ? 0.0 : jsOptions["variancePct"].DoubleValue;
                FindPatternResult result = null;
                var opts = new FindOptions() {
                    Limit = limit,
                    VariancePct = variancePct
                };

                var task = Task.Run<JSValue>(async () =>
                {
                    foreach (var patternNodeKvp in pathToPatternNode)
                    {
                        if (!_isRunning)
                        {
                            result = new FindPatternResult()
                            {
                                IsSuccess = false
                            };
                            break;
                        }

                        var path = patternNodeKvp.Key;
                        var patternNode = patternNodeKvp.Value;

                        _logger.LogDebug($"Find: {path}");
                        if (patternNode.IsMultiPattern)
                        {
                            var points = new List<Point>();
                            var multiResult = new FindPatternResult();
                            foreach (var pattern in patternNode.Patterns)
                            {
                                if (InDebugMode && pattern.Bounds != null)
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        _screenService.DebugRectangle(pattern.Bounds.Start, pattern.Bounds.End);
                                    });
                                }
                                var singleResult = await _screenService.FindPattern(pattern, opts);
                                if (singleResult.IsSuccess)
                                {
                                    points.AddRange(singleResult.Points);
                                }
                            }
                            multiResult.Points = points.ToArray();
                            multiResult.IsSuccess = points.Count > 0;
                            result = multiResult;
                        }
                        else
                        {
                            var pattern = patternNode.Patterns.First();
                            if (InDebugMode && pattern.Bounds != null)
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    _screenService.DebugRectangle(pattern.Bounds.Start, pattern.Bounds.End);
                                });
                            }
                            result = await _screenService.FindPattern(pattern, opts);
                        }

                        if (InDebugMode && result.IsSuccess)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                foreach (var point in result.Points)
                                {
                                    _screenService.DebugCircle(point);
                                }
                            });
                        }

                        if (result.IsSuccess)
                        {
                            result.Path = path;
                            break;
                        }
                    }

                    return new JSObject(new List<JSProperty>()
                    {
                        new JSProperty("path", new JSString(result.Path), JSPropertyAttributes.ReadonlyValue),
                        new JSProperty("isSuccess", result.IsSuccess ? JSBoolean.True : JSBoolean.False, JSPropertyAttributes.ReadonlyValue),
                        new JSProperty("point", new JSObject(new List<JSProperty>()
                        {
                            new JSProperty("x", new JSNumber(result.Point.X), JSPropertyAttributes.ReadonlyValue),
                            new JSProperty("y", new JSNumber(result.Point.Y), JSPropertyAttributes.ReadonlyValue)
                        }), JSPropertyAttributes.ReadonlyValue),
                        new JSProperty("points", result.Points == null ? JSNull.Value : new JSArray(result.Points.Select(p => new JSObject(new List<JSProperty>()
                        {
                            new JSProperty("x", new JSNumber(result.Point.X), JSPropertyAttributes.ReadonlyValue),
                            new JSProperty("y", new JSNumber(result.Point.Y), JSPropertyAttributes.ReadonlyValue)
                        }))), JSPropertyAttributes.ReadonlyValue)
                    });
                });

                return new JSPromise(task);
            }), JSPropertyAttributes.ReadonlyValue)
        });


        using var stream = FileSystem.OpenAppPackageFileAsync("initJavaScriptContext.js").Result;
        using var reader = new StreamReader(stream);
        var initScript = reader.ReadToEnd();
        await _jsContext.ExecuteAsync(initScript);
    }
}
