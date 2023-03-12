using Android.Views;
using Android.Content;
using Android.Runtime;
using Android.Provider;
using System.Collections.Concurrent;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Views;
using YeetMacro2.Data.Models;
using Android.Graphics;
using Point = Microsoft.Maui.Graphics.Point;
using YeetMacro2.Platforms.Android.Services.OpenCv;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Services;
public enum WindowView
{
    PatternsTreeView,
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
    ConcurrentDictionary<WindowView, IShowable> _views = new ConcurrentDictionary<WindowView, IShowable>();
    FormsView _windowView;
    ConcurrentDictionary<string, (int x, int y)> _packageToStatusBarHeight = new ConcurrentDictionary<string, (int x, int y)>();
    double _displayWidth, _displayHeight;
    public int OverlayWidth => _windowView == null ? 0 : _windowView.MeasuredWidthAndState;
    public int OverlayHeight => _windowView == null ? 0 : _windowView.MeasuredHeightAndState;
    public int DisplayCutoutTop => _windowView == null ? 0 : _windowView.RootWindowInsets.DisplayCutout?.SafeInsetTop ?? 0;
    public AndroidWindowManagerService(MediaProjectionService mediaProjectionService, YeetAccessibilityService accessibilityService)
    {
        _context = (MainActivity)Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        _windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        _mediaProjectionService = mediaProjectionService;
        _accessibilityService = accessibilityService;

        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        _displayWidth = DeviceDisplay.MainDisplayInfo.Width;
        _displayHeight = DeviceDisplay.MainDisplayInfo.Height;
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

    public void Show(WindowView windowView)
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
                case WindowView.LogView:
                    var logView = new LogView();
                    var logAndroidView = new StaticView(_context, _windowManager, logView);
                    logAndroidView.SetUpLayoutParameters(lp =>
                    {
                        lp.Gravity = GravityFlags.Bottom;
                        lp.Width = WindowManagerLayoutParams.MatchParent;
                    });
                    _views.TryAdd(windowView, logAndroidView);
                    break;
                case WindowView.ActionView:
                    var actionControl = new ActionControl();
                    var actionView = new MoveView(_context, _windowManager, actionControl);
                    _views.TryAdd(windowView, actionView);
                    break;
                case WindowView.ActionMenuView:
                    var actionMenu = new ActionMenu();
                    var actionMenuView = new FormsView(_context, _windowManager, actionMenu);
                    _views.TryAdd(windowView, actionMenuView);
                    break;
                case WindowView.PatternsTreeView:
                    var patternsTree = new PatternNodeView();
                    var patternsTreeView = new ResizeView(_context, _windowManager, this, patternsTree);
                    //var patternsTreeView = new FormsView(_context, _windowManager, patternsTree);
                    _views.TryAdd(windowView, patternsTreeView);
                    break;
                //case WindowView.PatternsView:
                //    var patternsControl = new PatternView() { Parent = Xamarin.Forms.Application.Current };
                //    var patternsView = new ResizeView(_context, _windowManager, this, patternsControl);
                //    patternsView.Focusable = false;
                //    _views.TryAdd(windowView, patternsView);
                //    break;
                case WindowView.PromptStringInputView:
                    var promptStringInput = new PromptStringInput();
                    var promptStringInputView = new FormsView(_context, _windowManager, promptStringInput);
                    _views.TryAdd(windowView, promptStringInputView);
                    break;
                case WindowView.PromptSelectOptionView:
                    var promptSelectOption = new PromptSelectOption();
                    var promptSelectOptionView = new FormsView(_context, _windowManager, promptSelectOption);
                    _views.TryAdd(windowView, promptSelectOptionView);
                    break;
                case WindowView.UserDrawView:
                    var userdrawControl = new DrawControl();
                    var userDrawView = new FormsView(_context, _windowManager, userdrawControl) { IsModal = false };
                    userDrawView.SetBackgroundToTransparent();
                    userDrawView.DisableTranslucentNavigation();
                    userDrawView.SetIsTouchable(true);
                    userdrawControl.CloseAfterDraw = true;
                    _views.TryAdd(windowView, userDrawView);
                    break;
                case WindowView.DrawView:
                    var drawControl = new DrawControl();
                    var drawView = new FormsView(_context, _windowManager, drawControl);
                    drawControl.InputTransparent = true;
                    drawView.SetIsTouchable(false);
                    drawView.Click += DrawView_Click;
                    drawView.SetBackgroundToTransparent();
                    drawView.DisableTranslucentNavigation();
                    _views.TryAdd(windowView, drawView);
                    break;
                case WindowView.DebugDrawView:
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

        if (windowView == WindowView.ActionMenuView)
        {
            var ve = _views[windowView].VisualElement;
            var ctx = ve.BindingContext;
            ve.BindingContext = null;
            ve.BindingContext = ctx;
        }
    }
    public void Close(WindowView view)
    {
        if (!_views.ContainsKey(view)) return;
        _views[view].Close();
    }

