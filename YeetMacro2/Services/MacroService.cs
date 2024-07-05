using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OneOf;
using System.Text.Json.Serialization;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels.NodeViewModels;
using YeetMacro2.ViewModels;

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
    public double TimeoutMs { get; set; } = 0.0;
    public Point ClickOffset { get; set; } = Point.Zero;
}

public class SwipePollPatternFindOptions : FindOptions
{
    public int PollTimeoutMs { get; set; } = 2_000;
    public int SwipeDelayMs { get; set; } = 5_00;
    public int MaxSwipes { get; set; } = 5;
    public Point Start { get; set; }
    public Point End { get; set; }
}

public class CloneOptions
{
    public double X { get; set; }
    public double Y { get; set; }
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Padding { get; set; }
    public double Scale { get; set; } = 1.0;
    public string Path { get; set; }
    public OffsetCalcType OffsetCalcType { get; set; } = OffsetCalcType.Default;
}

public class MacroService
{
    readonly ILogger _logger;
    readonly IScreenService _screenService;
    readonly Dictionary<string, Point> _pathToOffset;
    readonly Random _random;
    public bool InDebugMode { get; set; }
    public bool IsRunning { get; set; }

    public MacroService(ILogger<MacroService> logger, IScreenService screenService)
    {
        _logger = logger;
        _screenService = screenService;
        _pathToOffset = [];
        _random = new Random();

        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, string>(this, nameof(MacroManagerViewModel), (r, propertyChangedMessage) =>
        {
            if (propertyChangedMessage.PropertyName == nameof(MacroManagerViewModel.InDebugMode))
            {
                InDebugMode = propertyChangedMessage.NewValue;
            }
        });
    }

    public void Sleep(int ms)
    {
        Thread.Sleep(ms);
    }

    public Point CalcOffset(string path, Pattern pattern)
    {
        if (pattern.IsLocationDynamic)
        {
            return PatternNodeManagerViewModel.CalcOffset(pattern, _screenService.Resolution, _screenService.GetTopLeft());
        }

        if (!_pathToOffset.ContainsKey(path))
        {
            _pathToOffset[path] = PatternNodeManagerViewModel.CalcOffset(pattern, _screenService.CalcResolution, _screenService.GetTopLeft());
        }

        return _pathToOffset[path];
    }

    public Size GetCurrentResolution()
    {
        return _screenService.Resolution;
    }

    public double GetScreenDensity()
    {
        return _screenService.Density;
    }

    public Point GetTopLeft()
    {
        return _screenService.GetTopLeft();
    }

    public PatternNode ClonePattern(PatternNode patternNode, CloneOptions opts)
    {
        var clone = PatternNodeManagerViewModel.CloneNode(patternNode);
        if (!string.IsNullOrEmpty(opts.Path)) clone.Path = opts.Path;

        foreach (var pattern in clone.Patterns)
        {
            var rect = pattern.Rect;
            var size = new Size(
                (opts.Width == 0 ? rect.Width : opts.Width) + opts.Padding,
                (opts.Height == 0 ? rect.Height : opts.Height) + opts.Padding
            );

            var calcX = rect.X;
            if (opts.CenterX != 0) calcX = opts.CenterX - (size.Width / 2.0);
            if (opts.X != 0) calcX = opts.X;

            var calcY = rect.Y;
            if (opts.CenterY != 0) calcY = opts.CenterY - (size.Height / 2.0);
            if (opts.Y != 0) calcY = opts.Y;

            var location = new Point(calcX, calcY);

            pattern.Rect = new Rect(location, size);
            if (opts.OffsetCalcType != OffsetCalcType.Default)
            {
                pattern.OffsetCalcType = opts.OffsetCalcType;
            }
        }

        if (opts.Scale != 1.0)
        {
            clone.Patterns = clone.Patterns.Select(
                p => PatternNodeManagerViewModel.GetScaled(_screenService, p, opts.Scale)).ToList();
        }

        return clone;
    }

