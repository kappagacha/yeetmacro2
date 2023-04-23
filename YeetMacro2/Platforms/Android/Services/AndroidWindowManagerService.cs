using Android.Views;
using Android.Content;
using Android.Runtime;
using Android.Provider;
using System.Collections.Concurrent;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Views;
using YeetMacro2.Data.Models;
using Point = Microsoft.Maui.Graphics.Point;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Services;
using OpenCvHelper = YeetMacro2.Platforms.Android.Services.OpenCv.OpenCvHelper;
using Tesseract.Droid;

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
    LogView
}

public class AndroidWindowManagerService : IInputService, IScreenService
{
    public const int OVERLAY_SERVICE_REQUEST = 0;
    private MainActivity _context;
    IWindowManager _windowManager;
    MediaProjectionService _mediaProjectionService;
    YeetAccessibilityService _accessibilityService;
    ConcurrentDictionary<AndroidWindowView, IShowable> _views = new ConcurrentDictionary<AndroidWindowView, IShowable>();
    FormsView _windowView;
    ConcurrentDictionary<string, (int x, int y)> _packageToStatusBarHeight = new ConcurrentDictionary<string, (int x, int y)>();
    double _displayWidth, _displayHeight;
    public int OverlayWidth => _windowView == null ? 0 : _windowView.MeasuredWidthAndState;
    public int OverlayHeight => _windowView == null ? 0 : _windowView.MeasuredHeightAndState;
    public int DisplayCutoutTop => _windowView == null ? 0 : _windowView.RootWindowInsets.DisplayCutout?.SafeInsetTop ?? 0;
    TesseractApi _tesseractApi;
    public AndroidWindowManagerService(MediaProjectionService mediaProjectionService, YeetAccessibilityService accessibilityService)
    {
        _context = (MainActivity)Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        _windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        _mediaProjectionService = mediaProjectionService;
        _accessibilityService = accessibilityService;

        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        _displayWidth = DeviceDisplay.MainDisplayInfo.Width;
        _displayHeight = DeviceDisplay.MainDisplayInfo.Height;

        // https://github.com/halkar/Tesseract.Xamarin
        // https://stackoverflow.com/questions/52157436/q-system-invalidoperationexception-call-init-first-ocr-tesseract-error-in-xa
        _tesseractApi = new TesseractApi(_context, AssetsDeployment.OncePerVersion);
        _ = _tesseractApi.Init("eng").Result;
    }

    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        Console.WriteLine("[*****YeetMacro*****] WindowManagerService DeviceDisplay_MainDisplayInfoChanged Start");
        _displayWidth = e.DisplayInfo.Width;
        _displayHeight = e.DisplayInfo.Height;
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
        if (!Settings.CanDrawOverlays(_context))
        {
            _context.StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission, global::Android.Net.Uri.Parse("package:" + _context.PackageName)), OVERLAY_SERVICE_REQUEST);
            return;
        }

        _windowView.Show();
    }

    public void CloseOverlayWindow()
    {
        if (_windowView != null)
        {
            _windowView.Close();
        }
    }

    public void Show(AndroidWindowView windowView)
    {
        //Get overlay permissin if needed
        if (!Settings.CanDrawOverlays(_context))
        {
            _context.StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission, global::Android.Net.Uri.Parse("package:" + _context.PackageName)), OVERLAY_SERVICE_REQUEST);
            return;
        }

        if (!_views.ContainsKey(windowView))
        {
            switch (windowView)
            {
                case AndroidWindowView.LogView:
                    var logAndroidView = new StaticView(_context, _windowManager, new LogView());
                    logAndroidView.SetUpLayoutParameters(lp =>
                    {
                        lp.Gravity = GravityFlags.Bottom;
                        lp.Width = WindowManagerLayoutParams.MatchParent;
                    });
                    _views.TryAdd(windowView, logAndroidView);
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
                    break;
                case AndroidWindowView.ScriptsNodeView:
                    var scriptsNodeView = new ResizeView(_context, _windowManager, this, new ScriptNodeView() { ShowExecuteButton = true });
                    _views.TryAdd(windowView, scriptsNodeView);
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
            }
        }

        _views[windowView].Show();

        if (windowView == AndroidWindowView.ActionMenuView)
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
        viewModel.Options = options;
        var formsView = (FormsView)_views[AndroidWindowView.PromptSelectOptionView];
        if (await formsView.WaitForClose())
        {
            return viewModel.SelectedOption;
        }
        return null;
    }

    public async Task<(Point start, Point end)> DrawUserRectangle()
    {
        Show(AndroidWindowView.UserDrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.UserDrawView].VisualElement;
        drawControl.ClearRectangles();
        var formsView = (FormsView)_views[AndroidWindowView.UserDrawView];
        if (await formsView.WaitForClose())
        {
            return (drawControl.Start, drawControl.End);
        }
        return (Point.Zero, Point.Zero);
    }

    public void DrawClear()
    {
        if (!_views.ContainsKey(AndroidWindowView.DrawView)) return;

        var drawControl = (DrawControl)_views[AndroidWindowView.DrawView].VisualElement;
        drawControl.ClearCircles();
        drawControl.ClearRectangles();
    }

    public void DrawRectangle(Point start, Point end)
    {
        Show(AndroidWindowView.DrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.DrawView].VisualElement;
        var drawView = (FormsView)_views[AndroidWindowView.DrawView];
        drawView.SetIsTouchable(true);
        drawControl.AddRectangle(start, end);
    }

    public void DrawCircle(Point point)
    {
        Show(AndroidWindowView.DrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.DrawView].VisualElement;
        var drawView = (FormsView)_views[AndroidWindowView.DrawView];
        drawView.SetIsTouchable(true);
        drawControl.AddCircle(point);
    }

    public void DebugRectangle(Point start, Point end)
    {
        var width = end.X - start.X;
        var height = end.Y - start.Y;
        Show(AndroidWindowView.DebugDrawView);
        var drawControl = (DrawControl)_views[AndroidWindowView.DebugDrawView].VisualElement;
        var thickness = 10;
        drawControl.AddRectangle(new Point(start.X - thickness, start.Y - thickness), new Point(end.X + thickness * 2, end.Y + thickness * 2));
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
        var currentPackage = _accessibilityService.CurrentPackage;

        if (_packageToStatusBarHeight.ContainsKey(currentPackage))
        {
            return _packageToStatusBarHeight[currentPackage];
        }

        var topLeft = GetTopLeft();

        if (currentPackage != "unknown")
        {
            _packageToStatusBarHeight.TryAdd(currentPackage, topLeft);
        }

        return topLeft;
    }

    // https://stackoverflow.com/questions/3407256/height-of-status-bar-in-android
    public (int x, int y) GetTopLeft()
    {
        var loc = new int[2];
        _windowView.GetLocationOnScreen(loc);
        var topLeft = (x: loc[0], y: loc[1]);

        return topLeft;
    }

    public void LaunchYeetMacro()
    {
        AndroidServiceHelper.LaunchApp(_context.PackageName);
    }

    public bool ProjectionServiceEnabled { get => _mediaProjectionService.Enabled; }

    public void RequestAccessibilityPermissions()
    {
        _accessibilityService.Start();
    }

    public void RevokeAccessibilityPermissions()
    {
        _accessibilityService.Stop();
    }

    public void StartProjectionService()
    {
        _mediaProjectionService.Start();
        _context.StartForegroundServiceCompat<ForegroundService>();
    }

    public void StopProjectionService()
    {
        _context.StartForegroundServiceCompat<ForegroundService>(ForegroundService.EXIT_ACTION);
    }

    private void DrawView_Click(object sender, System.EventArgs e)
    {
        Close(AndroidWindowView.DrawView);
    }

    public async Task<List<Point>> GetMatches(Pattern pattern, FindOptions opts)
    {
        try
        {
            var boundsPadding = 4;
            byte[] needleImageData = pattern.ImageData;
            byte[] haystackImageData = null;

            try
            {
                haystackImageData = pattern.Bounds != null ?
                    await _mediaProjectionService.GetCurrentImageData(pattern.Bounds.Start, pattern.Bounds.End) :
                    await _mediaProjectionService.GetCurrentImageData();
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
            }

            var threshold = 0.8;
            if (pattern.VariancePct != 0.0) threshold = 1 - pattern.VariancePct / 100;
            if ((opts?.VariancePct ?? 0.0) != 0.0) threshold = opts.VariancePct;


            if (pattern.TextMatch.IsActive && !String.IsNullOrEmpty(pattern.TextMatch.Text))
            {
                if (!String.IsNullOrWhiteSpace(pattern.TextMatch.WhiteList)) _tesseractApi.SetWhitelist(pattern.TextMatch.WhiteList);
                await _tesseractApi.SetImage(haystackImageData);

                var textPoints = new List<Point>();
                if (_tesseractApi.Text == pattern.TextMatch.Text && pattern.Bounds != null)
                {
                    textPoints.Add(new Point(
                       (int)((pattern.Bounds.Start.X + pattern.Bounds.End.X - boundsPadding) / 2.0),
                       (int)((pattern.Bounds.Start.Y + pattern.Bounds.End.Y - boundsPadding) / 2.0)));
                }
                else if (_tesseractApi.Text == pattern.TextMatch.Text)  // TextMatch is not meant to be used on whole screen
                {
                    // TODO: Throw exception instead?
                    textPoints.Add(new Point(0, 0));
                }

                _tesseractApi.SetWhitelist("");
                return textPoints;
            }

            var points = OpenCvHelper.GetPointsWithMatchTemplate(haystackImageData, needleImageData, opts?.Limit ?? 1, threshold);
            if (pattern.Bounds != null)
            {
                var newPoints = new List<Point>();
                for (int i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    newPoints.Add(new Point(
                        (int)(point.X + (pattern.Bounds.Start.X - boundsPadding)),
                        (int)(point.Y + (pattern.Bounds.Start.Y - boundsPadding))));
                }
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
        _accessibilityService.DoClick(point);
    }

    public void DoSwipe(Point start, Point end)
    {
        _accessibilityService.DoSwipe(start, end);
    }

    public async Task ScreenCapture()
    {
        await _mediaProjectionService.TakeScreenCapture();
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

    public async Task<byte[]> GetCurrentImageData(Point start, Point end)
    {
        return await _mediaProjectionService.GetCurrentImageData(start, end);
    }

    public async Task<string> GetText(Pattern pattern)
    {
        var boundsPadding = 4;
        var currentImageData = pattern.Bounds != null ?
            await _mediaProjectionService.GetCurrentImageData(
                new Point(pattern.Bounds.Start.X - boundsPadding, pattern.Bounds.Start.Y - boundsPadding),
                new Point(pattern.Bounds.End.X + boundsPadding, pattern.Bounds.End.Y + boundsPadding)) :
            await _mediaProjectionService.GetCurrentImageData();
        if (!String.IsNullOrWhiteSpace(pattern.TextMatch.WhiteList)) _tesseractApi.SetWhitelist(pattern.TextMatch.WhiteList);
        await _tesseractApi.SetImage(pattern.ColorThreshold.IsActive ?
            OpenCvHelper.CalcColorThreshold(currentImageData, pattern.ColorThreshold):
            currentImageData);
        _tesseractApi.SetWhitelist("");

        return _tesseractApi.Text;
    }

    public async Task<FindPatternResult> FindPattern(Pattern pattern, FindOptions opts = null)
    {
        var points = await GetMatches(pattern, opts);

        var result = new FindPatternResult();
        result.IsSuccess = points.Count > 0;
        if (points.Count > 0)
        {
            result.Point = new Point(points[0].X, points[0].Y);
            result.Points = points.ToArray();
        }

        return result;
    }

    public async Task<FindPatternResult> ClickPattern(Pattern pattern)
    {
        var result = await FindPattern(pattern);
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