    public void Cancel(WindowView view)
    {
        if (!_views.ContainsKey(view)) return;
        _views[view]?.CloseCancel();
    }

    public async Task<string> PromptInput(string message)
    {
        Show(WindowView.PromptStringInputView);
        var viewModel = (PromptStringInputViewModel)_views[WindowView.PromptStringInputView].VisualElement.BindingContext;
        viewModel.Message = message;
        var formsView = (FormsView)_views[WindowView.PromptStringInputView];
        if (await formsView.WaitForClose())
        {
            return viewModel.Input;
        }
        return null;
    }

    public async Task<string> SelectOption(string message, params string[] options)
    {
        Show(WindowView.PromptSelectOptionView);
        var viewModel = (PromptSelectOptionViewModel)_views[WindowView.PromptSelectOptionView].VisualElement.BindingContext;
        viewModel.Message = message;
        viewModel.Options = options;
        var formsView = (FormsView)_views[WindowView.PromptSelectOptionView];
        if (await formsView.WaitForClose())
        {
            return viewModel.SelectedOption;
        }
        return null;
    }

    public async Task<Bounds> DrawUserRectangle()
    {
        Show(WindowView.UserDrawView);
        var drawControl = (DrawControl)_views[WindowView.UserDrawView].VisualElement;
        drawControl.ClearRectangles();
        var formsView = (FormsView)_views[WindowView.UserDrawView];
        if (await formsView.WaitForClose())
        {
            return new Bounds() { X = drawControl.RectX, Y = drawControl.RectY, W = drawControl.RectWidth, H = drawControl.RectHeight };
        }
        return null;
    }

    public void DrawClear()
    {
        if (!_views.ContainsKey(WindowView.DrawView)) return;

        var drawControl = (DrawControl)_views[WindowView.DrawView].VisualElement;
        drawControl.ClearCircles();
        drawControl.ClearRectangles();
    }

    public void DrawRectangle(int x, int y, int width, int height)
    {
        Show(WindowView.DrawView);
        var drawControl = (DrawControl)_views[WindowView.DrawView].VisualElement;
        var drawView = (FormsView)_views[WindowView.DrawView];
        drawView.SetIsTouchable(true);
        drawControl.AddRectangle(x, y, width, height);
    }

    public void DrawCircle(int x, int y)
    {
        Show(WindowView.DrawView);
        var drawControl = (DrawControl)_views[WindowView.DrawView].VisualElement;
        var drawView = (FormsView)_views[WindowView.DrawView];
        drawView.SetIsTouchable(true);
        drawControl.AddCircle(x, y);
    }

    public void DebugRectangle(int x, int y, int width, int height)
    {
        Show(WindowView.DebugDrawView);
        var drawControl = (DrawControl)_views[WindowView.DebugDrawView].VisualElement;
        drawControl.AddRectangle(x - 2, y - 2, width + 4, height + 4);
    }

