using Microsoft.Extensions.Logging;
using YeetMacro2.ViewModels;
using YeetMacro2.Data.Models;
using Jint;
using System.Dynamic;
using Jint.Runtime;

namespace YeetMacro2.Services;

public interface IScriptService
{
    bool InDebugMode { get; set; }
    void RunScript(string scriptToRun, IEnumerable<ScriptNode> scripts, string jsonPatterns, string jsonOptions, Action<string> onScriptFinished);
    void Stop();
}

public class ScriptService : IScriptService
{
    ILogger _logger;
    IScreenService _screenService;
    IToastService _toastService;
    bool _isRunning;
    Engine _engine;
    Random _random;
    Dictionary<string, PatternNode> _jsonValueToPatternNode;
    public bool InDebugMode { get; set; }
    CancellationToken _cancellationToken;

    public ScriptService(ILogger<ScriptService> logger, IScreenService screenService, IToastService toastService)
    {
        _logger = logger;
        _screenService = screenService;
        _toastService = toastService;
        _random = new Random();
        _jsonValueToPatternNode = new Dictionary<string, PatternNode>();
        _engine = new Engine();
        //_engine = new Engine(opts =>
        //{
        //    opts.CancellationToken(_cancellationToken);
        //});
        Task.Run(InitJSContext);
    }

    public void RunScript(string scriptToRun, IEnumerable<ScriptNode> scripts, string jsonPatterns, string jsonSettings, Action<string> onScriptFinished)
    {
        if (_isRunning) return;

        // https://github.com/sebastienros/jint
        Task.Run(() =>
        {
            _isRunning = true;
            try
            {
                foreach (var script in scripts)
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
                //_engine.SetValue("result", null);
                _engine.Execute($"patterns = {jsonPatterns}; settings = {jsonSettings}; resolvePath({{ $isParent: true, ...patterns }});");
                _engine.Execute($"{{\n{scriptToRun}\n}}");
                
                _toastService.Show(_isRunning ? "Script finished..." : "Script stopped...");
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
                else if (jsResult != Jint.Native.JsValue.Null && jsResult != Jint.Native.JsValue.Undefined)
                {
                    result = jsResult.ToString();
                }
                onScriptFinished?.Invoke(result);
                _isRunning = false;
                _jsonValueToPatternNode.Clear();
            }
        });
    }

    public void Stop()
    {
        _isRunning = false;
    }

