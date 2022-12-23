using System.Dynamic;
using YeetMacro2.Data.Models;
using Point = Android.Graphics.Point;

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
    dynamic BuildDynamicObject();
    Task<FindPatternResult> ClickPattern(PatternBase pattern);
    Task<FindPatternResult> FindPattern(PatternBase pattern);
}

public class MacroService : IMacroService
{
    IWindowManagerService _windowManagerService;
    IMediaProjectionService _projectionService;
    IToastService _toastService;
    IAccessibilityService _accessibilityService;
    public bool InDebugMode { get; set; }
    public MacroService(IWindowManagerService windowManagerService, IMediaProjectionService projectionService,
        IToastService toastService, IAccessibilityService accessibilityService)
    {
        _windowManagerService = windowManagerService;
        _projectionService = projectionService;
        _toastService = toastService;
        _accessibilityService = accessibilityService;
    }

    public async Task<FindPatternResult> FindPattern(PatternBase pattern)
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] FindPattern");
            var bounds = pattern.Bounds;
            Console.WriteLine("[*****YeetMacro*****] FindPattern GetMatches Start");
            var points = await _windowManagerService.GetMatches(pattern);
            Console.WriteLine("[*****YeetMacro*****] FindPattern GetMatches End");

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
                    _accessibilityService.DoClick((float)point.X, (float)point.Y);
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
        Console.WriteLine("[*****YeetMacro*****] DynamicFindPattern");
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

        Console.WriteLine("[*****YeetMacro*****] DynamicFindPattern Done");
        result.Path = p.path;
        return result;
    }

    public dynamic BuildDynamicObject()
    {
        dynamic dynamicObject = new ExpandoObject();
        dynamicObject.FindPattern = new Func<dynamic, FindPatternResult>((p) => {
            Console.WriteLine("[*****YeetMacro*****] BuildDynamicObject FindPattern");
            if (p is Array)
            {
                Console.WriteLine("[*****YeetMacro*****] BuildDynamicObject FindPattern Arrays");
                foreach (var pattern in p)
                {
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
                Console.WriteLine("[*****YeetMacro*****] BuildDynamicObject FindPattern");
                return DynamicFindPattern(p);
            }
        });

        dynamicObject.ClickPattern = new Func<dynamic, FindPatternResult>((p) => {
            Console.WriteLine("[*****YeetMacro*****] BuildDynamicObject ClickPattern");
            var result = dynamicObject.FindPattern(p);
            if (result.IsSuccess)
            {
                foreach (var point in result.Points)
                {
                    Console.WriteLine("[*****YeetMacro*****] BuildDynamicObject DoClick Start");
                    _accessibilityService.DoClick((float)point.X, (float)point.Y);
                    Console.WriteLine("[*****YeetMacro*****] BuildDynamicObject DoClick End");
                }
            }

            return result;
        });

        return dynamicObject;
    }
}