    public void DebugCircle(int x, int y)
    {
        Show(WindowView.DebugDrawView);
        var drawControl = (DrawControl)_views[WindowView.DebugDrawView].VisualElement;
        drawControl.AddCircle(x, y);
    }

    public void DebugClear()
    {
        if (!_views.ContainsKey(WindowView.DebugDrawView)) return;

        var drawControl = (DrawControl)_views[WindowView.DebugDrawView].VisualElement;
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
        Close(WindowView.DrawView);
    }

    public async Task<List<Point>> GetMatches(PatternBase template, FindOptions opts)
    {
        try
        {
            var boundsPadding = 4;
            var templateBitmap = BitmapFactory.DecodeStream(new MemoryStream(template.ImageData));
            var templateResolutionWidth = template.Resolution.Width;
            var templateResolutionHeight = template.Resolution.Height;
            var targetResolutionWidth = _displayWidth;
            var targetResolutionHeight = _displayHeight;
            //https://stackoverflow.com/questions/33750272/how-to-resize-images-in-xamarin-forms
            var xRatio = targetResolutionWidth / templateResolutionWidth;
            var yRatio = targetResolutionHeight / templateResolutionHeight;
            var ratio = Math.Min(xRatio, yRatio);
            var targetWidth = (float)(templateBitmap.Width * ratio);
            var targetHeight = (float)(templateBitmap.Height * ratio);
            var needleBitmap = Bitmap.CreateScaledBitmap(templateBitmap, (int)targetWidth, (int)targetHeight, true);
            var calcBounds = TransformBounds(template.Bounds, template.Resolution);

            //template.Bounds = null;
            Bitmap haystackBitmap = null;
            try
            {
                var strokeThickness = 3;
                haystackBitmap = template.Bounds != null ?
                await _mediaProjectionService.GetCurrentImageBitmap<Bitmap>(
                    (int)(calcBounds.X - boundsPadding),
                    (int)(calcBounds.Y - boundsPadding),
                    (int)(calcBounds.W + boundsPadding),
                    (int)(calcBounds.H + boundsPadding)) :
                await _mediaProjectionService.GetCurrentImageBitmap<Bitmap>();

                //imageBitmap = template.Bounds != null ?
                //await _mediaProjectionService.GetCurrentImageBitmap<Bitmap>(
                //    (int)template.Bounds.X + strokeThickness - 1,
                //    (int)template.Bounds.Y + strokeThickness - 1,
                //    (int)template.Bounds.W - strokeThickness + 1,
                //    (int)template.Bounds.H - strokeThickness - 1) :
                //await _mediaProjectionService.GetCurrentImageBitmap<Bitmap>();

                //var imageStream = template.Bounds != null ?
                //await _mediaProjectionService.GetCurrentImageStream(
                //    (int)template.Bounds.X + strokeThickness - 1,
                //    (int)template.Bounds.Y + strokeThickness - 1,
                //    (int)template.Bounds.W - strokeThickness + 1,
                //    (int)template.Bounds.H - strokeThickness - 1) :

                //haystackBitmap = BitmapFactory.DecodeStream(imageStream);

                //await _mediaProjectionService.GetCurrentImageBitmap(
                //    (int)(calcBounds.X - boundsPadding),
                //    (int)(calcBounds.Y - boundsPadding),
                //    (int)(calcBounds.W + boundsPadding),
                //    (int)(calcBounds.H + boundsPadding)) :
                //await _mediaProjectionService.GetCurrentImageBitmap();
            }
            catch (Exception ex)
            {
                return new List<Point>();
            }

            //imageBitmap.Dispose();
            //resized.Dispose();
            //return new List<Point>() { new Point(100, 50) };


            if (haystackBitmap == null) return new List<Point>();

            var threshold = 0.8;
            if (template.Threshold != 0.0) threshold = template.Threshold;
            if ((opts?.Threshold ?? 0.0) != 0.0) threshold = opts.Threshold;

            var points = OpenCvHelper.GetPointsWithMatchTemplate(haystackBitmap, needleBitmap, opts?.Limit ?? 1, threshold);
            //raw bitmap comparison doesn't seem to work when bounds are changed (searching subbitmap)
            //var points = BitmapHelper.SearchBitmap(templateBitmap, haystackBitmap, 0.0);
            needleBitmap.Dispose();
            templateBitmap.Dispose();
            haystackBitmap.Dispose();

            //Console.WriteLine("[*****YeetMacro*****] MediaProjectionService GetMatches GetPointsWithMatchTemplate Start");
            //var points = XamarinApp.Droid.OpenCv.Utils.GetPointsWithMatchTemplate(imageBitmap, resized, limit);
            //Console.WriteLine("[*****YeetMacro*****] MediaProjectionService GetMatches GetPointsWithMatchTemplate End");

            if (template.Bounds != null)
            {
                var newPoints = new List<Point>();
                for (int i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    newPoints.Add(new Point(
                        (int)(point.X + (calcBounds.X - boundsPadding)),
                        (int)(point.Y + (calcBounds.Y - boundsPadding))));
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

    public Bounds TransformBounds(Bounds originalBounds, Resolution originalResolution)
    {
        try
        {
            var templateResolutionWidth = originalResolution.Width;
            var templateResolutionHeight = originalResolution.Height;
            var targetResolutionWidth = _displayWidth;
            var targetResolutionHeight = _displayHeight;
            var xRatio = targetResolutionWidth / templateResolutionWidth;
            var yRatio = targetResolutionHeight / templateResolutionHeight;
            var topLeft = GetTopLeftByPackage();
            var ratio = Math.Min(xRatio, yRatio);
            var x = (int)(originalBounds.X * ratio);
            var y = (int)(originalBounds.Y * ratio);
            var width = (int)(originalBounds.W * xRatio);
            var height = (int)(originalBounds.H * yRatio);
            if (xRatio > yRatio)    //assuming content gets centered
            {
                var leftoverWidth = targetResolutionWidth - templateResolutionWidth - topLeft.x;
                var offsetX = leftoverWidth / 2.0 + topLeft.x;
                x += (int)offsetX;
                //width += (int)offsetX;
            }
            //else if (yRatio > xRatio)    // ??
            //{
            //    //y = (int)(y * yRatio - (y * yRatio) / 2.0 + topLeft.y);
            //    //height += (int)(y * yRatio);

            //    //x = (int)(x * yRatio - (x * yRatio) / 2.0);
            //    //width += (int)(x * yRatio);

            //    var leftoverHeight = targetResolutionHeight - templateResolutionHeight - topLeft.y;
            //    y = (int)(y * yRatio - leftoverHeight / 2.0 + topLeft.y);
            //    height += (int)(leftoverHeight);
            //    x = (int)(x / 2 * 2.5);     //divide by original density then multiple by current density
            //    //x = (int)(x - width * yRatio / 2.0 );
            //    //width = (int)(width * yRatio);

            //    //var offsetY = leftoverHeigh / 2.0 + topLeft.y;
            //    //y += (int)leftoverHeigh;
            //    //height += (int)offsetY;
            //}

            return new Bounds() { X = x, Y = y, W = width, H = height };
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] WindowManagerService TransformBounds Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
            return originalBounds;
        }
    }

    public Task<MemoryStream> GetCurrentImageStream()
    {
        return _mediaProjectionService.GetCurrentImageStream();
    }

    public Task<MemoryStream> GetCurrentImageStream(int x, int y, int width, int height)
    {
        return _mediaProjectionService.GetCurrentImageStream(x, y, width, height);
    }

    public void DoClick(float x, float y)
    {
        _accessibilityService.DoClick(x, y);
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
}