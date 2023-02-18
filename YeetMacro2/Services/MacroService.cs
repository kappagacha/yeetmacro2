
using Microsoft.Extensions.Logging;
using System.Dynamic;
using YeetMacro2.Data.Models;
using Point = Microsoft.Maui.Graphics.Point;

namespace YeetMacro2.Services;

public class FindPatternResult
{
    public bool IsSuccess { get; set; }
    public string Path { get; set; }
    public Point Point { get; set; }
    public Point[] Points { get; set; }
}

public class FindOptions
{
    public int Limit { get; set; }
    public double Threshold { get; set; }
}

public interface IMacroService
{
    dynamic BuildDynamicObject(CancellationToken token);
    Task<FindPatternResult> ClickPattern(PatternBase pattern);
    Task<FindPatternResult> FindPattern(PatternBase pattern, FindOptions opts);
    bool InDebugMode { get; set; }
}

public class MacroService : IMacroService
{
    ILogger _logger;
    IScreenService _screenService;
    IToastService _toastService;
    public bool InDebugMode { get; set; }
    public MacroService(ILogger<MacroService> logger, IScreenService screenService, IToastService toastService)
    {
        _logger = logger;
        _screenService = screenService;
        _toastService = toastService;
    }

    public async Task<FindPatternResult> FindPattern(PatternBase pattern, FindOptions opts = null)
    {
        try
        {
            var bounds = pattern.Bounds;
            var points = await _screenService.GetMatches(pattern, opts);

            var result = new FindPatternResult();
            result.IsSuccess = points.Count > 0;
            if (points.Count > 0)
            {
                result.Point = new Point(points[0].X, points[0].Y);
                result.Points = points.ToArray();
            }

            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    public async Task<FindPatternResult> ClickPattern(PatternBase pattern)
    {
        try
        {
            var result = await FindPattern(pattern);
            if (result.IsSuccess)
            {
                foreach (var point in result.Points)
                {
                    _screenService.DoClick((float)point.X, (float)point.Y);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private FindPatternResult DynamicFindPattern(dynamic p, dynamic o)
    {
        _logger.LogDebug($"Find: {p.path}");
        FindPatternResult result;

        FindOptions opts = new FindOptions() {
            Limit = DynamicHelper.GetProperty(o, "limit", 1),
            Threshold = DynamicHelper.GetProperty(o, "threshold", 0.0)
        };

        if (p.metadata.IsMultiPattern)
        {
            var points = new List<Point>();
            var multiResult = new FindPatternResult();
            foreach (PatternBase pattern in p.patterns)
            {
                if (InDebugMode && pattern.Bounds != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _screenService.DebugRectangle((int)pattern.Bounds.X, (int)pattern.Bounds.Y, (int)pattern.Bounds.W, (int)pattern.Bounds.H);
                    });
                }
                var singleResult = FindPattern(pattern, opts).Result;
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
            PatternBase pattern = (PatternBase)p.patterns[0];
            if (InDebugMode && pattern.Bounds != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _screenService.DebugRectangle((int)pattern.Bounds.X, (int)pattern.Bounds.Y, (int)pattern.Bounds.W, (int)pattern.Bounds.H);
                });
            }
            result = FindPattern(pattern, opts).Result;
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

        result.Path = p.path;
        return result;
    }

    public dynamic BuildDynamicObject(CancellationToken token)
    {
        dynamic dynamicObject = new ExpandoObject();

        dynamicObject.findPattern = new Func<dynamic, dynamic, FindPatternResult>((p, o) =>
        {
            if (p is Array)
            {
                foreach (var pattern in p)
                {
                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    var result = DynamicFindPattern(pattern, o);
                    if (result.IsSuccess)
                    {
                        return result;
                    }
                }
                return new FindPatternResult() { IsSuccess = false };
            }
            else
            {
                Console.WriteLine("[*****YeetMacro*****] Find pattern: " + p.path);
                return DynamicFindPattern(p, o);
            }
        });

        dynamicObject.clickPattern = new Func<dynamic, dynamic, FindPatternResult>((p, o) =>
        {
            dynamic clickOffsetX = DynamicHelper.GetProperty<int>(o, "clickOffsetX", 0);
            dynamic clickOffsetY = DynamicHelper.GetProperty<int>(o, "clickOffsetY", 0);

            var result = dynamicObject.findPattern(p, o);
            if (result.IsSuccess)
            {
                foreach (var point in result.Points)
                {
                    Console.WriteLine("[*****YeetMacro*****] Click pattern: " + p.path);
                    _screenService.DoClick((float)(point.X + clickOffsetX), (float)(point.Y + clickOffsetY));
                }
            }

            return result;
        });

        dynamicObject.clickPoint = new Action<Point>(p =>
        {
            _screenService.DoClick((float)p.X, (float)p.Y);
        });

        dynamicObject.pollPattern = new Func<dynamic, dynamic, FindPatternResult>((p, o) =>
        {
            var patternFound = false;
            dynamic result = null;
            dynamic intervalDelayms = DynamicHelper.GetProperty<int>(o, "intervalDelayms", 1000);
            dynamic predicatePattern = DynamicHelper.GetProperty(o, "predicatePattern", null);
            dynamic touchPattern = DynamicHelper.GetProperty(o, "touchPattern", null);
            dynamic inversePredicatePattern = DynamicHelper.GetProperty(o, "inversePredicatePattern", null);

            if (predicatePattern != null || inversePredicatePattern != null)
            {
                var inversePatternNotFoundChecks = 0;
                dynamic predicateCheckSteps = DynamicHelper.GetProperty<int>(o, "predicateCheckSteps", 8);
                dynamic inversePredicateCheckSteps = DynamicHelper.GetProperty<int>(o, "inversePredicateCheckSteps", 4);
                dynamic predicateCheckDelayms = DynamicHelper.GetProperty<int>(o, "predicateCheckDelayms", 250);
                dynamic predicateOpts = new ExpandoObject();
                predicateOpts.threshold = DynamicHelper.GetProperty(o, "predicateThreshold", 0.0);
                
                for (int i = predicateCheckSteps; ; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        return result;
                    }

                    Console.WriteLine("[*****YeetMacro*****] Find predicate pattern");

                    _screenService.DebugClear();

                    if (predicatePattern != null && dynamicObject.findPattern(predicatePattern, predicateOpts).IsSuccess)
                    {
                        break;
                    }

                    _screenService.DebugClear();

                    if (inversePredicatePattern != null && !dynamicObject.findPattern(inversePredicatePattern, predicateOpts).IsSuccess && ++inversePatternNotFoundChecks >= inversePredicateCheckSteps)
                    {
                        break;
                    }

                    _screenService.DebugClear();

                    if (i % inversePredicateCheckSteps == 0)
                    {
                        inversePatternNotFoundChecks = 0;
                    }

                    if (i % predicateCheckSteps == 0)
                    {
                        Console.WriteLine("[*****YeetMacro*****] Find pattern: " + p.path);
                        result = dynamicObject.findPattern(p, o);
                        if (o.doClick && result.IsSuccess)
                        {
                            dynamicObject.clickPoint(result.Point);
                        }
                        Thread.Sleep(intervalDelayms);

                        _screenService.DebugClear();

                        if (touchPattern != null)
                        {
                            dynamicObject.clickPattern(touchPattern, o);
                        }
                    }

                    Thread.Sleep(predicateCheckDelayms);
                }
            }
            else
            {
                while (!patternFound)
                {
                    _screenService.DebugClear();

                    if (token.IsCancellationRequested)
                    {
                        return result;
                    }

                    if (touchPattern != null)
                    {
                        dynamicObject.clickPattern(touchPattern, o);
                    }

                    _screenService.DebugClear();

                    Console.WriteLine("[*****YeetMacro*****] Polling pattern: " + p.path);
                    result = dynamicObject.findPattern(p, o);

                    _screenService.DebugClear();

                    patternFound = result.IsSuccess;
                    if (o.doClick && result.IsSuccess)
                    {
                        dynamicObject.clickPattern(p, o);
                    }

                    Thread.Sleep(intervalDelayms);
                }
            }

            return result;
        });

        return dynamicObject;
    }
}

public static class DynamicHelper
{
    public static object GetProperty(dynamic item, string propertyName, object defaultValue)
    {
        if (item is ExpandoObject eo && (eo as IDictionary<string, object>).ContainsKey(propertyName))
        {
            return (eo as IDictionary<string, object>)[propertyName];
        }

        return defaultValue;
    }

    public static T GetProperty<T>(dynamic item, string propertyName, T defaultValue)
    {
        if (item is ExpandoObject eo && (eo as IDictionary<string, object>).ContainsKey(propertyName)) {
            return (T)Convert.ChangeType((eo as IDictionary<string, object>)[propertyName], typeof(T));
        }

        return defaultValue;
    }
}