    public void InitJSContext()
    {
        _engine.SetValue("sleep", new Action<int>((ms) => Thread.Sleep(ms)));

        dynamic state = new ExpandoObject();
        state.isRunning = new Func<bool>(() => { 
            return _isRunning;
        });
        _engine.SetValue("state", state);

        dynamic logger = new ExpandoObject();
        logger.info = new Action<string>((msg) => _logger.LogInformation(msg));
        logger.debug = new Action<string>((msg) => _logger.LogDebug(msg));
        _engine.SetValue("logger", logger);

        dynamic screenService = new ExpandoObject();
        screenService.debugRectangle = new Action<dynamic>((jsRect) =>
        {
            try
            {
                var x = jsRect["x"].DoubleValue;
                var y = jsRect["y"].DoubleValue;
                var width = jsRect["width"].DoubleValue;
                var height = jsRect["height"].DoubleValue;
                var rect = new Rect(x, y, width, height);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _screenService.DebugRectangle(rect);
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"debugRectangle failed: {ex.Message}");
                throw;
            }
        });
        screenService.debugClear = new Action(() =>
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _screenService.DebugClear();
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"debugClear failed: {ex.Message}");
                throw;
            }
        });
        screenService.doClick = new Action<dynamic>((jsPoint) =>
        {
            try
            {
                _screenService.DoClick(new Point(jsPoint.x, jsPoint.y));
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"doClick failed: {ex.Message}");
                throw;
            }
        });
        screenService.doSwipe = new Action<dynamic, dynamic>((jsPointStart, jsPointEnd) =>
        {
            try
            {
                _screenService.DoSwipe(
                    new Point(jsPointStart.x, jsPointStart.y),
                    new Point(jsPointEnd.x, jsPointEnd.y));
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"doClick failed: {ex.Message}");
                throw;
            }
        });
        screenService.getText = new Func<dynamic, string, string>((jsPattern, whiteList) =>
        {
            var patternNode = ((Dictionary<string, PatternNode>)ResolvePatterns(jsPattern)).Values.First();
            var maxTry = 10;
            if (patternNode?.Patterns.FirstOrDefault() == null) return string.Empty;

            var currentTry = 0;
            while (currentTry < maxTry)
            {
                try
                {
                    return _screenService.GetText(patternNode.Patterns.First(), new TextFindOptions() { Whitelist = whiteList }).Result;
                }
                catch
                {
                    Thread.Sleep(200);
                    currentTry++;
                }
            }
            _logger.LogDebug($"getText failed {maxTry} times...");
            return String.Empty;
        });
        _engine.SetValue("screenService", screenService);

        dynamic macroService = new ExpandoObject();
        macroService.calcOffset = new Func<dynamic, dynamic>((jsPattern) =>
        {
            //var patternNode = PatternNodeViewModel.FromJsonNode(JSJSON.Stringify(jsPattern));
            //foreach (var pattern in patternNode.Patterns)
            //{
            //    var offset = PatternNodeViewModel.CalcOffset(pattern);
            //    return new
            //    {
            //        x = offset.X,
            //        y = offset.Y
            //    };
            //}

            return new
            {
                x = 0,
                y = 0
            };
        });
        macroService.findPattern = new Func<dynamic, dynamic, dynamic>((jsPattern, jsOptions) =>
        {
            var pathToPatternNode = ResolvePatterns(jsPattern);
            var limit = GetValue(jsOptions, "limit", 1);
            var variancePct = GetValue(jsOptions, "variancePct", 0.0);

            FindPatternResult result = null;
            var opts = new FindOptions()
            {
                Limit = limit,
                VariancePct = variancePct
            };

            try
            {
                foreach (var patternNodeKvp in pathToPatternNode)
                {
                    if (InDebugMode)
                    {
                        MainThread.BeginInvokeOnMainThread(_screenService.DebugClear);
                        Thread.Sleep(50);
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
                                Thread.Sleep(50);
                            }

                            var singleResult = _screenService.FindPattern(pattern, opts).Result;
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
                        var pattern = patternNode.Patterns[0];
                        if (InDebugMode && pattern.Rect != Rect.Zero)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                _screenService.DebugRectangle(pattern.Rect);
                            });
                        }
                        result = _screenService.FindPattern(pattern, opts).Result;
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

                dynamic dResult = new ExpandoObject();
                dResult.path = result.Path;
                dResult.isSuccess = result.IsSuccess;
                dResult.point = new
                {
                    x = result.Point.X,
                    y = result.Point.Y
                };
                if (result.Points is not null && result.Points.Length > 0)
                {
                    dResult.points = result.Points.Select(p => new
                    {
                        x = p.X,
                        y = p.Y
                    });
                }
                return dResult;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"findPattern failed: {ex.Message}");
                throw;
            }
        });
        macroService.doClick = new Action<dynamic>(jsPoint =>
        {
            var variance = 3;
            var xVariance = _random.Next(-variance, variance);
            var yVariance = _random.Next(-variance, variance);
            screenService.doClick(new { x = jsPoint.x + xVariance, y = jsPoint.y + yVariance });
        });
        macroService.clickPattern = new Func<dynamic, dynamic, dynamic>((jsPattern, jsOptions) =>
        {
            var clickOffsetX = GetValue(jsOptions, "clickOffsetX", 0.0);
            var clickOffsetY = GetValue(jsOptions, "clickOffsetY", 0.0);
            var result = macroService.findPattern(jsPattern, jsOptions);
            if (result.isSuccess)
            {
                foreach (var point in result.points)
                {
                    macroService.doClick(new { x = point.x + clickOffsetX, y = point.y + clickOffsetY });
                }
            }

            Thread.Sleep(500);
            return result;
        });
        macroService.pollPattern = new Func<dynamic, dynamic, dynamic>((jsPattern, jsOptions) =>
        {
            dynamic result = new ExpandoObject();
            result.isSuccess = false;
            var intervalDelayMs = GetValue(jsOptions, "intervalDelayMs", 250);
            var predicatePattern = GetValue(jsOptions, "predicatePattern", null);
            var clickPattern = GetValue(jsOptions, "clickPattern", null);
            var inversePredicatePattern = GetValue(jsOptions, "inversePredicatePattern", null);
            var clickOffsetX = GetValue(jsOptions, "clickOffsetX", 0);
            var clickOffsetY = GetValue(jsOptions, "clickOffsetY", 0);

            if (inversePredicatePattern is not null)
            {
                var inversePredicateChecks = GetValue(jsOptions, "inversePredicateChecks", 5);
                var inversePredicateCheckDelayMs = GetValue(jsOptions, "inversePredicateCheckDelayMs", 100);
                var predicateOpts = new { threshold = GetValue(jsOptions, "predicateThreshold", 0.0) };
                while (_isRunning)
                {
                    var numChecks = 1;
                    var inversePredicateResult = macroService.findPattern(inversePredicatePattern, predicateOpts);
                    while (_isRunning && !inversePredicateResult.isSuccess && numChecks < inversePredicateChecks)
                    {
                        inversePredicateResult = macroService.findPattern(inversePredicatePattern, predicateOpts);
                        numChecks++;
                        Thread.Sleep((int)inversePredicateCheckDelayMs);
                    }
                    if (!inversePredicateResult.isSuccess)
                    {
                        result.inversePredicatePath = inversePredicateResult.path;
                        break;
                    }
                    result = macroService.findPattern(jsPattern, jsOptions);
                    if (GetValue(jsOptions, "doClick", false) && jsOptions.doClick && result.isSuccess)
                    {
                        var point = result.point;
                        macroService.doClick(new { x = point.x + clickOffsetX, y = point.y + clickOffsetY });
                        Thread.Sleep(500);
                    }
                    if (clickPattern is not null) macroService.clickPattern(clickPattern, jsOptions);
                    Thread.Sleep((int)intervalDelayMs);
                }
            }
            else if (predicatePattern is not null)
            {
                var predicateOpts = new { threshold = GetValue(jsOptions, "predicateThreshold", 0.0) };
                while (_isRunning)
                {
                    var predicateResult = macroService.findPattern(predicatePattern, predicateOpts);
                    if (predicateResult.isSuccess)
                    {
                        result.predicatePath = predicateResult.path;
                        break;
                    }
                    result = macroService.findPattern(jsPattern, jsOptions);
                    if (GetValue(jsOptions, "doClick", false) && result.isSuccess)
                    {
                        var point = result.point;
                        macroService.doClick(new { x = point.x + clickOffsetX, y = point.y + clickOffsetY });
                        Thread.Sleep(500);
                    }
                    if (clickPattern is not null) macroService.clickPattern(clickPattern, jsOptions);
                    Thread.Sleep((int)intervalDelayMs);
                }
            }
            else
            {
                while (_isRunning)
                {
                    result = macroService.findPattern(jsPattern, jsOptions);
                    if (GetValue(jsOptions, "doClick", false) && result.isSuccess)
                    {
                        var point = result.point;
                        macroService.doClick(new { x = point.x + clickOffsetX, y = point.y + clickOffsetY });
                        Thread.Sleep(500);
                    }
                    if (result.isSuccess) break;
                    if (clickPattern is not null) macroService.clickPattern(clickPattern, jsOptions);
                    Thread.Sleep((int)intervalDelayMs);
                }
            }

            return result;
        });

        _engine.SetValue("macroService", macroService);

        var initScript = ServiceHelper.GetAssetContent("initJavaScriptContext.js");
        _engine.Execute(initScript);
    }

    static dynamic GetValue(dynamic obj, string key, dynamic defaultValue)
    {
        if (obj is null) return defaultValue;
        if (obj is IDictionary<string, object> dict && dict.ContainsKey(key)) return dict[key];
        return defaultValue;
    }

    private Dictionary<string, PatternNode> ResolvePatterns(dynamic jsPattern)
    {
        var pathToPatternNode = new Dictionary<string, PatternNode>();
        if (jsPattern is dynamic[] jsArray)
        {
            foreach (var elem in jsArray)
            {
                if (!_jsonValueToPatternNode.ContainsKey(elem.props.path))
                {
                    var patternNode = PatternNodeViewModel.FromJsonNode(System.Text.Json.JsonSerializer.Serialize(elem));
                    foreach (var pattern in patternNode.Patterns)
                    {
                        var offset = PatternNodeViewModel.CalcOffset(pattern);
                        if (offset != Point.Zero) pattern.Rect = pattern.Rect.Offset(offset);
                    }
                    _jsonValueToPatternNode.Add(elem.props.path, patternNode);
                }
                var path = elem.props.path.ToString();
                pathToPatternNode.Add(path, _jsonValueToPatternNode[elem.props.path]);
            }
        }
        else
        {
            if (!_jsonValueToPatternNode.ContainsKey(jsPattern.props.path))
            {
                var patternNode = PatternNodeViewModel.FromJsonNode(System.Text.Json.JsonSerializer.Serialize(jsPattern));
                foreach (var pattern in patternNode.Patterns)
                {
                    var offset = PatternNodeViewModel.CalcOffset(pattern);
                    if (offset != Point.Zero) pattern.Rect = pattern.Rect.Offset(offset);
                }
                _jsonValueToPatternNode.Add(jsPattern.props.path, patternNode);
            }
            var path = jsPattern.props.path.ToString();
            pathToPatternNode.Add(path, _jsonValueToPatternNode[jsPattern.props.path]);
        }

        return pathToPatternNode;
    }
}
