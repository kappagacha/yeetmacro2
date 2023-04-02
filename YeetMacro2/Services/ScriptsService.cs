using Microsoft.Extensions.Logging;
using YeetMacro2.ViewModels;
using YantraJS.Core;

namespace YeetMacro2.Services;

public interface IScriptsService
{
    bool InDebugMode { get; set; }
    void RunScript(string script);
    void Stop();
}

public class ScriptsService : IScriptsService
{
    ILogger _logger;
    MacroManagerViewModel _macroManagerViewModel;
    LogViewModel _logViewModel;
    IMacroService _macroService;
    IScreenService _screenService;
    IToastService _toastService;
    bool _isRunning;

    public bool InDebugMode { get; set; }

    public ScriptsService(ILogger<ScriptsService> logger, MacroManagerViewModel macroManagerViewModel,
        LogViewModel logViewModel, IMacroService macroService, IScreenService screenService, IToastService toastService)
    {
        _logger = logger;
        _macroManagerViewModel = macroManagerViewModel;
        _logViewModel = logViewModel;
        _macroService = macroService;
        _screenService = screenService;
        _toastService = toastService;
    }

    public void RunScript(string script)
    {
        if (_isRunning) return;

        Task.Run(async () =>
        {
            _isRunning = true;
            try
            {
                var jsContext = await InitJSContext();
                await jsContext.ExecuteAsync(@"
const loopPatterns = [patterns.titles.home, patterns.titles.quest];
while(state.isRunning) {
    const result = await macroService.findPattern(patterns.titles.home);
    if (result.isSuccess) {
        logger.logInfo(result.path);
    }
    await sleep(1_000);
}
                ");

                _isRunning = false;

                //var jsValue = await jsContext.ExecuteAsync("[patterns.titles]");
                //if (jsValue is JSArray array)
                //{
                //    var x = jsValue[0];
                //    var z = jsValue[1];
                //    var elements = array.GetArrayElements();
                //    foreach (var element in elements )
                //    {
                //        var something = _macroManagerViewModel.Patterns.FromJsonNode(JSJSON.Stringify(element.value));
                //    }
                //}
                //var titles = _macroManagerViewModel.Patterns.FromJsonNode(JSJSON.Stringify(jsValueTitles));


                //await context.ExecuteAsync("await sleep(1000);");
                //await context.ExecuteAsync("await macroService.clickPattern({bounds: { x: 1, y: 2, w: 3, h: 4}});");
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

    public async Task<JSContext> InitJSContext()
    {
        var synchronizationContext = new SynchronizationContext();
        var jsContext = new JSContext(synchronizationContext);
        var patterns = _macroManagerViewModel.Patterns;
        await patterns.WaitForInitialization();

        jsContext["state"] = new JSObject(new List<JSProperty>()
        {
            new JSProperty("isRunning", new JSFunction((in Arguments a) =>
            {
                return _isRunning ? JSBoolean.True : JSBoolean.False;
            }), JSPropertyAttributes.ReadonlyProperty)
        });
        jsContext["logger"] = new JSObject(new List<JSProperty>()
        {
            new JSProperty("logInfo", new JSFunction((in Arguments a) =>
            {
                _logger.LogInformation(a[0].ToString());
                return JSNull.Value;
            }), JSPropertyAttributes.ReadonlyValue),
            new JSProperty("logDebug", new JSFunction((in Arguments a) =>
            {
                _logger.LogDebug(a[0].ToString());
                return JSNull.Value;
            }), JSPropertyAttributes.ReadonlyValue)
        });
        jsContext["macroService"] = new JSObject(new List<JSProperty>()
        {
            new JSProperty("findPattern", new JSFunction((in Arguments a) =>
            {
                // TODO: check if there is at least one argument
                var jsPattern = a[0];
                var path = jsPattern["properties"]["path"].ToString();
                _logger.LogDebug($"Find: {path}");
                var patternNode = patterns.FromJsonNode(JSJSON.Stringify(jsPattern));
                FindPatternResult result;
                // TODO: get from second argument

                var opts = new FindOptions() {
                    Limit = 1,
                    VariancePct = 0.0
                };
                var task = Task.Run<JSValue>(async () =>
                {
                    try
                    {
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
                                    _screenService.DebugRectangle((int)pattern.Bounds.X, (int)pattern.Bounds.Y, (int)pattern.Bounds.W, (int)pattern.Bounds.H);
                                });
                            }
                            var singleResult = await _macroService.FindPattern(pattern, opts);
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
                                _screenService.DebugRectangle((int)pattern.Bounds.X, (int)pattern.Bounds.Y, (int)pattern.Bounds.W, (int)pattern.Bounds.H);
                            });
                        }
                        result = await _macroService.FindPattern(pattern, opts);
                    }

                    if (InDebugMode && result.IsSuccess)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            foreach (var point in result.Points)
                            {
                                _screenService.DebugCircle((int)point.X, (int)point.Y);
                            }
                        });
                    }

                    return new JSObject(new List<JSProperty>()
                    {
                        new JSProperty("path", new JSString(path), JSPropertyAttributes.ReadonlyValue),
                        new JSProperty("isSuccess", result.IsSuccess ? JSBoolean.True : JSBoolean.False, JSPropertyAttributes.ReadonlyValue),
                        new JSProperty("point", new JSObject(new List<JSProperty>()
                        {
                            new JSProperty("x", new JSNumber(result.Point.X), JSPropertyAttributes.ReadonlyValue),
                            new JSProperty("y", new JSNumber(result.Point.X), JSPropertyAttributes.ReadonlyValue)
                        }), JSPropertyAttributes.ReadonlyValue),
                        new JSProperty("points", result.Points == null ? JSNull.Value : new JSArray(result.Points.Select(p => new JSObject(new List<JSProperty>()
                        {
                            new JSProperty("x", new JSNumber(result.Point.X), JSPropertyAttributes.ReadonlyValue),
                            new JSProperty("y", new JSNumber(result.Point.X), JSPropertyAttributes.ReadonlyValue)
                        }))), JSPropertyAttributes.ReadonlyValue)
                     });
                    } 
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                });

                return new JSPromise(task);
            }), JSPropertyAttributes.ReadonlyValue)
        });

        await jsContext.ExecuteAsync(@"
function resolvePath(node, path = '') {
    let currentPath = path;
    if (node.properties) {
        currentPath = `${path ? path + '.' : ''}` + node.properties.name;
        node.properties.path = currentPath;
    }
    if (node.$isParent) {
        for (const [key, value] of Object.entries(node)) {
            if (['properties', '$isParent'].includes(key)) continue;
            resolvePath(value, currentPath);
        }
    }
}");
        await jsContext.ExecuteAsync($"const patterns = {patterns.ToJson()};");
        await jsContext.ExecuteAsync("resolvePath({ $isParent: true, ...patterns });");
        await jsContext.ExecuteAsync("const sleep = ms => new Promise(r => setTimeout(r, ms));");

        return jsContext;
    }

    //public async Task<Engine> CreateEngine()
    //{
    //    if (_cancellationTokenSource != null)
    //    {
    //        _cancellationTokenSource.Dispose();
    //    }
    //    _cancellationTokenSource = new CancellationTokenSource();

    //    var treeViewViewModel = _macroManagerViewModel.Patterns;
    //    //var scripts = _macroManagerViewModel.Scripts.Scripts;
    //    await treeViewViewModel.WaitForInitialization();
    //    var patterns = treeViewViewModel.Root.BuildDynamicObject();

    //    var engine = new Engine(opt => opt.CancellationToken(_cancellationTokenSource.Token))
    //            .SetValue("log", new Action<string>((msg) => _logger.LogInformation(msg)))
    //            .SetValue("sleep", new Action<int>((ms) => Thread.Sleep(ms)))
    //            .SetValue("patterns", patterns)
    //            .SetValue("macroService", _macroService.BuildDynamicObject(_cancellationTokenSource.Token));

    //    //foreach (var script in scripts)
    //    //{
    //    //    var func = script.Text.StartsWith("function") ? script.Text : 
    //    //        $@"function {script.Name}() {{
    //    //            {script.Text}
    //    //        }}";

    //    //    engine.Execute(func);
    //    //}

    //    return engine;
    //}
}
