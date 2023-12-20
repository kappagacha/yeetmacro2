using Android.Content;
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
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using static Android.Graphics.Bitmap;

namespace YeetMacro2.Platforms.Android.Services;

public enum AndroidWindowView
{
    PatternNodeView,
    ScriptNodeView,
    SettingNodeView,
    DailyNodeView,
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
    IWindowManager _windowManager;
    ConcurrentDictionary<AndroidWindowView, IShowable> _views = new ConcurrentDictionary<AndroidWindowView, IShowable>();
    FormsView _overlayWindow;
    ILogger _logger;
    MainActivity _context;
    OpenCvService _openCvService;
    MediaProjectionService _mediaProjectionService;
    IOcrService _ocrService;
    YeetAccessibilityService _accessibilityService;
    IToastService _toastService;
    Size _initialResolution;
    double _density;
    public IReadOnlyDictionary<AndroidWindowView, IShowable> Views => _views;
    public bool IsDrawing { get; set; }
    public int UserDrawViewWidth => _views.ContainsKey(AndroidWindowView.UserDrawView) ? ((FormsView)_views[AndroidWindowView.UserDrawView]).MeasuredHeightAndState : -1;
    public int UserDrawViewHeight => _views.ContainsKey(AndroidWindowView.UserDrawView) ? ((FormsView)_views[AndroidWindowView.UserDrawView]).MeasuredWidthAndState : -1;
    public Size CalcResolution
    {
        get
        {
            var overlayWidth = _overlayWindow?.MeasuredWidthAndState ?? 0;
            var overlayHeight = _overlayWindow?.MeasuredHeightAndState ?? 0;
            var width = overlayWidth > overlayHeight ? Math.Max(_initialResolution.Width, _initialResolution.Height) : Math.Min(_initialResolution.Width, _initialResolution.Height);
            var height = overlayHeight > overlayWidth ? Math.Max(_initialResolution.Width, _initialResolution.Height) : Math.Min(_initialResolution.Width, _initialResolution.Height);
            return new Size(width, height);
        }
    }
    public Size Resolution => new Size(_overlayWindow?.MeasuredWidthAndState ?? 0, _overlayWindow?.MeasuredHeightAndState ?? 0);
    public double Density => _density;
    //public int DisplayCutoutTop => _androidWindowManagerService.OverlayWindow == null ? 0 : _androidWindowManagerService.OverlayWindow.RootWindowInsets.DisplayCutout?.SafeInsetTop ?? 0;
    JsonSerializerOptions _serializationOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = PointPropertiesResolver.Instance
    };
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
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        _initialResolution = new Size(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);
        _density = DeviceDisplay.MainDisplayInfo.Density;
        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, string>(this, nameof(ForegroundService), (r, propertyChangedMessage) =>
        {
            if (propertyChangedMessage.NewValue)    // true means ForegroundService was started
            {
                ShowOverlayWindow();
            }
            else // Foreground Service Exit action
            {
                Close(AndroidWindowView.ActionView);
                Close(AndroidWindowView.StatusPanelView);
                Close(AndroidWindowView.MacroOverlayView);
                Close(AndroidWindowView.PatternNodeView);
                Close(AndroidWindowView.SettingNodeView);
                Close(AndroidWindowView.DailyNodeView);
                Close(AndroidWindowView.TestView);
                Close(AndroidWindowView.DebugDrawView);
                CloseOverlayWindow();
            }
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

    public void DoClick(Point point)
    {
        _accessibilityService.DoClick(point);
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
        if (pattern.IsBoundsPattern)
        {
            return new FindPatternResult()
            {
                IsSuccess = true,
                Point = pattern.Rect.Offset(opts.Offset).Center,
                Points = new Point[] { pattern.Rect.Offset(opts.Offset).Center }
            };
        }

        var points = GetMatches(pattern, opts);

        var result = new FindPatternResult();
        result.IsSuccess = points.Count > 0;
        if (points.Count > 0)
        {
            result.Point = new Point(points[0].X, points[0].Y);
            result.Points = points.ToArray();
        }

        return result;
    }

    public byte[] GetCurrentImageData(Rect rect)
    {
        return _mediaProjectionService.GetCurrentImageData(rect);
    }

    public byte[] ScaleImageData(byte[] data, double scale)
    {
        var bitmap = global::Android.Graphics.BitmapFactory.DecodeByteArray(data, 0, data.Length);
        var targetWidth = bitmap.Width * scale;
        var targetHeight = bitmap.Height * scale;
        var resizedBitmap = global::Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, (int)targetWidth, (int)targetHeight, false);

        MemoryStream ms = new MemoryStream();
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
            var rect = pattern.Rect.Offset(opts.Offset);
            //var rect = opts.OverrideRect != Rect.Zero ? opts.OverrideRect : pattern.Rect;

            watch.Start();
            try
            {
                //var topLeft = GetTopLeftByPackage();
                //haystackImageData = pattern.Rect != Rect.Zero ?
                //    await _mediaProjectionService.GetCurrentImageData(pattern.Rect.Offset(-topLeft.x, -topLeft.y)) :
                //    await _mediaProjectionService.GetCurrentImageData();
                if (pattern.TextMatch.IsActive && !String.IsNullOrEmpty(pattern.TextMatch.Text))
                {
                    haystackImageData = _mediaProjectionService.GetCurrentImageData(
                        new Rect(rect.Location.Offset(-boundsPadding, -boundsPadding),
                        pattern.Rect.Size + new Size(boundsPadding, boundsPadding)));
                }
                else
                {
                    haystackImageData = pattern.Rect != Rect.Zero ?
                       _mediaProjectionService.GetCurrentImageData(rect) :
                       _mediaProjectionService.GetCurrentImageData();
                }
            }
            catch (Exception ex)
            {
                return new List<Point>();
            }

            if (haystackImageData == null)
            {
                return new List<Point>();
            }

            if (pattern.ColorThreshold.IsActive)
            {
                needleImageData = pattern.ColorThreshold.ImageData; // OpenCvHelper.CalcColorThreshold(pattern.ImageData, pattern.ColorThreshold);
                haystackImageData = _openCvService.CalcColorThreshold(haystackImageData, pattern.ColorThreshold);

                if (needleImageData.Length == 0 || haystackImageData.Length == 0)
                {
                    return new List<Point>();
                }
            }

            var threshold = 0.8;
            if (pattern.VariancePct != 0.0) threshold = 1 - pattern.VariancePct / 100;
            if ((opts?.VariancePct ?? 0.0) != 0.0) threshold = opts.VariancePct;

            if (pattern.TextMatch.IsActive && !String.IsNullOrEmpty(pattern.TextMatch.Text))
            {
                var text = _ocrService.GetText(haystackImageData, pattern.TextMatch.WhiteList);
                var textPoints = new List<Point>();

                if (text == pattern.TextMatch.Text && pattern.Rect != Rect.Zero)
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

            var points = _openCvService.GetPointsWithMatchTemplate(haystackImageData, needleImageData, opts?.Limit ?? 1, threshold);
            if (pattern.Rect != Rect.Zero)
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
            return new List<Point>();
        }
    }

    public string GetText(Pattern pattern, TextFindOptions opts)
    {
        _logger.LogTrace("AndroidWindowManagerService GetText");
        var boundsPadding = 4;
        var currentImageData = pattern.Rect != Rect.Zero ?
            _mediaProjectionService.GetCurrentImageData(
                new Rect(pattern.Rect.Location.Offset(opts.Offset.X, opts.Offset.Y).Offset(-boundsPadding, -boundsPadding),
                          pattern.Rect.Size + new Size(boundsPadding, boundsPadding))) :
            _mediaProjectionService.GetCurrentImageData();
        var imageData = pattern.ColorThreshold.IsActive ?
            _openCvService.CalcColorThreshold(currentImageData, pattern.ColorThreshold) :
            currentImageData;

        return _ocrService.GetText(imageData, pattern.TextMatch.WhiteList ?? opts.Whitelist);
    }

    public string GetText(byte[] currentImage)
    {
        _logger.LogTrace("AndroidWindowManagerService GetText");
        return _ocrService.GetText(currentImage);
    }

    public void ShowMessage(string message)
    {
        Show(AndroidWindowView.MessageView);
        var viewModel = (MessageViewModel)_views[AndroidWindowView.MessageView].VisualElement.BindingContext;
        viewModel.Message = message;
    }

    public void ScreenCapture()
    {
        _mediaProjectionService.Start();
        _mediaProjectionService.TakeScreenCapture();
        _mediaProjectionService.Stop();
    }

    public async Task StartProjectionService()
    {
        _context.StartForegroundServiceCompat<ForegroundService>();
        await _mediaProjectionService.EnsureProjectionServiceStarted();
    }

    public void StopProjectionService()
    {
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

            if (_views.ContainsKey(AndroidWindowView.ActionView) && _views[AndroidWindowView.ActionView].IsShowing)
            {
                Close(AndroidWindowView.ActionView);
                Show(AndroidWindowView.ActionView);
            }
        }
    }

    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        if (_views.ContainsKey(AndroidWindowView.ActionView) && _views[AndroidWindowView.ActionView].IsShowing)
        {
            Close(AndroidWindowView.ActionView);
            Show(AndroidWindowView.ActionView);
        }
    }

    public void ShowOverlayWindow()
    {
        if (_overlayWindow == null)
        {
            var grid = new Grid() { InputTransparent = true, CascadeInputTransparent = true };
            _overlayWindow = new FormsView(_context, _windowManager, grid) { IsModal = false };
            _overlayWindow.SetIsTouchable(false);
            _overlayWindow.SetBackgroundToTransparent();
            _overlayWindow.DisableTranslucentNavigation();
        }

        //Get overlay permissin if needed
        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M && !Settings.CanDrawOverlays(_context))
        {
            _context.StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission, global::Android.Net.Uri.Parse("package:" + _context.PackageName)), OVERLAY_SERVICE_REQUEST);
            return;
        }

        _overlayWindow.Show();
    }

    public void CloseOverlayWindow()
    {
        _overlayWindow?.Close();
    }

    public void Show(AndroidWindowView windowView)
    {
        //Get overlay permission if needed
        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M && !Settings.CanDrawOverlays(_context))
        {
            _context.StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission, global::Android.Net.Uri.Parse("package:" + _context.PackageName)), OVERLAY_SERVICE_REQUEST);
            return;
        }

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
                    });
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
                    var patternsNodeView = new ResizeView(_context, _windowManager, this, new PatternNodeView());
                    _views.TryAdd(windowView, patternsNodeView);
                    patternsNodeView.OnShow = () => {
                        if (_views.ContainsKey(AndroidWindowView.ScriptNodeView) && _views[AndroidWindowView.ScriptNodeView].IsShowing)
                        {
                            IsDrawing = true;
                            Close(AndroidWindowView.ScriptNodeView);
                            IsDrawing = false;
                        }
                        else if (!IsDrawing)
                        {
                            _mediaProjectionService.Start();
                        }
                        Show(AndroidWindowView.MacroOverlayView);
                    };
                    patternsNodeView.OnClose = () => { 
                        Close(AndroidWindowView.MacroOverlayView); 
                        if (!IsDrawing) _mediaProjectionService.Stop();
                        ServiceHelper.GetService<AndriodHomeViewModel>().ShowPatternNodeView = false;
                    };
                    break;
                case AndroidWindowView.ScriptNodeView:
                    var scriptNodeView = new ResizeView(_context, _windowManager, this, new ScriptNodeView() { ShowExecuteButton = true });
                    _views.TryAdd(windowView, scriptNodeView);
                    scriptNodeView.OnShow = () => { Show(AndroidWindowView.MacroOverlayView); if (!IsDrawing) _mediaProjectionService.Start(); };
                    scriptNodeView.OnClose = () => { Close(AndroidWindowView.MacroOverlayView); if (!IsDrawing) _mediaProjectionService.Stop(); };
                    break;
                case AndroidWindowView.SettingNodeView:
                    var settingNodeView = new ResizeView(_context, _windowManager, this, new SettingNodeView());
                    _views.TryAdd(windowView, settingNodeView);
                    settingNodeView.OnClose = () => ServiceHelper.GetService<AndriodHomeViewModel>().ShowSettingNodeView = false;
                    break;
                case AndroidWindowView.DailyNodeView:
                    var dailyNodeView = new ResizeView(_context, _windowManager, this, new DailyNodeView());
                    _views.TryAdd(windowView, dailyNodeView);
                    dailyNodeView.OnClose = () => ServiceHelper.GetService<AndriodHomeViewModel>().ShowDailyNodeView = false;
                    break;
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
                    drawView.DisableTranslucentNavigation();
                    _views.TryAdd(windowView, drawView);
                    break;
                case AndroidWindowView.DebugDrawView:
                    var debugDrawControl = new DrawControl() { InputTransparent = true, CascadeInputTransparent = true };
                    var debugDrawView = new FormsView(_context, _windowManager, debugDrawControl) { IsModal = false };
                    debugDrawView.SetIsTouchable(false);
                    debugDrawView.SetBackgroundToTransparent();
                    debugDrawView.DisableTranslucentNavigation();
                    _views.TryAdd(windowView, debugDrawView);
                    break;
                case AndroidWindowView.MessageView:
                    var messageView = new ResizeView(_context, _windowManager, this, new MessageView());
                    _views.TryAdd(windowView, messageView);
                    break;
                case AndroidWindowView.TestView:
                    var testView = new ResizeView(_context, _windowManager, this, new TestView());
                    testView.OnClose = () => ServiceHelper.GetService<AndriodHomeViewModel>().ShowTestView = false;
                    _views.TryAdd(windowView, testView);
                    break;
            }
        }

        if (windowView == AndroidWindowView.ActionView)
        {
            var selectedMacroSetName = Preferences.Default.Get<string>(nameof(MacroManagerViewModel.SelectedMacroSet), null);
            if (selectedMacroSetName is not null)
            {
                var ve = _views[windowView].VisualElement;
                var ctx = (IMovable)ve.BindingContext;
                var orientation = DeviceDisplay.Current.MainDisplayInfo.Orientation;
                var preferenceKey = $"{selectedMacroSetName}_location_{orientation}";
                var strTargetLocation = Preferences.Default.Get<string>(preferenceKey, null);
                if (strTargetLocation is not null)
                {
                    var targetLocation = JsonSerializer.Deserialize<Point>(strTargetLocation, _serializationOptions);
                    ctx.Location = targetLocation;
                }
                else
                {
                    ctx.Location = Point.Zero;
                }
            }
        }

        _views[windowView].Show();

        if (windowView == AndroidWindowView.MacroOverlayView)
        {
            var ve = _views[windowView].VisualElement;
            var ctx = ve.BindingContext;
            ve.BindingContext = null;
            ve.BindingContext = ctx;
        }
    }

    public void Close(AndroidWindowView view)
    {
        if (!_views.ContainsKey(view)) return;
        _views[view].Close();
    }

    public void Cancel(AndroidWindowView view)
    {
        if (!_views.ContainsKey(view)) return;
        _views[view]?.CloseCancel();
    }

    // https://stackoverflow.com/questions/3407256/height-of-status-bar-in-android
    public Point GetTopLeft()
    {
        var loc = new int[2];
        _overlayWindow?.GetLocationOnScreen(loc);
        return new Point(loc[0], loc[1]);
    }

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

    private void DrawView_Click(object sender, System.EventArgs e)
    {
        Close(AndroidWindowView.DrawView);
    }
}