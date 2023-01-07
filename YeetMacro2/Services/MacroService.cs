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

public interface IMacroService
{
    dynamic BuildDynamicObject(CancellationToken token);
    Task<FindPatternResult> ClickPattern(PatternBase pattern);
    Task<FindPatternResult> FindPattern(PatternBase pattern);
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

    public async Task<FindPatternResult> FindPattern(PatternBase pattern)
    {
        try
        {
            var bounds = pattern.Bounds;
            var points = await _screenService.GetMatches(pattern);

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

    private FindPatternResult DynamicFindPattern(dynamic p)
    {
        FindPatternResult result;
        if (p.metadata.IsMultiPattern)
        {
            var points = new List<Point>();
            var multiResult = new FindPatternResult();
            foreach (PatternBase pattern in p.Patterns)
            {
                var singleResult = FindPattern(pattern).Result;
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
            result = FindPattern(pattern).Result;
        }

        result.Path = p.path;
        return result;
    }

    // TODO: pass cancelation token here for polling
    public dynamic BuildDynamicObject(CancellationToken token)
    {
        dynamic dynamicObject = new ExpandoObject();

        dynamicObject.findPattern = new Func<dynamic, FindPatternResult>((p) =>
        {
            if (p is Array)
            {
                foreach (var pattern in p)
                {
                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    var result = DynamicFindPattern(pattern);
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
                return DynamicFindPattern(p);
            }
        });

        dynamicObject.clickPattern = new Func<dynamic, FindPatternResult>((p) =>
        {
            var result = dynamicObject.findPattern(p);
            if (result.IsSuccess)
            {
                foreach (var point in result.Points)
                {
                    Console.WriteLine("[*****YeetMacro*****] Click pattern: " + p.path);
                    _screenService.DoClick((float)point.X, (float)point.Y);
                }
            }

            return result;
        });

        dynamicObject.pollPattern = new Func<dynamic, dynamic, FindPatternResult>((p, o) =>
        {
            var patternFound = false;
            dynamic result = null;

            if (o.predicatePattern != null)
            {
                dynamic predicateResult = null;

                var steps = 8;
                for (int i = 8; ; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        return result;
                    }

                    Console.WriteLine("[*****YeetMacro*****] Find predicate pattern: " + o.predicatePattern.path);
                    predicateResult = dynamicObject.findPattern(o.predicatePattern);
                    if (predicateResult.IsSuccess)
                    {
                        break;
                    }

                    if (i % steps == 0)
                    {
                        Console.WriteLine("[*****YeetMacro*****] Find pattern: " + p.path);
                        result = dynamicObject.findPattern(p);
                        if (o.doClick && result.IsSuccess)
                        {
                            dynamicObject.clickPattern(p);
                        }
                    }

                    Thread.Sleep(250);
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

                    Console.WriteLine("[*****YeetMacro*****] Polling pattern: " + p.path);
                    result = dynamicObject.findPattern(p);

                    patternFound = result.IsSuccess;
                    if (o.doClick && result.IsSuccess)
                    {
                        dynamicObject.clickPattern(p);
                    }

                    Thread.Sleep(1000);
                }
            }

            return result;
        });

        return dynamicObject;
    }
}