using Android.Views;
using Android.Content;
using Android.Runtime;
using Android.Provider;
using System.Collections.Concurrent;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Views;
using YeetMacro2.Data.Models;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Services;
using OpenCvHelper = YeetMacro2.Platforms.Android.Services.OpenCv.OpenCvHelper;
using System.Text.Json;
using YeetMacro2.ViewModels;
using YeetMacro2.Data.Serialization;
using Microsoft.Extensions.Logging;
using TesseractOcrMaui;

namespace YeetMacro2.Platforms.Android.Services;
public enum AndroidWindowView
{
    PatternsNodeView,
    ScriptsNodeView,
    PatternsView,
    DrawView,
    UserDrawView,
    DebugDrawView,
    ActionView,
    ActionMenuView,
    PromptStringInputView,
    PromptSelectOptionView,
    StatusPanelView,
    MacroOverlayView,
    MessageView,
    TestView
}

public class AndroidWindowManagerService : IInputService, IScreenService
{
    ILogger _logger;
    public const int OVERLAY_SERVICE_REQUEST = 0;
    public const int REQUEST_IGNORE_BATTERY_OPTIMIZATIONS = 2;
    private MainActivity _context;
    IWindowManager _windowManager;
    MediaProjectionService _mediaProjectionService;
    IToastService _toastService;
    IOcrService _ocrService;
    ConcurrentDictionary<AndroidWindowView, IShowable> _views = new ConcurrentDictionary<AndroidWindowView, IShowable>();
    FormsView _windowView;
    ConcurrentDictionary<string, (int x, int y)> _packageToStatusBarHeight = new ConcurrentDictionary<string, (int x, int y)>();
    //double _displayWidth, _displayHeight;
    public int OverlayWidth => _windowView == null ? 0 : _windowView.MeasuredWidthAndState;
    public int OverlayHeight => _windowView == null ? 0 : _windowView.MeasuredHeightAndState;
    //public int DisplayCutoutTop => _windowView == null ? 0 : _windowView.RootWindowInsets.DisplayCutout?.SafeInsetTop ?? 0;
    JsonSerializerOptions _serializationOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = PointPropertiesResolver.Instance
    };
    public AndroidWindowManagerService(ILogger<AndroidWindowManagerService> logger, MediaProjectionService mediaProjectionService,
        IToastService toastService, IOcrService ocrService)
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] Setting _logger");
            _logger = logger;
            Console.WriteLine("[*****YeetMacro*****] Setting _context");
            _context = (MainActivity)Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            Console.WriteLine("[*****YeetMacro*****] Setting _windowManager");
            _windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            Console.WriteLine("[*****YeetMacro*****] Setting _mediaProjectionService");
            _mediaProjectionService = mediaProjectionService;
            Console.WriteLine("[*****YeetMacro*****] Setting _toastService");
            _toastService = toastService;
            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
            //_displayWidth = DeviceDisplay.MainDisplayInfo.Width;
            //_displayHeight = DeviceDisplay.MainDisplayInfo.Height;
            Console.WriteLine("[*****YeetMacro*****] Setting _ocrService");
            _ocrService = ocrService;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AndroidWindowManagerService {ex.Message}");
            Console.WriteLine($"AndroidWindowManagerService {ex.StackTrace}");
        }
    }

    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        Console.WriteLine("[*****YeetMacro*****] WindowManagerService DeviceDisplay_MainDisplayInfoChanged Start");
        //_displayWidth = e.DisplayInfo.Width;
        //_displayHeight = e.DisplayInfo.Height;
        _packageToStatusBarHeight.Clear();
        if (_views.ContainsKey(AndroidWindowView.ActionView) && _views[AndroidWindowView.ActionView].IsShowing)
        {
            Close(AndroidWindowView.ActionView);
            Show(AndroidWindowView.ActionView);
        }
        Console.WriteLine("[*****YeetMacro*****] WindowManagerService DeviceDisplay_MainDisplayInfoChanged End");
    }

    public void ShowOverlayWindow()
    {
        if (_windowView == null)
        {
            var grid = new Grid() { InputTransparent = true, CascadeInputTransparent = true };
            _windowView = new FormsView(_context, _windowManager, grid) { IsModal = false };
            _windowView.SetIsTouchable(false);
            _windowView.SetBackgroundToTransparent();
            _windowView.DisableTranslucentNavigation();
        }

        //Get overlay permissin if needed
        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M && !Settings.CanDrawOverlays(_context))
        {
            _context.StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission, global::Android.Net.Uri.Parse("package:" + _context.PackageName)), OVERLAY_SERVICE_REQUEST);
            return;
        }

        _windowView.Show();
    }

    public void CloseOverlayWindow()
    {
        _windowView?.Close();
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
                case AndroidWindowView.ActionMenuView:
                    var actionMenuView = new FormsView(_context, _windowManager, new ActionMenu());
                    _views.TryAdd(windowView, actionMenuView);
                    break;
                case AndroidWindowView.PatternsNodeView:
                    var patternsNodeView = new ResizeView(_context, _windowManager, this, new PatternNodeView());
                    _views.TryAdd(windowView, patternsNodeView);
                    patternsNodeView.OnShow = () => Show(AndroidWindowView.MacroOverlayView);
                    patternsNodeView.OnClose = () => Close(AndroidWindowView.MacroOverlayView);
                    break;
                case AndroidWindowView.ScriptsNodeView:
                    var scriptsNodeView = new ResizeView(_context, _windowManager, this, new ScriptNodeView() { ShowExecuteButton = true });
                    _views.TryAdd(windowView, scriptsNodeView);
                    scriptsNodeView.OnShow = () => Show(AndroidWindowView.MacroOverlayView);
                    scriptsNodeView.OnClose = () => Close(AndroidWindowView.MacroOverlayView);
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

        if (windowView == AndroidWindowView.ActionMenuView || windowView == AndroidWindowView.MacroOverlayView)
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

    public async Task<string> PromptInput(string message)
    {
        Show(AndroidWindowView.PromptStringInputView);
        var viewModel = (PromptStringInputViewModel)_views[AndroidWindowView.PromptStringInputView].VisualElement.BindingContext;
        viewModel.Message = message;
        var formsView = (FormsView)_views[AndroidWindowView.PromptStringInputView];
        if (await formsView.WaitForClose())
        {
            return viewModel.Input;
        }
        return null;
    }

    public async Task<string> SelectOption(string message, params string[] options)
    {
        Show(AndroidWindowView.PromptSelectOptionView);
        var viewModel = (PromptSelectOptionViewModel)_views[AndroidWindowView.PromptSelectOptionView].VisualElement.BindingContext;
        viewModel.Message = message;
        // Needs x:DataType="{x:Type x:String}" in DataTemplate
        // https://stackoverflow.com/questions/75283345/collectionview-working-in-debug-but-not-in-release-in-net-maui
        viewModel.Options = options;
        var formsView = (FormsView)_views[AndroidWindowView.PromptSelectOptionView];
        if (await formsView.WaitForClose())
        {
            return viewModel.SelectedOption;
        }
        return null;
    }

    public void ShowMessage(string message)
    {
        Show(AndroidWindowView.MessageView);
        var viewModel = (MessageViewModel)_views[AndroidWindowView.MessageView].VisualElement.BindingContext;
        viewModel.Message = message;
    }

    public async Task<Rect> DrawUserRectangle()
    {
        var patternsViewIsShowing = _views.TryGetValue(AndroidWindowView.PatternsNodeView, out IShowable patternsView) && patternsView.IsShowing;
        var macroOverlayViewIsShowing = _views.TryGetValue(AndroidWindowView.MacroOverlayView, out IShowable macroOverlayView) && macroOverlayView.IsShowing;
        var scriptsViewIsShowing = _views.TryGetValue(AndroidWindowView.ScriptsNodeView, out IShowable scriptsView) && scriptsView.IsShowing;
        if (patternsViewIsShowing) _views[AndroidWindowView.PatternsNodeView].Close();
        if (macroOverlayViewIsShowing) _views[AndroidWindowView.MacroOverlayView].Close();
        if (scriptsViewIsShowing) _views[AndroidWindowView.ScriptsNodeView].Close();

        Show(AndroidWindowView.UserDrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.UserDrawView].VisualElement;
        drawControl.ClearRectangles();
        var formsView = (FormsView)_views[AndroidWindowView.UserDrawView];
        if (await formsView.WaitForClose())
        {
            if (patternsViewIsShowing) _views[AndroidWindowView.PatternsNodeView].Show();
            if (macroOverlayViewIsShowing) _views[AndroidWindowView.MacroOverlayView].Show();
            if (scriptsViewIsShowing) _views[AndroidWindowView.ScriptsNodeView].Show();
            return drawControl.Rect;
        }
        return Rect.Zero;
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

    public void DrawCircle(Point point)
    {
        Show(AndroidWindowView.DrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.DrawView].VisualElement;
        var drawView = (FormsView)_views[AndroidWindowView.DrawView];
        drawView.SetIsTouchable(true);
        drawControl.AddCircle(point);
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

    // https://stackoverflow.com/questions/3407256/height-of-status-bar-in-android
    public (int x, int y) GetTopLeftByPackage()
    {
        //var currentPackage = _accessibilityService.CurrentPackage;

        //if (_packageToStatusBarHeight.ContainsKey(currentPackage))
        //{
        //    return _packageToStatusBarHeight[currentPackage];
        //}

        var topLeft = GetTopLeft();

        //if (currentPackage != "unknown")
        //{
        //    _packageToStatusBarHeight.TryAdd(currentPackage, topLeft);
        //}

        return topLeft;
    }

    // https://stackoverflow.com/questions/3407256/height-of-status-bar-in-android
    public (int x, int y) GetTopLeft()
    {
        var loc = new int[2];
        _windowView?.GetLocationOnScreen(loc);
        var topLeft = (x: loc[0], y: loc[1]);

        return topLeft;
    }

    public void LaunchYeetMacro()
    {
        AndroidServiceHelper.LaunchApp(_context.PackageName);
    }

    public bool ProjectionServiceEnabled { get => _mediaProjectionService.Enabled; }

    public bool IsIgnoringBatteryOptimizations
    {
        get
        {
            var pm = (global::Android.OS.PowerManager)_context.GetSystemService(Context.PowerService);
            return pm.IsIgnoringBatteryOptimizations(AppInfo.PackageName);
        }
    }

    public void RequestAccessibilityPermissions()
    {
        AndroidServiceHelper.StartAccessibilityService();
    }

    public void RevokeAccessibilityPermissions()
    {
        AndroidServiceHelper.AccessibilityService?.Stop();
    }

    // https://stackoverflow.com/questions/39256501/check-if-battery-optimization-is-enabled-or-not-for-an-app
    public void RequestIgnoreBatteryOptimizations()
    {
        if (IsIgnoringBatteryOptimizations) return;
        var intent = new Intent(Settings.ActionRequestIgnoreBatteryOptimizations, global::Android.Net.Uri.Parse($"package:{AppInfo.PackageName}"));
        _context.StartActivityForResult(intent, REQUEST_IGNORE_BATTERY_OPTIMIZATIONS);
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

    private void DrawView_Click(object sender, System.EventArgs e)
    {
        Close(AndroidWindowView.DrawView);
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

                haystackImageData = pattern.Rect != Rect.Zero ?
                    _mediaProjectionService.GetCurrentImageData(rect) :
                    _mediaProjectionService.GetCurrentImageData();
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
                needleImageData = OpenCvHelper.CalcColorThreshold(pattern.ImageData, pattern.ColorThreshold);
                haystackImageData = OpenCvHelper.CalcColorThreshold(haystackImageData, pattern.ColorThreshold);

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

            var points = OpenCvHelper.GetPointsWithMatchTemplate(haystackImageData, needleImageData, opts?.Limit ?? 1, threshold);
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

    public void DoClick(Point point)
    {
        AndroidServiceHelper.AccessibilityService?.DoClick(point);
    }

    public void DoSwipe(Point start, Point end)
    {
        AndroidServiceHelper.AccessibilityService?.DoSwipe(start, end);
    }

    public void ScreenCapture()
    {
        _mediaProjectionService.TakeScreenCapture();
    }

    public void StartRecording()
    {
        _mediaProjectionService.StartRecording();
    }

    public void StopRecording()
    {
        _mediaProjectionService.StopRecording();
    }

    public byte[] CalcColorThreshold(Pattern pattern, ColorThresholdProperties colorThreshold)
    {
        return OpenCvHelper.CalcColorThreshold(pattern.ImageData, colorThreshold);
    }

    public byte[] GetCurrentImageData(Rect rect)
    {
        return _mediaProjectionService.GetCurrentImageData(rect);
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
            OpenCvHelper.CalcColorThreshold(currentImageData, pattern.ColorThreshold) :
            currentImageData;
 
        return _ocrService.GetText(imageData, pattern.TextMatch.WhiteList ?? opts.Whitelist);
    }

    public string GetText(byte[] currentImage)
    {
        _logger.LogTrace("AndroidWindowManagerService GetText");
        return _ocrService.GetText(currentImage);
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

    public FindPatternResult ClickPattern(Pattern pattern, FindOptions opts = null)
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
}