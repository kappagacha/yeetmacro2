using Microsoft.Extensions.Logging;
using YeetMacro2.ViewModels;
using YantraJS.Core;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Services;

public interface IScriptService
{
    bool InDebugMode { get; set; }
    void RunScript(string script, string jsonPatterns, string jsonOptions, Action onScriptFinished);
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

    public void RunScript(string script, string jsonPatterns, string jsonSettings, Action onScriptFinished)
    {
        if (_isRunning) return;

        Task.Run(async () =>
        {
            _isRunning = true;
            try
            {
                await _jsContext.ExecuteAsync($"patterns = {jsonPatterns}; settings = {jsonSettings}; resolvePath({{ $isParent: true, ...patterns }});");
                await _jsContext.ExecuteAsync(script);
                _toastService.Show(_isRunning ? "Script finished..." : "Script stopped...");
                _isRunning = false;
                _jsonValueToPatternNode.Clear();
                onScriptFinished?.Invoke();
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
            }), JSPropertyAttributes.EnumerableReadonlyValue),
            new JSProperty("debug", new JSFunction((in Arguments a) =>
            {
                _logger.LogDebug(a[0].ToString());
                return JSNull.Value;
            }), JSPropertyAttributes.EnumerableReadonlyValue)
        });
        _jsContext["screenService"] = new JSObject(new List<JSProperty>()
        {
            new JSProperty("debugRectangle", new JSFunction((in Arguments a) =>
            {
                var jsRect = a[0];
                var x = jsRect["x"].DoubleValue;
                var y = jsRect["y"].DoubleValue;
                var width = jsRect["width"].DoubleValue;
                var height = jsRect["height"].DoubleValue;
                var rect = new Rect(x, y, width, height);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _screenService.DebugRectangle(rect);
                });
                return JSNull.Value;
            }), JSPropertyAttributes.EnumerableReadonlyValue),
            new JSProperty("debugClear", new JSFunction((in Arguments a) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _screenService.DebugClear();
                });
                
                return JSNull.Value;
            }), JSPropertyAttributes.EnumerableReadonlyValue),
            new JSProperty("doClick", new JSFunction((in Arguments a) =>
            {
                var jsPoint = a[0];
                var x = jsPoint["x"].DoubleValue;
                var y = jsPoint["y"].DoubleValue;
                _screenService.DoClick(new Point(x, y));
                return JSNull.Value;
            }), JSPropertyAttributes.EnumerableReadonlyValue),
            new JSProperty("doSwipe", new JSFunction((in Arguments a) =>
            {
                var jsPointStart = a[0];
                var jsPointEnd = a[1];
                _screenService.DoSwipe(
                    new Point(jsPointStart["x"].DoubleValue, jsPointStart["y"].DoubleValue),
                    new Point(jsPointEnd["x"].DoubleValue, jsPointEnd["y"].DoubleValue));
                return JSNull.Value;
            }), JSPropertyAttributes.EnumerableReadonlyValue),
            new JSProperty("getText", new JSFunction((in Arguments a) =>
            {
                var jsPattern = a[0];
                var patternNode = ResolvePatterns(jsPattern).Values.First();

                if (patternNode?.Patterns?.FirstOrDefault() == null) return JSNull.Value;

                var task = Task.Run<JSValue>(async () =>
                {
                    var text = await _screenService.GetText(patternNode.Patterns.First());
                    return new JSString(text);
                });

                return new JSPromise(task);
            }), JSPropertyAttributes.EnumerableReadonlyValue)
        });
        _jsContext["macroService"] = new JSObject(new List<JSProperty>()
        {
            new JSProperty("findPattern", new JSFunction((in Arguments a) =>
            {
                if (a.Length < 1) throw new ArgumentException("macroService.findPattern requires at least one argument");

                var jsPattern = a[0];
                var jsOptions = a[1];
                var pathToPatternNode = ResolvePatterns(jsPattern);
                var limit = jsOptions == null || jsOptions["limit"].IsUndefined ? 1 : jsOptions["limit"].IntValue;
                var variancePct = jsOptions == null || jsOptions["variancePct"].IsUndefined ? 0.0 : jsOptions["variancePct"].DoubleValue;
                FindPatternResult result = null;
                var opts = new FindOptions() {
                    Limit = limit,
                    VariancePct = variancePct
                };

                var task = Task.Run<JSValue>(async () =>
                {
                    foreach (var patternNodeKvp in pathToPatternNode)
                    {
                        if (InDebugMode)
                        {
                            MainThread.BeginInvokeOnMainThread(_screenService.DebugClear);
                        }

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
                                if (InDebugMode && pattern.Rect != Rect.Zero)
                                {
                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        _screenService.DebugClear();
                                        _screenService.DebugRectangle(pattern.Rect);
                                    });
                                }
                                var singleResult = await _screenService.FindPattern(pattern, opts);
                                if (singleResult.IsSuccess)
                                {
                                    points.AddRange(singleResult.Points);
                                    if (opts.Limit == 1)
                                    {
                                        multiResult.Point = points[0];
                                    }
                                }
                            }
                            multiResult.Points = points.ToArray();
                            multiResult.IsSuccess = points.Count > 0;
                            result = multiResult;
                        }
                        else
                        {
                            var pattern = patternNode.Patterns.First();
                            if (InDebugMode && pattern.Rect != Rect.Zero)
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    _screenService.DebugRectangle(pattern.Rect);
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
                        new JSProperty("path", new JSString(result.Path), JSPropertyAttributes.EnumerableReadonlyValue),
                        new JSProperty("isSuccess", result.IsSuccess ? JSBoolean.True : JSBoolean.False, JSPropertyAttributes.EnumerableReadonlyValue),
                        new JSProperty("point", new JSObject(new List<JSProperty>()
                        {
                            new JSProperty("x", new JSNumber(result.Point.X), JSPropertyAttributes.EnumerableReadonlyValue),
                            new JSProperty("y", new JSNumber(result.Point.Y), JSPropertyAttributes.EnumerableReadonlyValue)
                        }), JSPropertyAttributes.EnumerableReadonlyValue),
                        new JSProperty("points", result.Points == null ? JSNull.Value : new JSArray(result.Points.Select(p => new JSObject(new List<JSProperty>()
                        {
                            new JSProperty("x", new JSNumber(p.X), JSPropertyAttributes.EnumerableReadonlyValue),
                            new JSProperty("y", new JSNumber(p.Y), JSPropertyAttributes.EnumerableReadonlyValue)
                        }))), JSPropertyAttributes.EnumerableReadonlyValue)
                    });
                });

                return new JSPromise(task);
            }), JSPropertyAttributes.EnumerableReadonlyValue)
        });

        var initScript = ServiceHelper.GetAssetContent("initJavaScriptContext.js");
        await _jsContext.ExecuteAsync(initScript);
    }

    private Dictionary<string, PatternNode> ResolvePatterns(JSValue jsPattern)
    {
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
                var path = elem.value["props"]["path"].ToString();
                pathToPatternNode.Add(path, _jsonValueToPatternNode[elem]);
            }
        }
        else
        {
            if (!_jsonValueToPatternNode.ContainsKey(jsPattern))
            {
                _jsonValueToPatternNode.Add(jsPattern, PatternNodeViewModel.FromJsonNode(JSJSON.Stringify(jsPattern)));
            }
            var path = jsPattern["props"]["path"].ToString();
            pathToPatternNode.Add(path, _jsonValueToPatternNode[jsPattern]);
        }

        return pathToPatternNode;
    }
}
