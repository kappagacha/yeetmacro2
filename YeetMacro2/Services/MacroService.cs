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
    public double TimoutMs { get; set; } = 0.0;
    public Point ClickOffset { get; set; } = Point.Zero;
}

public class SwipePollPatternFindOptions : FindOptions
{
    public int PollTimoutMs { get; set; } = 2_000;
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
    ILogger _logger;
    IScreenService _screenService;
    Dictionary<string, Point> _pathToOffset;
    Random _random;
    public bool InDebugMode { get; set; }
    public bool IsRunning { get; set; }

    public MacroService(ILogger<MacroService> logger, IScreenService screenService)
    {
        _logger = logger;
        _screenService = screenService;
        _pathToOffset = new Dictionary<string, Point>();
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

    public Point CalcOffset(PatternNode patternNode)
    {
        if (patternNode.Patterns.Count == 0) return Point.Zero;

        var pattern = patternNode.Patterns.First();

        if (pattern.IsLocationDynamic)
        {
            return PatternNodeManagerViewModel.CalcOffset(pattern, _screenService.Resolution);
        }

        if (!_pathToOffset.ContainsKey(patternNode.Path))
        {
            _pathToOffset[patternNode.Path] = PatternNodeManagerViewModel.CalcOffset(pattern, _screenService.CalcResolution);
        }

        return _pathToOffset[patternNode.Path];
    }

    public Size GetCurrentResolution()
    {
        return _screenService.Resolution;
    }

    public double GetScreenDensity()
    {
        return _screenService.Density;
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
            pattern.OffsetCalcType = opts.OffsetCalcType;
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
        if (opts is null) opts = new FindOptions();

        var result = new FindPatternResult() { IsSuccess = false };
        PatternNode[] patternNodes;

        if (oneOfPattern.IsT1)
        {
            patternNodes = oneOfPattern.AsT1;
        }
        else
        {
            patternNodes = new PatternNode[] { oneOfPattern.AsT0 };
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
                    var offset = CalcOffset(patternNode);

                    foreach (var pattern in patternNode.Patterns)
                    {
                        var optsWithOffset = new FindOptions() {
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
                    var pattern = patternNode.Patterns.FirstOrDefault();
                    if (pattern is null) return result;

                    var offset = CalcOffset(patternNode);
                    var optsWithOffset = new FindOptions() {
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

        Sleep(500);
        return result;
    }

    public FindPatternResult PollPattern(OneOf<PatternNode, PatternNode[]> oneOfPattern, PollPatternFindOptions opts = null)
    {
        if (opts is null) opts = new PollPatternFindOptions();

        var result = new FindPatternResult() { IsSuccess = false };
        var intervalDelayMs = opts.IntervalDelayMs;
        var predicatePattern = opts.PredicatePattern;
        var clickPattern = opts.ClickPattern;
        var inversePredicatePattern = opts.InversePredicatePattern;
        var clickOffsetX = opts.ClickOffset.X;
        var clickOffsetY = opts.ClickOffset.Y;
        var hasTimeout = opts.TimoutMs > 0;
        var timeout = hasTimeout ? DateTime.Now.AddMilliseconds(opts.TimoutMs) : DateTime.MaxValue;

        if (inversePredicatePattern is not null)
        {
            var inversePredicateChecks = opts.InversePredicateChecks;
            var inversePredicateCheckDelayMs = opts.InversePredicateCheckDelayMs;
            var predicateOpts = new FindOptions() { VariancePct = opts.PredicateThreshold };
            FindPatternResult successResult = new FindPatternResult() { IsSuccess = false };

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
            FindPatternResult successResult = new FindPatternResult() { IsSuccess = false };
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
        var pollOpts = new PollPatternFindOptions() { TimoutMs = opts.PollTimoutMs };
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

        var maxTry = 10;
        if (patternNode?.Patterns.FirstOrDefault() == null) return string.Empty;

        var offset = CalcOffset(patternNode);
        var currentTry = 0;
        while (currentTry < maxTry)
        {
            try
            {
                return _screenService.GetText(patternNode.Patterns.First(), new TextFindOptions() { Whitelist = whiteList, Offset = offset });
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
