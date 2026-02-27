using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using OneOf;
using System.Text.Json.Serialization;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels.NodeViewModels;
using YeetMacro2.ViewModels;
using System.Collections.Concurrent;
using SwipeDirection = YeetMacro2.Data.Models.SwipeDirection;
using Jint.Native;

namespace YeetMacro2.Services;

public class PollPatternFindOptions : ClickPatternFindOptions
{
    public int IntervalDelayMs { get; set; } = 1_000;
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? PredicatePattern { get; set; }
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? ClickPattern { get; set; }
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? ClickPredicatePattern { get; set; }
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? PrimaryClickPredicatePattern { get; set; }
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? PrimaryClickInversePredicatePattern { get; set; }
    [JsonIgnore]
    public PatternNode SwipePattern { get; set; }
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? InversePredicatePattern { get; set; }
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? NoOpPattern { get; set; }
    [JsonIgnore]
    public OneOf<PatternNode, PatternNode[]>? GoBackPattern { get; set; }
    public int InversePredicateChecks { get; set; } = 5;
    public int InversePredicateCheckDelayMs { get; set; } = 100;
    public double PredicateThreshold { get; set; } = 0.0;
    public bool DoClick { get; set; }
    public long HoldDurationMs { get; set; } = 100;
    public double TimeoutMs { get; set; } = 0.0;
    [JsonIgnore]
    public Func<JsValue, JsValue[], JsValue> Callback { get; set; }
}

public class ClickPatternFindOptions : FindOptions
{
    public Point ClickOffset { get; set; } = Point.Zero;
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
    public string PathSuffix { get; set; }
    public Rect RawBounds { get; set; }
    public Size Resolution { get; set; }
    public OffsetCalcType OffsetCalcType { get; set; } = OffsetCalcType.Default;
    public BoundsCalcType BoundsCalcType { get; set; } = BoundsCalcType.Default;
    public SwipeDirection SwipeDirection { get; set; } = SwipeDirection.Auto;
}

public class MacroService
{
    readonly LogServiceViewModel _logServiceViewModel;
    readonly IScreenService _screenService;
    readonly IInputService _inputService;
    readonly ConcurrentDictionary<string, Point> _pathToOffset = [];
    readonly ConcurrentDictionary<string, Rect> _pathToBounds = [];
    readonly ConcurrentDictionary<string, PatternNode> _pathToClone = [];
    readonly Random _random;
    public bool InDebugMode { get; set; }
    public bool IsRunning { get; set; }

    public MacroService(LogServiceViewModel LogServiceViewModel, IScreenService screenService, IInputService inputService)
    {
        _logServiceViewModel = LogServiceViewModel;
        _screenService = screenService;
        _inputService = inputService;
        _random = new Random();

        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, string>(this, nameof(MacroManagerViewModel), (r, propertyChangedMessage) =>
        {
            if (propertyChangedMessage.PropertyName == nameof(MacroManagerViewModel.InDebugMode))
            {
                InDebugMode = propertyChangedMessage.NewValue;
            }
        });

        WeakReferenceMessenger.Default.Register<DisplayInfoChangedEventArgs>(this, (r, e) =>
        {
            Reset();
        });
    }

    public void Sleep(int ms)
    {
        Thread.Sleep(ms);
    }

    public Point CalcOffset(string path, Pattern pattern)
    {
        if (!_pathToOffset.ContainsKey(path))
        {
            _pathToOffset[path] = pattern.Offset;
        }

        return _pathToOffset[path];
    }

    public Rect CalcBounds(string path, Pattern pattern)
    {
        if (!_pathToBounds.ContainsKey(path))
        {
            _pathToBounds[path] = pattern.Bounds;
        }

        return _pathToBounds[path];
    }

    public Size GetCurrentResolution()
    {
        return DisplayHelper.PhysicalResolution;
    }

    public double GetScreenDensity()
    {
        return DisplayHelper.DisplayInfo.Density;
    }

    public Point GetTopLeft()
    {
        return DisplayHelper.TopLeft;
    }

