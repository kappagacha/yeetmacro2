using Microsoft.Extensions.Logging;
using OneOf;
using System.Text.Json.Serialization;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Services;

public class PollPatternFindOptions : FindOptions
{
    public int IntervalDelayMs { get; set; } = 1_000;
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? PredicatePattern { get; set; }
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? ClickPattern { get; set; }
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? InversePredicatePattern { get; set; }
    public int InversePredicateChecks { get; set; } = 5;
    public int InversePredicateCheckDelayMs { get; set; } = 100;
    public double PredicateThreshold { get; set; } = 0.0;
    public bool DoClick { get; set; }
}

public class MacroService
{
    ILogger _logger;
    IScreenService _screenService;
    Dictionary<string, PatternNode> _jsonValueToPatternNode;
    Random _random;
    public bool InDebugMode { get; set; }
    public bool IsRunning { get; set; }

    public MacroService(ILogger<MacroService> logger, IScreenService screenService)
    {
        _logger = logger;
        _screenService = screenService;
        _jsonValueToPatternNode = new Dictionary<string, PatternNode>();
        _random = new Random();
    }

    public Point CalcOffset(PatternNode patternNode)
    {
        foreach (var pattern in patternNode.Patterns)
        {
            return PatternNodeManagerViewModel.CalcOffset(pattern);
        }

        return Point.Zero;
    }

    public FindPatternResult FindPattern(OneOf<PatternNode, PatternNode[]> oneOfPattern, FindOptions opts = null)
    {
        if (opts is null) opts = new FindOptions();

        var pathToPatternNode = ResolvePatterns(oneOfPattern);
        FindPatternResult result = null;

        try
        {
            foreach (var patternNodeKvp in pathToPatternNode)
            {
                if (InDebugMode)
                {
                    MainThread.BeginInvokeOnMainThread(_screenService.DebugClear);
                    Thread.Sleep(50);
                }

                if (!IsRunning)
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
                    var pattern = patternNode.Patterns.First();
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

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"findPattern failed: {ex.Message}");
            throw;
        }
    }

    public void DoClick(Point point)
    {
        var variance = 3;
        var xVariance = _random.Next(-variance, variance);
        var yVariance = _random.Next(-variance, variance);
        _screenService.DoClick(point.Offset(xVariance, yVariance));
    }

    public FindPatternResult ClickPattern(OneOf<PatternNode, PatternNode[]> oneOfPattern, FindOptions opts = null)
    {
        if (opts is null) opts = new FindOptions();

        FindPatternResult result = this.FindPattern(oneOfPattern, opts);
        if (result.IsSuccess)
        {
            foreach (var point in result.Points)
            {
                this.DoClick(point.Offset(opts.Offset.X, opts.Offset.Y));
            }
        }

        Thread.Sleep(500);
        return result;
    }