    public FindPatternResult FindPattern(OneOf<PatternNode, PatternNode[]> oneOfPattern, FindOptions opts = null)
    {
        opts ??= new FindOptions();

        var result = new FindPatternResult() { IsSuccess = false };
        PatternNode[] patternNodes;

        if (oneOfPattern.IsT1)
        {
            patternNodes = oneOfPattern.AsT1;
        }
        else
        {
            patternNodes = [oneOfPattern.AsT0];
        }

        try
        {
            foreach (var patternNode in patternNodes)
            {
                if (InDebugMode)
                {
                    MainThread.BeginInvokeOnMainThread(_screenService.DebugClear);
                }
                Sleep(50);

                if (!IsRunning) break;

                var path = patternNode.Path;
                _logger.LogDebug($"Find: {path}");

                if (patternNode.IsMultiPattern)
                {
                    var points = new List<Point>();
                    var multiResult = new FindPatternResult();
                    var idx = 0;

                    foreach (var pattern in patternNode.Patterns)
                    {
                        var offset = CalcOffset($"{patternNode.Path}_{idx}", pattern);
                        var optsWithOffset = new FindOptions()
                        {
                            Limit = opts.Limit,
                            VariancePct = opts.VariancePct,
                            Offset = opts.Offset.Offset(offset.X, offset.Y)
                        };

                        if (InDebugMode && pattern.Rect != Rect.Zero)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                _screenService.DebugClear();
                                _screenService.DebugRectangle(pattern.Rect.Offset(offset));
                            });
                        }
                        Sleep(50);
                        var singleResult = _screenService.FindPattern(pattern, optsWithOffset);
                        if (singleResult.IsSuccess)
                        {
                            points.AddRange(singleResult.Points);
                        }

                        if (points.Count >= opts.Limit)
                        {
                            break;
                        }
                    }
                    multiResult.IsSuccess = points.Count > 0;
                    multiResult.Points = points.Take(opts.Limit).ToArray();
                    if (multiResult.IsSuccess && opts.Limit >= 1)
                    {
                        multiResult.Point = points[0];
                    }
                    result = multiResult;
                    idx++;
                }
                else
                {
                    var pattern = patternNode.Patterns.FirstOrDefault();
                    if (pattern is null) return result;

                    var offset = CalcOffset(patternNode.Path, pattern);
                    var optsWithOffset = new FindOptions()
                    {
                        Limit = opts.Limit,
                        VariancePct = opts.VariancePct,
                        Offset = opts.Offset.Offset(offset.X, offset.Y)
                    };

                    if (InDebugMode && pattern.Rect != Rect.Zero)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            _screenService.DebugRectangle(pattern.Rect.Offset(offset));
                        });
                    }
                    result = _screenService.FindPattern(pattern, optsWithOffset);
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
        var variance = 5;
        var xVariance = _random.Next(-variance, variance);
        var yVariance = _random.Next(-variance, variance);
        _screenService.DoClick(point.Offset(xVariance, yVariance));
    }

    public FindPatternResult ClickPattern(OneOf<PatternNode, PatternNode[]> oneOfPattern, FindOptions opts = null)
    {
        opts ??= new FindOptions();

        FindPatternResult result = this.FindPattern(oneOfPattern, opts);
        if (result.IsSuccess)
        {
            foreach (var point in result.Points)
            {
                this.DoClick(point.Offset(opts.Offset.X, opts.Offset.Y));
            }
        }

        Sleep(500);
        return result;
    }

    public FindPatternResult PollPoint(Point point, PollPatternFindOptions opts = null)
    {
        opts ??= new PollPatternFindOptions();
        opts.DoClick = true;

        var patternNode = new PatternNode()
        {
            Path = point.ToString(),
            Patterns = [
                    new Pattern()
                    {
                        IsBoundsPattern = true,
                        Rect = new Rect(point, Size.Zero),
                        Resolution = _screenService.CalcResolution,
                        OffsetCalcType = OffsetCalcType.None
                    }
                ]
        };

        return PollPattern(patternNode, opts);
    }

    public FindPatternResult PollPattern(OneOf<PatternNode, PatternNode[]> oneOfPattern, PollPatternFindOptions opts = null)
    {
        opts ??= new PollPatternFindOptions();

        var result = new FindPatternResult() { IsSuccess = false };
        var intervalDelayMs = opts.IntervalDelayMs;
        var predicatePattern = opts.PredicatePattern;
        var clickPattern = opts.ClickPattern;
        var inversePredicatePattern = opts.InversePredicatePattern;
        var clickOffsetX = opts.ClickOffset.X;
        var clickOffsetY = opts.ClickOffset.Y;
        var hasTimeout = opts.TimeoutMs > 0;
        var timeout = hasTimeout ? DateTime.Now.AddMilliseconds(opts.TimeoutMs) : DateTime.MaxValue;

        if (inversePredicatePattern is not null)
        {
            var inversePredicateChecks = opts.InversePredicateChecks;
            var inversePredicateCheckDelayMs = opts.InversePredicateCheckDelayMs;
            var predicateOpts = new FindOptions() { VariancePct = opts.PredicateThreshold };
            FindPatternResult successResult = new() { IsSuccess = false };

            while (IsRunning)
            {
                if (hasTimeout && DateTime.Now > timeout) break;

                var numChecks = 1;
                FindPatternResult inversePredicateResult = this.FindPattern(inversePredicatePattern.Value, predicateOpts);
                while (IsRunning && !inversePredicateResult.IsSuccess && numChecks < inversePredicateChecks)
                {
                    inversePredicateResult = this.FindPattern(inversePredicatePattern.Value, predicateOpts);
                    numChecks++;
                    Sleep(inversePredicateCheckDelayMs);
                }
                if (!inversePredicateResult.IsSuccess)
                {
                    result.InversePredicatePath = inversePredicateResult.Path;
                    break;
                }
                result = this.FindPattern(oneOfPattern, opts);
                if (opts.DoClick && result.IsSuccess)
                {
                    successResult = result;
                    var point = result.Point;
                    this.DoClick(point.Offset(clickOffsetX, clickOffsetY));
                    Sleep(500);
                }
                if (clickPattern is not null) this.ClickPattern(clickPattern.Value, opts);
                Sleep(intervalDelayMs);
            }

            if (successResult.IsSuccess && !result.IsSuccess)
            {
                successResult.InversePredicatePath = result.InversePredicatePath;
                result = successResult;
            }
        }
        else if (predicatePattern is not null)
        {
            var predicateOpts = new FindOptions() { VariancePct = opts.PredicateThreshold };
            FindPatternResult successResult = new() { IsSuccess = false };
            while (IsRunning)
            {
                if (hasTimeout && DateTime.Now > timeout) break;

                FindPatternResult predicateResult = this.FindPattern(predicatePattern.Value, predicateOpts);
                if (predicateResult.IsSuccess)
                {
                    result.PredicatePath = predicateResult.Path;
                    break;
                }
                result = this.FindPattern(oneOfPattern, opts);
                if (opts.DoClick && result.IsSuccess)
                {
                    successResult = result;
                    var point = result.Point;
                    this.DoClick(point.Offset(clickOffsetX, clickOffsetY));
                    Sleep(500);
                }
                if (clickPattern is not null) this.ClickPattern(clickPattern.Value, opts);
                Sleep(intervalDelayMs);
            }
            if (successResult.IsSuccess && !result.IsSuccess)
            {
                successResult.PredicatePath = result.PredicatePath;
                result = successResult;
            }
        }
        else
        {
            while (IsRunning)
            {
                if (hasTimeout && DateTime.Now > timeout) break;

                result = this.FindPattern(oneOfPattern, opts);
                if (opts.DoClick && result.IsSuccess)
                {
                    var point = result.Point;
                    this.DoClick(point.Offset(clickOffsetX, clickOffsetY));
                    Sleep(500);
                }
                if (result.IsSuccess) break;
                if (clickPattern is not null) this.ClickPattern(clickPattern.Value, opts);
                Sleep(intervalDelayMs);
            }
        }

        return result;
    }

    public FindPatternResult SwipePollPattern(OneOf<PatternNode, PatternNode[]> oneOfPattern, SwipePollPatternFindOptions opts)
    {
        var pollOpts = new PollPatternFindOptions() { TimeoutMs = opts.PollTimeoutMs };
        var swipeCount = 0;
        var result = PollPattern(oneOfPattern, pollOpts);
        while (IsRunning && !result.IsSuccess && swipeCount < opts.MaxSwipes)
        {
            DoSwipe(opts.Start, opts.End);
            Sleep(opts.SwipeDelayMs);
            result = PollPattern(oneOfPattern, pollOpts);
            swipeCount++;
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
            var variance = 5;
            var xVarianceStart = _random.Next(-variance, variance);
            var yVarianceStart = _random.Next(-variance, variance);
            var xVarianceEnd = _random.Next(-variance, variance);
            var yVarianceEnd = _random.Next(-variance, variance);

            _screenService.DoSwipe(start.Offset(xVarianceStart, yVarianceStart), end.Offset(xVarianceEnd, yVarianceEnd));
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"DoSwipe failed: {ex.Message}");
            throw;
        }
    }

    public void Reset()
    {
        _pathToOffset.Clear();
    }

    public int Random(int min, int max)
    {
        return _random.Next(min, max);
    }

    public string GetText(OneOf<PatternNode, PatternNode[]> oneOfPattern, string whiteList = "")
    {
        PatternNode patternNode;
        if (oneOfPattern.IsT1)
        {
            patternNode = oneOfPattern.AsT1[0];
        }
        else
        {
            patternNode = oneOfPattern.AsT0;
        }

        var pattern = patternNode.Patterns.First();
        var maxTry = 10;
        if (patternNode?.Patterns.FirstOrDefault() == null) return string.Empty;

        var offset = CalcOffset(patternNode.Path, pattern);
        if (InDebugMode)
        {
            MainThread.BeginInvokeOnMainThread(_screenService.DebugClear);
            Sleep(50);
            MainThread.BeginInvokeOnMainThread(() => _screenService.DebugRectangle(pattern.Rect.Offset(offset)));
        }

        var currentTry = 0;
        while (currentTry < maxTry)
        {
            try
            {
                return _screenService.GetText(pattern, new TextFindOptions() { Whitelist = whiteList, Offset = offset });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetText Error");
                Sleep(200);
                currentTry++;
            }
        }
        _logger.LogDebug($"getText failed {maxTry} times...");
        return String.Empty;
    }

}