    public PatternNode ClonePattern(PatternNode patternNode, CloneOptions opts)
    {
        var resolvedPath = patternNode.Path;
        if (!string.IsNullOrEmpty(opts.PathSuffix)) resolvedPath = $"{patternNode.Path}{opts.PathSuffix}";
        if (!string.IsNullOrEmpty(opts.Path)) resolvedPath = opts.Path;

        if (_pathToClone.ContainsKey(resolvedPath))
        {
            return _pathToClone[resolvedPath];
        }

        var clone = PatternNodeManagerViewModel.CloneNode(patternNode);
        clone.Path = resolvedPath;

        foreach (var pattern in clone.Patterns)
        {
            if (opts.OffsetCalcType != OffsetCalcType.Default) pattern.OffsetCalcType = opts.OffsetCalcType;
            if (opts.BoundsCalcType != BoundsCalcType.Default) pattern.BoundsCalcType = opts.BoundsCalcType;
            if (opts.SwipeDirection != SwipeDirection.Auto) pattern.SwipeDirection = opts.SwipeDirection;

            if (!opts.RawBounds.IsEmpty)
            {
                pattern.RawBounds = opts.RawBounds;
                continue;
            }

            if (!opts.Resolution.IsZero)
            {
                pattern.Resolution = opts.Resolution;
            }

            var rect = pattern.RawBounds;
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

            pattern.RawBounds = new Rect(location, size);
        }

        if (opts.Scale != 1.0)
        {
            clone.Patterns = clone.Patterns.Select(
                p => PatternNodeManagerViewModel.GetScaled(_screenService, p, opts.Scale)).ToList();
        }

        _pathToClone.TryAdd(resolvedPath, clone);
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
                _logServiceViewModel.Debug = $"Find: {path}";

                if (patternNode.IsMultiPattern)
                {
                    var points = new List<Point>();
                    var multiResult = new FindPatternResult();
                    var idx = 0;

                    foreach (var pattern in patternNode.Patterns)
                    {
                        var patternPath = $"{patternNode.Path}_{idx}";
                        var offset = CalcOffset(patternPath, pattern);
                        var bounds = CalcBounds(patternPath, pattern);
                        var optsWithOffset = new FindOptions()
                        {
                            Limit = opts.Limit,
                            VariancePct = opts.VariancePct,
                            Offset = opts.Offset.Offset(offset.X, offset.Y),
                            OverrideBounds = opts.OverrideBounds,
                            OverrideOffsetCalcType = opts.OverrideOffsetCalcType
                        };

                        if (InDebugMode && pattern.RawBounds != Rect.Zero)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                _screenService.DebugClear();
                                _screenService.DebugRectangle(bounds.Offset(offset));
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
                    if (pattern is null) continue;

                    var offset = CalcOffset(patternNode.Path, pattern);
                    var bounds = CalcBounds(patternNode.Path, pattern);
                    var optsWithOffset = new FindOptions()
                    {
                        Limit = opts.Limit,
                        VariancePct = opts.VariancePct,
                        Offset = opts.Offset.Offset(offset.X, offset.Y),
                        OverrideBounds = opts.OverrideBounds,
                        OverrideOffsetCalcType = opts.OverrideOffsetCalcType
                    };

                    if (InDebugMode && pattern.RawBounds != Rect.Zero)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            _screenService.DebugRectangle(bounds.Offset(offset));
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
            _logServiceViewModel.Debug = $"findPattern failed: {ex.Message}";
            throw;
        }
    }

    public void DoClick(Point point, long holdDurationMs = 100)
    {
        var variance = 5;
        var xVariance = _random.Next(-variance, variance);
        var yVariance = _random.Next(-variance, variance);
        _screenService.DoClick(point.Offset(xVariance, yVariance), holdDurationMs);
    }

    public FindPatternResult ClickPattern(OneOf<PatternNode, PatternNode[]> oneOfPattern, ClickPatternFindOptions opts = null)
    {
        opts ??= new ClickPatternFindOptions();

        FindPatternResult result = this.FindPattern(oneOfPattern, opts);
        if (result.IsSuccess)
        {
            foreach (var point in result.Points)
            {
                this.DoClick(point.Offset(opts.ClickOffset.X, opts.ClickOffset.Y));
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
                        Type = PatternType.Bounds,
                        RawBounds = new Rect(point, Size.Zero),
                        Resolution = DisplayHelper.PhysicalResolution,
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
        var predicatePattern = opts.PredicatePattern;
        var clickPattern = opts.ClickPattern;
        var clickPredicatePattern = opts.ClickPredicatePattern;
        var primaryClickPredicatePattern = opts.PrimaryClickPredicatePattern;
        var primaryClickInversePredicatePattern = opts.PrimaryClickInversePredicatePattern;
        var swipePattern = opts.SwipePattern;
        var intervalDelayMs = opts.SwipePattern is not null && opts.IntervalDelayMs == 1_000 ? 2_000 : opts.IntervalDelayMs;
        var inversePredicatePattern = opts.InversePredicatePattern;
        var noOpPattern = opts.NoOpPattern;
        var goBackPattern = opts.GoBackPattern;
        var clickOffsetX = opts.ClickOffset.X;
        var clickOffsetY = opts.ClickOffset.Y;
        var hasTimeout = opts.TimeoutMs > 0;
        var timeout = hasTimeout ? DateTime.Now.AddMilliseconds(opts.TimeoutMs) : DateTime.MaxValue;

        if (inversePredicatePattern is not null)
        {
            var inversePredicateChecks = opts.InversePredicateChecks;
            var inversePredicateCheckDelayMs = opts.InversePredicateCheckDelayMs;
            var predicateOpts = new FindOptions() { VariancePct = opts.PredicateThreshold };
            var successResult = new FindPatternResult() { IsSuccess = false };

            while (IsRunning)
            {
                opts.Callback?.Invoke(null, null);

                if (hasTimeout && DateTime.Now > timeout) return new FindPatternResult() { IsSuccess = false };
                if (noOpPattern is not null && this.FindPattern(noOpPattern.Value, opts).IsSuccess)
                {
                    Sleep(intervalDelayMs);
                    continue;
                }
                if (goBackPattern is not null && this.FindPattern(goBackPattern.Value, opts).IsSuccess)
                {
                    GoBack();
                    Sleep(intervalDelayMs);
                    continue;
                }

                var numChecks = 1;
                var inversePredicateResult = this.FindPattern(inversePredicatePattern.Value, predicateOpts);
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
                var shouldClickPrimary = opts.DoClick && result.IsSuccess &&
                    (primaryClickPredicatePattern is null || this.FindPattern(primaryClickPredicatePattern.Value).IsSuccess) &&
                    (primaryClickInversePredicatePattern is null || !this.FindPattern(primaryClickInversePredicatePattern.Value).IsSuccess);
                if (shouldClickPrimary)
                {
                    successResult = result;
                    var point = result.Point;
                    this.DoClick(point.Offset(clickOffsetX, clickOffsetY), opts.HoldDurationMs);
                    Sleep(500);
                }
                if (clickPredicatePattern is null && clickPattern is not null) this.ClickPattern(clickPattern.Value, opts);
                if (clickPredicatePattern is not null && clickPattern is not null && this.FindPattern(clickPredicatePattern.Value).IsSuccess) this.ClickPattern(clickPattern.Value, opts);
                if (swipePattern is not null) this.SwipePattern(swipePattern);
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
            var successResult = new FindPatternResult() { IsSuccess = false };
            while (IsRunning)
            {
                opts.Callback?.Invoke(null, null);
                if (hasTimeout && DateTime.Now > timeout) return new FindPatternResult() { IsSuccess = false };
                if (noOpPattern is not null && this.FindPattern(noOpPattern.Value, opts).IsSuccess)
                {
                    Sleep(intervalDelayMs);
                    continue;
                }
                if (goBackPattern is not null && this.FindPattern(goBackPattern.Value, opts).IsSuccess)
                {
                    GoBack();
                    Sleep(intervalDelayMs);
                    continue;
                }

                FindPatternResult predicateResult = this.FindPattern(predicatePattern.Value, predicateOpts);
                if (predicateResult.IsSuccess)
                {
                    result.PredicatePath = predicateResult.Path;
                    break;
                }
                result = this.FindPattern(oneOfPattern, opts);
                var shouldClickPrimary = opts.DoClick && result.IsSuccess &&
                    (primaryClickPredicatePattern is null || this.FindPattern(primaryClickPredicatePattern.Value).IsSuccess) &&
                    (primaryClickInversePredicatePattern is null || !this.FindPattern(primaryClickInversePredicatePattern.Value).IsSuccess);
                if (shouldClickPrimary)
                {
                    successResult = result;
                    var point = result.Point;
                    this.DoClick(point.Offset(clickOffsetX, clickOffsetY), opts.HoldDurationMs);
                    Sleep(500);
                }
                if (clickPredicatePattern is null && clickPattern is not null) this.ClickPattern(clickPattern.Value, opts);
                if (clickPredicatePattern is not null && clickPattern is not null && this.FindPattern(clickPredicatePattern.Value).IsSuccess) this.ClickPattern(clickPattern.Value, opts);
                if (swipePattern is not null) this.SwipePattern(swipePattern);
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
                opts.Callback?.Invoke(null, null);
                if (hasTimeout && DateTime.Now > timeout) return new FindPatternResult() { IsSuccess = false };
                if (noOpPattern is not null && this.FindPattern(noOpPattern.Value, opts).IsSuccess)
                {
                    Sleep(intervalDelayMs);
                    continue;
                }
                if (goBackPattern is not null && this.FindPattern(goBackPattern.Value, opts).IsSuccess)
                {
                    GoBack();
                    Sleep(intervalDelayMs);
                    continue;
                }

                result = this.FindPattern(oneOfPattern, opts);
                var shouldClickPrimary = opts.DoClick && result.IsSuccess &&
                    (primaryClickPredicatePattern is null || this.FindPattern(primaryClickPredicatePattern.Value).IsSuccess) &&
                    (primaryClickInversePredicatePattern is null || !this.FindPattern(primaryClickInversePredicatePattern.Value).IsSuccess);
                if (shouldClickPrimary)
                {
                    var point = result.Point;
                    this.DoClick(point.Offset(clickOffsetX, clickOffsetY), opts.HoldDurationMs);
                    Sleep(500);
                }
                if (result.IsSuccess) break;
                if (clickPredicatePattern is null && clickPattern is not null) this.ClickPattern(clickPattern.Value, opts);
                if (clickPredicatePattern is not null && clickPattern is not null && this.FindPattern(clickPredicatePattern.Value).IsSuccess) this.ClickPattern(clickPattern.Value, opts);
                if (swipePattern is not null) this.SwipePattern(swipePattern);
                Sleep(intervalDelayMs);
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
            _logServiceViewModel.Debug = $"debugRectangle failed: {ex.Message}";
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
            _logServiceViewModel.Debug = $"debugClear failed: {ex.Message}";
            throw;
        }
    }

    public void SwipePattern(PatternNode patternNode)
    {
        if (patternNode.Pattern is null) throw new Exception("Pattern not found");

        var pattern = patternNode.Pattern;
        if (InDebugMode)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var bounds = CalcBounds(patternNode.Path, pattern);
                _screenService.DebugClear();
                _screenService.DebugRectangle(bounds.Offset(pattern.Offset));
            });
        }

        _screenService.SwipePattern(pattern);
    }

    public void Reset()
    {
        _pathToOffset.Clear();
        _pathToClone.Clear();
        _pathToBounds.Clear();
        if (InDebugMode)
        {
            MainThread.BeginInvokeOnMainThread(_screenService.DebugClear);
        }
    }

    public int Random(int min, int max)
    {
        return _random.Next(min, max);
    }

    public string FindTextWithBounds(Rect bounds, string whiteList = "")
    {
        if (InDebugMode)
        {
            MainThread.BeginInvokeOnMainThread(_screenService.DebugClear);
            Sleep(50);
            MainThread.BeginInvokeOnMainThread(() => _screenService.DebugRectangle(bounds));
        }

        return _screenService.FindText(bounds, new TextFindOptions() { Whitelist = whiteList });
    }

    public string FindText(OneOf<PatternNode, PatternNode[]> oneOfPattern, string whiteList = "")
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
            var bounds = CalcBounds(patternNode.Path, pattern);
            MainThread.BeginInvokeOnMainThread(_screenService.DebugClear);
            Sleep(50);
            MainThread.BeginInvokeOnMainThread(() => _screenService.DebugRectangle(bounds.Offset(offset)));
        }

        var currentTry = 0;
        while (currentTry < maxTry)
        {
            try
            {
                return _screenService.FindText(pattern, new TextFindOptions() { Whitelist = whiteList, Offset = offset });
            }
            catch (Exception ex)
            {
                _logServiceViewModel.LogException(ex);
                Sleep(200);
                currentTry++;
            }
        }
        _logServiceViewModel.Debug = $"getText failed {maxTry} times...";
        return String.Empty;
    }

    public void GoBack()
    {
        _inputService.GoBack();
    }

}
