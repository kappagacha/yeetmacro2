﻿using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Provider;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Serialization;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Services;
using YeetMacro2.ViewModels;
using YeetMacro2.Views;
using CommunityToolkit.Mvvm.Messaging;
using static Android.Graphics.Bitmap;
using Android.Media.Projection;
using AndroidX.Core.View;

namespace YeetMacro2.Platforms.Android.Services;

public enum AndroidWindowView
{
    PatternNodeView,
    ScriptNodeView,
    SettingNodeView,
    DailyNodeView,
    WeeklyNodeView,
    DrawView,
    UserDrawView,
    DebugDrawView,
    ActionView,
    PromptStringInputView,
    PromptSelectOptionView,
    StatusPanelView,
    MacroOverlayView,
    MessageView,
    TestView
}

public class AndroidScreenService : IScreenService
{
    public const int OVERLAY_SERVICE_REQUEST = 0;
    public const int POST_NOTIFICATION_REQUEST = 10;
    public const int MEDIA_PROJECTION_REQUEST = 20;
    readonly IWindowManager _windowManager;
    readonly ConcurrentDictionary<AndroidWindowView, IShowable> _views = new();
    //FormsView _overlayWindow;
    readonly ILogger _logger;
    readonly MainActivity _context;
    readonly OpenCvService _openCvService;
    readonly MediaProjectionService _mediaProjectionService;
    readonly IOcrService _ocrService;
    readonly YeetAccessibilityService _accessibilityService;
    readonly IToastService _toastService;
    readonly Random _random = new Random();
    //Size _initialResolution;
    //readonly double _density;
    public int UserDrawViewWidth => _views.ContainsKey(AndroidWindowView.UserDrawView) ? ((FormsView)_views[AndroidWindowView.UserDrawView]).MeasuredHeightAndState : -1;
    public int UserDrawViewHeight => _views.ContainsKey(AndroidWindowView.UserDrawView) ? ((FormsView)_views[AndroidWindowView.UserDrawView]).MeasuredWidthAndState : -1;
    //public Size CalcResolution
    //{
    //    get
    //    {
    //        var overlayWidth = _overlayWindow?.MeasuredWidthAndState ?? 0;
    //        var overlayHeight = _overlayWindow?.MeasuredHeightAndState ?? 0;
    //        var width = overlayWidth > overlayHeight ? Math.Max(_initialResolution.Width, _initialResolution.Height) : Math.Min(_initialResolution.Width, _initialResolution.Height);
    //        var height = overlayHeight > overlayWidth ? Math.Max(_initialResolution.Width, _initialResolution.Height) : Math.Min(_initialResolution.Width, _initialResolution.Height);
    //        return new Size(width, height);
    //    }
    //}
    //public Size Resolution => new(_overlayWindow?.MeasuredWidthAndState ?? 0, _overlayWindow?.MeasuredHeightAndState ?? 0);
    //public double Density => _density;
    //public int DisplayCutoutTop => _androidWindowManagerService.OverlayWindow == null ? 0 : _androidWindowManagerService.OverlayWindow.RootWindowInsets.DisplayCutout?.SafeInsetTop ?? 0;
    readonly JsonSerializerOptions _serializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = PointPropertiesResolver.Instance
    };
    readonly Dictionary<MacroSetViewModel, IShowable> _macroSetToPatternsView = new Dictionary<MacroSetViewModel, IShowable>();
    readonly Dictionary<MacroSetViewModel, IShowable> _macroSetToSettingsView = new Dictionary<MacroSetViewModel, IShowable>();
    readonly Dictionary<MacroSetViewModel, IShowable> _macroSetToScriptsView = new Dictionary<MacroSetViewModel, IShowable>();
    readonly Dictionary<MacroSetViewModel, IShowable> _macroSetToDailiesView = new Dictionary<MacroSetViewModel, IShowable>();
    readonly Dictionary<MacroSetViewModel, IShowable> _macroSetToWeekliesView = new Dictionary<MacroSetViewModel, IShowable>();
    public IReadOnlyDictionary<AndroidWindowView, IShowable> Views => _views;
    public IReadOnlyDictionary<MacroSetViewModel, IShowable> PatternViews => _macroSetToPatternsView;
    public IReadOnlyDictionary<MacroSetViewModel, IShowable> ScriptViews => _macroSetToScriptsView;
    public string TestMessage
    {
        get
        {
            var y = Platform.CurrentActivity?.Window.Attributes;
            var insets = Platform.CurrentActivity?.Window?.DecorView.RootWindowInsets;

            var x = insets.GetInsets(WindowInsetsCompat.Type.TappableElement());
            var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
            var ime = insets.GetInsets(WindowInsetsCompat.Type.Ime());
            var displayCutout = Platform.CurrentActivity?.Window?.DecorView.RootWindowInsets?.DisplayCutout;

            return "Test";
            //var activity = Platform.CurrentActivity;
            //var window = activity?.Window;
            //if (window is null) return "Window not found";

            //var rect = new Rect();
            //window.DecorView.GetWindowVisibleDisplayFrame(rect);
            //return $"Top: {rect.Top}\nLeft: {rect.Left}\nRight: {rect.Right}\nBottom: {rect.Bottom}";

            //var rect = new Rect();
            //_overlayWindow.GetGlobalVisibleRect(rect);
            //return $"Top: {rect.Top}\nLeft: {rect.Left}\nRight: {rect.Right}\nBottom: {rect.Bottom}" + "\n\n" +
            // $"X: {_overlayWindow.GetX()}\nY: {_overlayWindow.GetY()}\nTop: {_overlayWindow.Top}\nLeft: {_overlayWindow.Left}\nRight: {_overlayWindow.Right}\nBottom: {_overlayWindow.Bottom}";
        }
    }
    public bool CanDrawOverlays => Settings.CanDrawOverlays(_context);

    public AndroidScreenService(ILogger<AndroidScreenService> logger, OpenCvService openCvService, 
        MediaProjectionService mediaProjectionService, IOcrService ocrService,
        YeetAccessibilityService accessibilityService, IToastService toastService)
    {
        _logger = logger;
        _context = (MainActivity)Platform.CurrentActivity;
        _openCvService = openCvService;
        _mediaProjectionService = mediaProjectionService;
        _ocrService = ocrService;
        _accessibilityService = accessibilityService;
        _toastService = toastService;
        _windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        
        //_initialResolution = new Size(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);
        //_density = DeviceDisplay.MainDisplayInfo.Density;
        WeakReferenceMessenger.Default.Register<ForegroundService>(this, (r, foregroundService) =>
        {
            if (foregroundService.IsRunning)    // true means ForegroundService was started
            {
                //ShowOverlayWindow();
            }
            else // Foreground Service Exit action
            {
                CloseAll();
                Close(AndroidWindowView.MacroOverlayView);
                Close(AndroidWindowView.ActionView);
                Close(AndroidWindowView.StatusPanelView);
                //CloseOverlayWindow();
            }
        });

        WeakReferenceMessenger.Default.Register<DisplayInfoChangedEventArgs>(this, (r, e) =>
        {
            RefreshActionViewLocation();
        });
    }

    public byte[] CalcColorThreshold(Pattern pattern, ColorThresholdProperties colorThreshold)
    {
        return _openCvService.CalcColorThreshold(pattern.ImageData, colorThreshold);
    }

    public FindPatternResult ClickPattern(Pattern pattern, FindOptions opts)
    {
        var result = FindPattern(pattern, opts);
        if (result.IsSuccess)
        {
            foreach (var point in result.Points)
            {
                DoClick(point);
            }
        }

        return result;
    }

    public void DebugCircle(Point point)
    {
        Show(AndroidWindowView.DebugDrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.DebugDrawView].VisualElement;
        drawControl.AddCircle(point);
    }

    public void DebugClear()
    {
        if (!_views.ContainsKey(AndroidWindowView.DebugDrawView)) return;

        var drawControl = (DrawControl)_views[AndroidWindowView.DebugDrawView].VisualElement;
        drawControl.ClearCircles();
        drawControl.ClearRectangles();
    }

    public void DebugRectangle(Rect rect)
    {
        Show(AndroidWindowView.DebugDrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.DebugDrawView].VisualElement;
        var thickness = 10;
        var loc = new Point(rect.X - thickness, rect.Y - thickness);
        var size = new Size(rect.Width + thickness * 2, rect.Height + thickness * 2);

        drawControl.AddRectangle(new Rect(loc, size));
    }

    public void DoClick(Point point, long holdDurationMs = 100)
    {
        _accessibilityService.DoClick(point, holdDurationMs);
    }

    public void SwipePattern(Pattern pattern)
    {
        var bounds = pattern.Bounds.Offset(pattern.Offset);
        var direction = pattern.SwipeDirection == Data.Models.SwipeDirection.Auto
            ? (bounds.Height > bounds.Width
                ? Data.Models.SwipeDirection.BottomToTop
                : Data.Models.SwipeDirection.RightToLeft)
            : pattern.SwipeDirection;

        var center = bounds.Center;
        var halfWidth = bounds.Width / 2;
        var halfHeight = bounds.Height / 2;

        var (dx, dy) = direction switch
        {
            Data.Models.SwipeDirection.RightToLeft => (halfWidth, 0.0),
            Data.Models.SwipeDirection.LeftToRight => (-halfWidth, 0.0),
            Data.Models.SwipeDirection.BottomToTop => (0.0, halfHeight),
            Data.Models.SwipeDirection.TopToBottom => (0.0, -halfHeight),
            _ => (0, 0)
        };

        var variance = 5;
        var dxVarianceStart = _random.Next(-variance, variance);
        var dyVarianceStart = _random.Next(-variance, variance);
        var dxVarianceEnd = _random.Next(-variance, variance);
        var dyVarianceEnd = _random.Next(-variance, variance);
        var start = center.Offset(dx + dxVarianceStart, dy + dyVarianceStart);
        var end = center.Offset(-dx + dxVarianceEnd, -dy + dyVarianceEnd);
        DoSwipe(start, end);
    }

    public void DoSwipe(Point start, Point end)
    {
        _accessibilityService.DoSwipe(start, end);
    }

    public void DrawCircle(Point point)
    {
        Show(AndroidWindowView.DrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.DrawView].VisualElement;
        var drawView = (FormsView)_views[AndroidWindowView.DrawView];
        drawView.SetIsTouchable(true);
        drawControl.AddCircle(point);
    }

    public void DrawClear()
    {
        if (!_views.ContainsKey(AndroidWindowView.DrawView)) return;

        var drawControl = (DrawControl)_views[AndroidWindowView.DrawView].VisualElement;
        drawControl.ClearCircles();
        drawControl.ClearRectangles();
    }

    public void DrawRectangle(Rect rect)
    {
        Show(AndroidWindowView.DrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.DrawView].VisualElement;
        var drawView = (FormsView)_views[AndroidWindowView.DrawView];
        drawView.SetIsTouchable(true);
        drawControl.AddRectangle(rect);
    }

    public FindPatternResult FindPattern(Pattern pattern, FindOptions opts)
    {
        if (pattern.Type == PatternType.Bounds)
        {
            return new FindPatternResult()
            {
                IsSuccess = true,
                Point = pattern.RawBounds.Offset(opts.Offset).Center,
                Points = [pattern.RawBounds.Offset(opts.Offset).Center]
            };
        }

        var points = GetMatches(pattern, opts);

        var result = new FindPatternResult
        {
            IsSuccess = points.Count > 0
        };
        if (points.Count > 0)
        {
            result.Point = new Point(points[0].X, points[0].Y);
            result.Points = [.. points];
        }

        return result;
    }

    public byte[] GetCurrentImageData()
    {
        return _mediaProjectionService.GetCurrentImageData();
    }

    public byte[] GetCurrentImageData(Rect rect)
    {
        return _mediaProjectionService.GetCurrentImageData(rect);
    }

    public byte[] ScaleImageData(byte[] data, double scale)
    {
        if (data is null || data.Length == 0) return null;

        var bitmap = global::Android.Graphics.BitmapFactory.DecodeByteArray(data, 0, data.Length);
        var targetWidth = bitmap.Width * scale;
        var targetHeight = bitmap.Height * scale;
        var resizedBitmap = global::Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, (int)targetWidth, (int)targetHeight, false);

        MemoryStream ms = new();
        resizedBitmap.Compress(CompressFormat.Jpeg, 100, ms);
        ms.Position = 0;
        var array = ms.ToArray();
        bitmap.Dispose();
        resizedBitmap.Dispose();
        ms.Close();
        ms.Dispose();

        return array;
    }

    public List<Point> GetMatches(Pattern pattern, FindOptions opts)
    {
        try
        {
            var watch = new System.Diagnostics.Stopwatch();
            var boundsPadding = 4;
            byte[] needleImageData = pattern.ImageData;
            byte[] haystackImageData = null;
            var rect = pattern.Bounds.Offset(opts.Offset);
            //var rect = opts.OverrideRect != Rect.Zero ? opts.OverrideRect : pattern.RawBounds;

            watch.Start();
            try
            {
                //haystackImageData = rect != Rect.Zero ?
                //    await _mediaProjectionService.GetCurrentImageData(rect.Offset(-topLeft.x, -topLeft.y)) :
                //    await _mediaProjectionService.GetCurrentImageData();
                if (pattern.TextMatch.IsActive && !String.IsNullOrEmpty(pattern.TextMatch.Text))
                {
                    haystackImageData = _mediaProjectionService.GetCurrentImageData(
                        new Rect(rect.Location.Offset(-boundsPadding, -boundsPadding),
                        rect.Size + new Size(boundsPadding, boundsPadding)));
                }
                else if (pattern.OffsetCalcType == OffsetCalcType.Default || pattern.OffsetCalcType == OffsetCalcType.Center)
                {
                    var topLeftPadding = 1;
                    haystackImageData = _mediaProjectionService.GetCurrentImageData(
                        new Rect(rect.Location.Offset(-topLeftPadding, -topLeftPadding),
                        rect.Size + new Size(topLeftPadding * 2, topLeftPadding * 2)));
                    //var topLeft = DisplayHelper.TopLeft;
                    //if (!topLeft.IsEmpty)   // handle off by one due to calculations
                    //{
                    //    var topLeftPadding = 1;
                    //    haystackImageData = _mediaProjectionService.GetCurrentImageData(
                    //        new Rect(rect.Location.Offset(-topLeftPadding, -topLeftPadding),
                    //        rect.Size + new Size(topLeftPadding * 2, topLeftPadding * 2)));
                    //}
                }

                haystackImageData ??= rect != Rect.Zero ?
                       _mediaProjectionService.GetCurrentImageData(rect) :
                       _mediaProjectionService.GetCurrentImageData();
            }
            catch (Exception ex)
            {

                _logger.LogTrace($"AndroidScreenService Exception: {ex.Message}");
                return [];
            }

            if (haystackImageData == null)
            {
                return [];
            }

            if (pattern.ColorThreshold.IsActive)
            {
                needleImageData = pattern.ColorThreshold.ImageData; // OpenCvHelper.CalcColorThreshold(pattern.ImageData, pattern.ColorThreshold);
                haystackImageData = _openCvService.CalcColorThreshold(haystackImageData, pattern.ColorThreshold);

                if (needleImageData.Length == 0 || haystackImageData.Length == 0)
                {
                    return [];
                }
            }

            var threshold = 0.8;
            if (pattern.VariancePct != 0.0) threshold = 1 - pattern.VariancePct / 100;
            if ((opts?.VariancePct ?? 0.0) != 0.0) threshold = opts.VariancePct;

            if (pattern.TextMatch.IsActive && !String.IsNullOrEmpty(pattern.TextMatch.Text))
            {
                var text = _ocrService.FindText(haystackImageData, pattern.TextMatch.WhiteList);
                var textPoints = new List<Point>();

                if (text == pattern.TextMatch.Text && pattern.RawBounds != Rect.Zero)
                {
                    textPoints.Add(rect.Center.Offset(-boundsPadding, -boundsPadding));
                }
                else if (text == pattern.TextMatch.Text)  // TextMatch is not meant to be used on whole screen
                {
                    // TODO: Throw exception instead?
                    textPoints.Add(new Point(0, 0));
                }

                watch.Stop();
                Console.WriteLine($"GetMatches TextMatch: {watch.ElapsedMilliseconds} ms");
                return textPoints;
            }

            // for debugging
            //var folder = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
            //var haystackFile = System.IO.Path.Combine(folder, $"haystack_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.jpeg");
            //using (FileStream fs = new FileStream(haystackFile, FileMode.OpenOrCreate))
            //{
            //    fs.Write(haystackImageData, 0, haystackImageData.Length);
            //}
            //var needleFile = System.IO.Path.Combine(folder, $"needle_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.jpeg");
            //using (FileStream fs = new FileStream(needleFile, FileMode.OpenOrCreate))
            //{
            //    fs.Write(needleImageData, 0, needleImageData.Length);
            //}

            var points = _openCvService.GetPointsWithMatchTemplate(haystackImageData, needleImageData, opts?.Limit ?? 1, threshold, pattern.ColorThreshold.IgnoreBackground);
            if (pattern.RawBounds != Rect.Zero)
            {
                var newPoints = new List<Point>();
                for (int i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    newPoints.Add(point.Offset(rect.X, rect.Y));
                }

                watch.Stop();
                Console.WriteLine($"GetMatches: {watch.ElapsedMilliseconds} ms");
                return newPoints;
            }

            return points;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] MediaProjectionService GetMatches Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
            return [];
        }
    }

    public string FindText(Pattern pattern, TextFindOptions opts)
    {
        _logger.LogTrace("AndroidScreenService FindText");
        var boundsPadding = 4;
        var currentImageData = pattern.RawBounds != Rect.Zero ?
            _mediaProjectionService.GetCurrentImageData(
                new Rect(pattern.RawBounds.Location.Offset(opts.Offset.X, opts.Offset.Y).Offset(-boundsPadding, -boundsPadding),
                          pattern.RawBounds.Size + new Size(boundsPadding, boundsPadding))) :
            _mediaProjectionService.GetCurrentImageData();
        var imageData = pattern.ColorThreshold.IsActive ?
            _openCvService.CalcColorThreshold(currentImageData, pattern.ColorThreshold) :
            currentImageData;

        return _ocrService.FindText(imageData, pattern.TextMatch.WhiteList ?? opts.Whitelist);
    }

    public string FindText(Rect bounds, TextFindOptions opts)
    {
        _logger.LogTrace("AndroidScreenService FindText");
        var boundsPadding = 4;
        var imageData = _mediaProjectionService.GetCurrentImageData(
                new Rect(bounds.Location.Offset(opts.Offset.X, opts.Offset.Y).Offset(-boundsPadding, -boundsPadding),
                         bounds.Size + new Size(boundsPadding, boundsPadding)));

        return _ocrService.FindText(imageData, opts.Whitelist);
    }

    public Task<string> FindTextAsync(Pattern pattern, TextFindOptions opts)
    {
        _logger.LogTrace("AndroidScreenService FindText");
        var boundsPadding = 4;
        var currentImageData = pattern.RawBounds != Rect.Zero ?
            _mediaProjectionService.GetCurrentImageData(
                new Rect(pattern.RawBounds.Location.Offset(opts.Offset.X, opts.Offset.Y).Offset(-boundsPadding, -boundsPadding),
                          pattern.RawBounds.Size + new Size(boundsPadding, boundsPadding))) :
            _mediaProjectionService.GetCurrentImageData();
        var imageData = pattern.ColorThreshold.IsActive ?
            _openCvService.CalcColorThreshold(currentImageData, pattern.ColorThreshold) :
            currentImageData;

        return _ocrService.FindTextAsync(imageData, pattern.TextMatch.WhiteList ?? opts.Whitelist);
    }

    public string FindText(byte[] currentImage)
    {
        _logger.LogTrace("AndroidScreenService FindText");
        return _ocrService.FindText(currentImage);
    }

    public void ShowMessage(string message)
    {
        Show(AndroidWindowView.MessageView);
        var viewModel = (MessageViewModel)_views[AndroidWindowView.MessageView].VisualElement.BindingContext;
        viewModel.Message = message;
    }

    public void ScreenCapture()
    {
        _mediaProjectionService.TakeScreenCapture();
    }

    public void StartProjectionService()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33) &&
            _context.CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) != global::Android.Content.PM.Permission.Granted)
        {
            _context.RequestPermissions([global::Android.Manifest.Permission.PostNotifications], POST_NOTIFICATION_REQUEST);
            return;
        }

        if (_mediaProjectionService.IsInitialized)
        {
            ServiceHelper.GetService<LogServiceViewModel>().LogInfo("AndroidScreenService.StartProjectionService StartForegroundServiceCompat");
            Platform.AppContext.StartForegroundServiceCompat<ForegroundService>();
        }
        else
        {
            var mediaProjectionManager = (MediaProjectionManager)Platform.CurrentActivity.GetSystemService(Context.MediaProjectionService);
            Platform.CurrentActivity.StartActivityForResult(mediaProjectionManager.CreateScreenCaptureIntent(), Services.MediaProjectionService.REQUEST_MEDIA_PROJECTION);
            return;
        }
    }

    public void StopProjectionService()
    {
        ServiceHelper.GetService<LogServiceViewModel>().LogInfo("AndroidScreenService.StopProjectionService StartForegroundServiceCompat EXIT_ACTION");
        _context.StartForegroundServiceCompat<ForegroundService>(ForegroundService.EXIT_ACTION);
    }

    public void ResetActionViewLocation()
    {
        var selectedMacroSetName = Preferences.Default.Get<string>(nameof(MacroManagerViewModel.SelectedMacroSet), null);
        if (selectedMacroSetName is not null)
        {
            Preferences.Default.Remove($"{selectedMacroSetName}_location_Landscape");
            Preferences.Default.Remove($"{selectedMacroSetName}_location_Portrait");
            _toastService.Show($"Reset Action Button Location for MacroSet {selectedMacroSetName}");

            RefreshActionViewLocation();
        }
    }

    //public void ShowOverlayWindow()
    //{
    //    if (_overlayWindow == null)
    //    {
    //        var grid = new Grid() { InputTransparent = true, CascadeInputTransparent = true };
    //        _overlayWindow = new FormsView(_context, _windowManager, grid) { IsModal = false };
            
    //        _overlayWindow.SetIsTouchable(false);
    //        _overlayWindow.SetBackgroundToTransparent();
    //        //_overlayWindow.DisableTranslucentNavigation();
    //    }

    //    //Get overlay permissin if needed
    //    if (OperatingSystem.IsAndroidVersionAtLeast(23) && !CanDrawOverlays)
    //    {
    //        _context.StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission, global::Android.Net.Uri.Parse("package:" + _context.PackageName)), OVERLAY_SERVICE_REQUEST);
    //        return;
    //    }

    //    _overlayWindow.Show();
    //}

    //public void CloseOverlayWindow()
    //{
    //    _overlayWindow?.Close();
    //    _overlayWindow = null;
    //}

    public void Show(AndroidWindowView windowView)
    {
        //Get overlay permission if needed
        if (OperatingSystem.IsAndroidVersionAtLeast(23) && !CanDrawOverlays)
        {
            _context.StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission, global::Android.Net.Uri.Parse("package:" + _context.PackageName)), OVERLAY_SERVICE_REQUEST);
            return;
        }

        var currentMacroSet = ServiceHelper.GetService<MacroManagerViewModel>()?.SelectedMacroSet;
        if (!_views.ContainsKey(windowView))
        {
            switch (windowView)
            {
                case AndroidWindowView.StatusPanelView:
                    var statusPanelView = new StaticView(_context, _windowManager, new StatusPanelView());
                    statusPanelView.SetUpLayoutParameters(lp =>
                    {
                        lp.Gravity = GravityFlags.Bottom;
                        lp.Width = WindowManagerLayoutParams.MatchParent;
                        if (OperatingSystem.IsAndroidVersionAtLeast(26) && !OperatingSystem.IsAndroidVersionAtLeast(30))
                        {
                            lp.SystemUiFlags |= SystemUiFlags.HideNavigation;
                        }
                    });
                    //statusPanelView.OnClose = () => ServiceHelper.GetService<AndriodHomeViewModel>().ShowStatusPanel = false;
                    _views.TryAdd(windowView, statusPanelView);
                    break;
                case AndroidWindowView.MacroOverlayView:
                    var macroOverlayView = new StaticView(_context, _windowManager, new MacroOverlayView());
                    macroOverlayView.SetUpLayoutParameters(lp =>
                    {
                        lp.Gravity = GravityFlags.Top;
                        lp.Width = WindowManagerLayoutParams.MatchParent;
                    });
                    _views.TryAdd(windowView, macroOverlayView);
                    break;
                case AndroidWindowView.ActionView:
                    var actionView = new MoveView(_context, _windowManager, new ActionControl());
                    _views.TryAdd(windowView, actionView);
                    break;
                case AndroidWindowView.PatternNodeView:
                    if (!_macroSetToPatternsView.ContainsKey(currentMacroSet))
                    {
                        var patternsNodeView = new ResizeView(_context, _windowManager, new PatternNodeView(){ MacroSet = currentMacroSet });
                        patternsNodeView.OnShow = () => {
                            if (_macroSetToScriptsView.ContainsKey(currentMacroSet) && _macroSetToScriptsView[currentMacroSet].IsShowing)
                            {
                                _macroSetToScriptsView[currentMacroSet].Close();
                            }
                            Show(AndroidWindowView.MacroOverlayView);
                        };
                        patternsNodeView.OnClose = () => {
                            Close(AndroidWindowView.MacroOverlayView);
                            ServiceHelper.GetService<AndriodHomeViewModel>().ShowPatternNodeView = false;
                        };
                        _macroSetToPatternsView.Add(currentMacroSet, patternsNodeView);
                    }
                    _macroSetToPatternsView[currentMacroSet].Show();
                    return;
                case AndroidWindowView.SettingNodeView:
                    if (!_macroSetToSettingsView.ContainsKey(currentMacroSet))
                    {
                        var settingNodeView = new ResizeView(_context, _windowManager, new SettingNodeView(){ MacroSet = currentMacroSet });
                        settingNodeView.OnClose = () => ServiceHelper.GetService<AndriodHomeViewModel>().ShowSettingNodeView = false;
                        _macroSetToSettingsView.TryAdd(currentMacroSet, settingNodeView);
                    }
                    _macroSetToSettingsView[currentMacroSet].Show();
                    return;
                case AndroidWindowView.ScriptNodeView:
                    if (!_macroSetToScriptsView.ContainsKey(currentMacroSet))
                    {
                        var scriptNodeView = new ResizeView(_context, _windowManager, new ScriptNodeView() { MacroSet = currentMacroSet, ShowExecuteButton = true });
                        scriptNodeView.OnShow = () => {
                            Show(AndroidWindowView.MacroOverlayView);
                            ServiceHelper.GetService<MacroManagerViewModel>().SelectedMacroSet.Dailies?.ResolveSubViewModelDate();
                        };
                        scriptNodeView.OnClose = () => Close(AndroidWindowView.MacroOverlayView);
                        _macroSetToScriptsView.TryAdd(currentMacroSet, scriptNodeView);
                    }
                    _macroSetToScriptsView[currentMacroSet].Show();
                    return;
                case AndroidWindowView.DailyNodeView:
                    if (!_macroSetToDailiesView.ContainsKey(currentMacroSet))
                    {
                        var dailyNodeView = new ResizeView(_context, _windowManager, new TodoNodeView() { Todos = currentMacroSet.Dailies });
                        _views.TryAdd(windowView, dailyNodeView);
                        dailyNodeView.OnClose = () => ServiceHelper.GetService<AndriodHomeViewModel>().ShowDailyNodeView = false;
                        _macroSetToDailiesView.TryAdd(currentMacroSet, dailyNodeView);
                    }
                    _macroSetToDailiesView[currentMacroSet].Show();
                    return;
                case AndroidWindowView.WeeklyNodeView:
                    if (!_macroSetToWeekliesView.ContainsKey(currentMacroSet))
                    {
                        var weeklyNodeView = new ResizeView(_context, _windowManager, new TodoNodeView() { Todos = currentMacroSet.Weeklies });
                        _views.TryAdd(windowView, weeklyNodeView);
                        weeklyNodeView.OnClose = () => ServiceHelper.GetService<AndriodHomeViewModel>().ShowWeeklyNodeView = false;
                        _macroSetToWeekliesView.TryAdd(currentMacroSet, weeklyNodeView);
                    }
                    _macroSetToWeekliesView[currentMacroSet].Show();
                    return;
                case AndroidWindowView.PromptStringInputView:
                    var promptStringInputView = new FormsView(_context, _windowManager, new PromptStringInput());
                    _views.TryAdd(windowView, promptStringInputView);
                    break;
                case AndroidWindowView.PromptSelectOptionView:
                    var promptSelectOptionView = new FormsView(_context, _windowManager, new PromptSelectOption());
                    _views.TryAdd(windowView, promptSelectOptionView);
                    break;
                case AndroidWindowView.UserDrawView:
                    var userdrawControl = new DrawControl();
                    var userDrawView = new FormsView(_context, _windowManager, userdrawControl) { IsModal = false };
                    userDrawView.SetBackgroundToTransparent();
                    userDrawView.DisableTranslucentNavigation();
                    userDrawView.SetIsTouchable(true);
                    userdrawControl.CloseAfterDraw = true;
                    _views.TryAdd(windowView, userDrawView);
                    break;
                case AndroidWindowView.DrawView:
                    var drawControl = new DrawControl();
                    var drawView = new FormsView(_context, _windowManager, drawControl);
                    drawControl.InputTransparent = true;
                    drawView.SetIsTouchable(false);
                    drawView.Click += DrawView_Click;
                    drawView.SetBackgroundToTransparent();
                    //drawView.DisableTranslucentNavigation();
                    _views.TryAdd(windowView, drawView);
                    break;
                case AndroidWindowView.DebugDrawView:
                    var debugDrawControl = new DrawControl() { InputTransparent = true, CascadeInputTransparent = true };
                    var debugDrawView = new FormsView(_context, _windowManager, debugDrawControl) { IsModal = false };
                    debugDrawView.SetIsTouchable(false);
                    debugDrawView.SetBackgroundToTransparent();
                    //debugDrawView.DisableTranslucentNavigation();
                    _views.TryAdd(windowView, debugDrawView);
                    break;
                case AndroidWindowView.MessageView:
                    var messageView = new ResizeView(_context, _windowManager, new MessageView());
                    _views.TryAdd(windowView, messageView);
                    break;
                case AndroidWindowView.TestView:
                    var testView = new ResizeView(_context, _windowManager, new TestView())
                    {
                        OnClose = () => ServiceHelper.GetService<AndriodHomeViewModel>().ShowTestView = false
                    };
                    _views.TryAdd(windowView, testView);
                    break;
            }
        }

        if (windowView == AndroidWindowView.ActionView)
        {
            RefreshActionViewLocation();
        }

        _views[windowView].Show();

        //if (windowView == AndroidWindowView.MacroOverlayView)
        //{
        //    var ve = _views[windowView].VisualElement;
        //    var ctx = ve.BindingContext;
        //    ve.BindingContext = null;
        //    ve.BindingContext = ctx;
        //}
    }

    public void RefreshActionViewLocation()
    {
        var selectedMacroSetName = Preferences.Default.Get<string>(nameof(MacroManagerViewModel.SelectedMacroSet), null);
        if (selectedMacroSetName is not null && _views.ContainsKey(AndroidWindowView.ActionView))
        {
            var selectedMacroSet = ServiceHelper.GetService<MacroManagerViewModel>().SelectedMacroSet;
            var ve = _views[AndroidWindowView.ActionView].VisualElement;
            var ctx = (IMovable)ve.BindingContext;
            var orientation = DisplayHelper.DisplayInfo.Orientation;
            var preferenceKey = $"{selectedMacroSetName}_location_{orientation}";
            var strTargetLocation = Preferences.Default.Get<string>(preferenceKey, null);
            if (strTargetLocation is not null)
            {
                var targetLocation = JsonSerializer.Deserialize<Point>(strTargetLocation, _serializationOptions);
                ctx.Location = targetLocation;
            }
            else
            {
                ctx.Location = selectedMacroSet.DefaultLocation;
            }
            var moveView = (MoveView)_views[AndroidWindowView.ActionView];
            moveView.SyncLocation();
        }
    }

    // Except action view, overlay and status panel
    public void CloseAll()
    {
        Close(AndroidWindowView.PatternNodeView);
        Close(AndroidWindowView.SettingNodeView);
        Close(AndroidWindowView.ScriptNodeView);
        Close(AndroidWindowView.DailyNodeView);
        Close(AndroidWindowView.WeeklyNodeView);
        Close(AndroidWindowView.TestView);
        Close(AndroidWindowView.DebugDrawView);
    }

    public void Close(AndroidWindowView view)
    {
        var currentMacroSet = ServiceHelper.GetService<MacroManagerViewModel>()?.SelectedMacroSet;
        switch (view)
        {
            case AndroidWindowView.PatternNodeView:
                if (currentMacroSet is null || !_macroSetToPatternsView.ContainsKey(currentMacroSet)) return;
                _macroSetToPatternsView[currentMacroSet].Close();
                return;
            case AndroidWindowView.SettingNodeView:
                if (currentMacroSet is null || !_macroSetToSettingsView.ContainsKey(currentMacroSet)) return;
                _macroSetToSettingsView[currentMacroSet].Close();
                return;
            case AndroidWindowView.ScriptNodeView:
                if (currentMacroSet is null || !_macroSetToScriptsView.ContainsKey(currentMacroSet)) return;
                _macroSetToScriptsView[currentMacroSet].Close();
                return;
            case AndroidWindowView.DailyNodeView:
                if (currentMacroSet is null || !_macroSetToDailiesView.ContainsKey(currentMacroSet)) return;
                _macroSetToDailiesView[currentMacroSet].Close();
                return;
            case AndroidWindowView.WeeklyNodeView:
                if (currentMacroSet is null || !_macroSetToWeekliesView.ContainsKey(currentMacroSet)) return;
                _macroSetToWeekliesView[currentMacroSet].Close();
                return;
            default:
                if (!_views.ContainsKey(view)) return;
                _views[view].Close();
                break;
        }
    }

    public void Cancel(AndroidWindowView view)
    {
        var currentMacroSet = ServiceHelper.GetService<MacroManagerViewModel>()?.SelectedMacroSet;
        switch (view)
        {
            case AndroidWindowView.PatternNodeView:
                if (currentMacroSet is null || !_macroSetToPatternsView.ContainsKey(currentMacroSet)) return;
                _macroSetToPatternsView[currentMacroSet].CloseCancel();
                return;
            case AndroidWindowView.SettingNodeView:
                if (currentMacroSet is null || !_macroSetToSettingsView.ContainsKey(currentMacroSet)) return;
                _macroSetToSettingsView[currentMacroSet].CloseCancel();
                return;
            case AndroidWindowView.ScriptNodeView:
                if (currentMacroSet is null || !_macroSetToScriptsView.ContainsKey(currentMacroSet)) return;
                _macroSetToScriptsView[currentMacroSet].CloseCancel();
                return;
            case AndroidWindowView.DailyNodeView:
                if (currentMacroSet is null || !_macroSetToDailiesView.ContainsKey(currentMacroSet)) return;
                _macroSetToDailiesView[currentMacroSet].CloseCancel();
                return;
            case AndroidWindowView.WeeklyNodeView:
                if (currentMacroSet is null || !_macroSetToWeekliesView.ContainsKey(currentMacroSet)) return;
                _macroSetToWeekliesView[currentMacroSet].CloseCancel();
                return;
            default:
                if (!_views.ContainsKey(view)) return;
                _views[view].CloseCancel();
                break;
        }
    }

    // https://stackoverflow.com/questions/3407256/height-of-status-bar-in-android
    //public Point GetTopLeft()
    //{
    //    var loc = new int[2];
    //    _overlayWindow?.GetLocationOnScreen(loc);
    //    return new Point(loc[0], loc[1]);
    //}

    public Point GetUserDrawViewTopLeft()
    {
        var loc = new int[2];
        if (_views.ContainsKey(AndroidWindowView.UserDrawView))
        {
            var view = (FormsView)_views[AndroidWindowView.UserDrawView];
            view.GetLocationOnScreen(loc);
        }

        return new Point(loc[0], loc[1]);
    }

    public static Rect GetWindowBounds(DisplayRotation rotation)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(29)) return Rect.FromLTRB(0, 0, DisplayHelper.DisplayInfo.Width, DisplayHelper.DisplayInfo.Height);

        // https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/io/github/fate_grand_automata/util/CutoutManager.kt#L53
        //var cutout = OperatingSystem.IsAndroidVersionAtLeast(30) ? 
        //    Platform.CurrentActivity?.Display?.Cutout : 
        //    Platform.CurrentActivity?.Window?.DecorView?.RootWindowInsets?.DisplayCutout;

        //var rect = new global::Android.Graphics.Rect();
        //Platform.CurrentActivity?.Window?.DecorView?.GetWindowVisibleDisplayFrame(rect);
        //var frame = Platform.CurrentActivity?.Window?.DecorView?.RootWindowInsets?.Frame;

        // Should get the correct values as long as you start projection service in an orientation
        // that has the cutout. Otherwise, you get a null cutout
        var cutout = Platform.CurrentActivity?.Window?.DecorView?.RootWindowInsets?.DisplayCutout;
        if (cutout is null) return Rect.FromLTRB(0, 0, DisplayHelper.DisplayInfo.Width, DisplayHelper.DisplayInfo.Height);

        var displayInfo = DisplayHelper.DisplayInfo;
        int top = 0, left = 0;
        int width = (int)displayInfo.Width;
        int height = (int)displayInfo.Height;

        switch (rotation)
        {
            case DisplayRotation.Rotation0:
                top = cutout.SafeInsetTop;
                left = cutout.SafeInsetLeft;
                width -= cutout.SafeInsetLeft - cutout.SafeInsetRight;
                height -= cutout.SafeInsetTop - cutout.SafeInsetBottom;
                break;

            case DisplayRotation.Rotation90:
                top = cutout.SafeInsetRight;
                left = cutout.SafeInsetTop;
                width -= cutout.SafeInsetTop - cutout.SafeInsetBottom;
                height -= cutout.SafeInsetLeft - cutout.SafeInsetRight;
                break;

            case DisplayRotation.Rotation180:
                top = cutout.SafeInsetBottom;
                left = cutout.SafeInsetRight;
                width -= cutout.SafeInsetLeft - cutout.SafeInsetRight;
                height -= cutout.SafeInsetTop - cutout.SafeInsetBottom;
                break;

            case DisplayRotation.Rotation270:
                top = cutout.SafeInsetLeft;
                left = cutout.SafeInsetBottom;
                width -= cutout.SafeInsetTop - cutout.SafeInsetBottom;
                height -= cutout.SafeInsetLeft - cutout.SafeInsetRight;
                break;
        }

        return new Rect(left, top, width, height);
    }

    private void DrawView_Click(object sender, System.EventArgs e)
    {
        Close(AndroidWindowView.DrawView);
    }
}