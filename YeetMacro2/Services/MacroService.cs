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
}

public class MacroService : IMacroService
{
    IScreenService _screenService;
    IToastService _toastService;
    public bool InDebugMode { get; set; }
    public MacroService(IScreenService screenService, IToastService toastService)
    {
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
            result = FindPattern(pattern, opts).Result;
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

        dynamicObject.clickPoint = new Action<Point, dynamic>((p, o) =>
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


                    if (predicatePattern != null && dynamicObject.findPattern(predicatePattern, predicateOpts).IsSuccess)
                    {
                        break;
                    }

                    if (inversePredicatePattern != null && !dynamicObject.findPattern(inversePredicatePattern, predicateOpts).IsSuccess && ++inversePatternNotFoundChecks >= predicateCheckSteps)
                    {
                        break;
                    }

                    if (i % predicateCheckSteps == 0)
                    {
                        inversePatternNotFoundChecks = 0;
                        Console.WriteLine("[*****YeetMacro*****] Find pattern: " + p.path);
                        result = dynamicObject.findPattern(p, o);
                        if (o.doClick && result.IsSuccess)
                        {
                            dynamicObject.clickPattern(p, o);
                        }
                        Thread.Sleep(intervalDelayms);

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
                    if (token.IsCancellationRequested)
                    {
                        return result;
                    }

                    if (touchPattern != null)
                    {
                        dynamicObject.clickPattern(touchPattern, o);
                    }

                    Console.WriteLine("[*****YeetMacro*****] Polling pattern: " + p.path);
                    result = dynamicObject.findPattern(p, o);

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