    public FindPatternResult PollPattern(OneOf<PatternNode, PatternNode[]> oneOfPattern, PollPatternFindOptions opts = null)
    {
        if (opts is null) opts = new PollPatternFindOptions();

        FindPatternResult result = new FindPatternResult() { IsSuccess = false };
        var intervalDelayMs = opts.IntervalDelayMs;
        var predicatePattern = opts.PredicatePattern;
        var clickPattern = opts.ClickPattern;
        var inversePredicatePattern = opts.InversePredicatePattern;
        var clickOffsetX = opts.Offset.X;
        var clickOffsetY = opts.Offset.Y;

        if (inversePredicatePattern is not null)
        {
            var inversePredicateChecks = opts.InversePredicateChecks;
            var inversePredicateCheckDelayMs = opts.InversePredicateCheckDelayMs;
            var predicateOpts = new FindOptions() { VariancePct = opts.PredicateThreshold };

            while (IsRunning)
            {
                var numChecks = 1;
                FindPatternResult inversePredicateResult = this.FindPattern(inversePredicatePattern.Value, predicateOpts);
                while (IsRunning && !inversePredicateResult.IsSuccess && numChecks < inversePredicateChecks)
                {
                    inversePredicateResult = this.FindPattern(inversePredicatePattern.Value, predicateOpts);
                    numChecks++;
                    Thread.Sleep(inversePredicateCheckDelayMs);
                }
                if (!inversePredicateResult.IsSuccess)
                {
                    result.InversePredicatePath = inversePredicateResult.Path;
                    break;
                }
                result = this.FindPattern(oneOfPattern, opts);
                if (opts.DoClick && result.IsSuccess)
                {
                    var point = result.Point;
                    this.DoClick(point.Offset(clickOffsetX, clickOffsetY));
                    Thread.Sleep(500);
                }
                if (clickPattern is not null) this.ClickPattern(clickPattern.Value, opts);
                Thread.Sleep(intervalDelayMs);
            }
        }
        else if (predicatePattern is not null)
        {
            var predicateOpts = new FindOptions() { VariancePct = opts.PredicateThreshold };
            while (IsRunning)
            {
                FindPatternResult predicateResult = this.FindPattern(predicatePattern.Value, predicateOpts);
                if (predicateResult.IsSuccess)
                {
                    result.PredicatePath = predicateResult.Path;
                    break;
                }
                result = this.FindPattern(oneOfPattern, opts);
                if (opts.DoClick && result.IsSuccess)
                {
                    var point = result.Point;
                    this.DoClick(point.Offset(clickOffsetX, clickOffsetY));
                    Thread.Sleep(500);
                }
                if (clickPattern is not null) this.ClickPattern(clickPattern.Value, opts);
                Thread.Sleep(intervalDelayMs);
            }
        }
        else
        {
            while (IsRunning)
            {
                result = this.FindPattern(oneOfPattern, opts);
                if (opts.DoClick && result.IsSuccess)
                {
                    var point = result.Point;
                    this.DoClick(point.Offset(clickOffsetX, clickOffsetY));
                    Thread.Sleep(500);
                }
                if (result.IsSuccess) break;
                if (clickPattern is not null) this.ClickPattern(clickPattern.Value, opts);
                Thread.Sleep(intervalDelayMs);
            }
        }

        return result;
    }

    public void DebugRectangle(Rect rect)
    {
        try
        {
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
    }

    public void DebugClear()
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
    }

    public void DoSwipe(Point start, Point end)
    {
        try
        {
            _screenService.DoSwipe(start, end);
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"DoSwipe failed: {ex.Message}");
            throw;
        }
    }

    public void Reset()
    {
        _jsonValueToPatternNode.Clear();
    }

    public string GetText(OneOf<PatternNode, PatternNode[]> oneOfPattern, string whiteList = "")
    {
        var patternNode = ResolvePatterns(oneOfPattern).Values.First();
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
    }

    private Dictionary<string, PatternNode> ResolvePatterns(OneOf<PatternNode, PatternNode[]> oneOfPattern)
    {
        var pathToPatternNode = new Dictionary<string, PatternNode>();
        if (oneOfPattern.Value is PatternNode[] patternNodes)
        {
            foreach (var patternNode in patternNodes)
            {
                if (!_jsonValueToPatternNode.ContainsKey(patternNode.Path))
                {
                    foreach (var pattern in patternNode.Patterns)
                    {
                        var offset = PatternNodeManagerViewModel.CalcOffset(pattern);
                        if (offset != Point.Zero) pattern.Rect = pattern.Rect.Offset(offset);
                    }
                    _jsonValueToPatternNode.Add(patternNode.Path, patternNode);
                }
                pathToPatternNode.Add(patternNode.Path, _jsonValueToPatternNode[patternNode.Path]);
            }
        }
        else
        {
            var patternNode = oneOfPattern.Value as PatternNode;
            if (!_jsonValueToPatternNode.ContainsKey(patternNode.Path))
            {
                foreach (var pattern in patternNode.Patterns)
                {
                    var offset = PatternNodeManagerViewModel.CalcOffset(pattern);
                    if (offset != Point.Zero) pattern.Rect = pattern.Rect.Offset(offset);
                }
                _jsonValueToPatternNode.Add(patternNode.Path, patternNode);
            }
            pathToPatternNode.Add(patternNode.Path, _jsonValueToPatternNode[patternNode.Path]);
        }

        return pathToPatternNode;
    }
}
