using YeetMacro2.Services;
using Android.Views;
using Android.Content;
using Android.Runtime;
using Android.Provider;
using System.Collections.Concurrent;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Controls;

namespace YeetMacro2.Platforms.Android.Services;
public class WindowManagerService : IWindowManagerService
{
    public const int OVERLAY_SERVICE_REQUEST = 0;
    private MainActivity _context;
    IWindowManager _windowManager;
    //IAccessibilityService _accessibilityService;
    //IMediaProjectionService _mediaProjectionService;
    ConcurrentDictionary<WindowView, IShowable> _views = new ConcurrentDictionary<WindowView, IShowable>();
    //ConcurrentDictionary<string, (int x, int y)> _packageToStatusBarHeight = new ConcurrentDictionary<string, (int x, int y)>();
    //FormsView _windowView;

    //public WindowManagerService(IAccessibilityService accessibilityService, IMediaProjectionService mediaProjectionService)
    public WindowManagerService()
    {
        _context = (MainActivity)Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        _windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        //_accessibilityService = accessibilityService;
        //_mediaProjectionService = mediaProjectionService;
    }

    //public void ShowOverlayWindow()
    //{
    //    if (_windowView == null)
    //    {
    //        var grid = new Xamarin.Forms.Grid() { InputTransparent = true, CascadeInputTransparent = true };
    //        _windowView = new FormsView(_context, _windowManager, grid) { IsModal = false };
    //        _windowView.SetIsTouchable(false);
    //        _windowView.SetBackgroundToTransparent();
    //        _windowView.DisableTranslucentNavigation();
    //    }
    //    _windowView.Show();
    //}

    //public void CloseOverlayWindow()
    //{
    //    if (_windowView != null)
    //    {
    //        _windowView.Close();
    //    }
    //}

    public void Show(WindowView view)
    {
        //Get overlay permissin if needed
        if (!Settings.CanDrawOverlays(_context))
        {
            _context.StartActivityForResult(new Intent(Settings.ActionManageOverlayPermission, global::Android.Net.Uri.Parse("package:" + _context.PackageName)), OVERLAY_SERVICE_REQUEST);
            return;
        }

        if (!_views.ContainsKey(view))
        {
            switch (view)
            {
                //case WindowView.PatternsTreeView:
                //    var patternsTree = new PatternTreeView() { Parent = Xamarin.Forms.Application.Current };
                //    var patternsTreeView = new ResizeView(_context, _windowManager, this, patternsTree);
                //    _views.TryAdd(view, patternsTreeView);
                //    break;
                //case WindowView.PatternsView:
                //    var patternsControl = new PatternControl() { Parent = Xamarin.Forms.Application.Current };
                //    var patternsView = new ResizeView(_context, _windowManager, this, patternsControl);
                //    patternsView.Focusable = false;
                //    _views.TryAdd(view, patternsView);
                //    break;
                //case WindowView.DrawView:
                //    var drawControl = new DrawControl() { Parent = Xamarin.Forms.Application.Current };
                //    var drawView = new FormsView(_context, _windowManager, drawControl);
                //    drawControl.InputTransparent = true;
                //    drawView.SetIsTouchable(false);
                //    drawView.Click += DrawView_Click;
                //    drawView.SetBackgroundToTransparent();
                //    drawView.DisableTranslucentNavigation();
                //    _views.TryAdd(view, drawView);
                //    break;
                //case WindowView.UserDrawView:
                //    var userdrawControl = new DrawControl() { Parent = Xamarin.Forms.Application.Current };
                //    var userDrawView = new FormsView(_context, _windowManager, userdrawControl);
                //    userDrawView.SetBackgroundToTransparent();
                //    userDrawView.DisableTranslucentNavigation();
                //    userdrawControl.CloseAfterDraw = true;
                //    _views.TryAdd(view, userDrawView);
                //    break;
                case WindowView.ActionView:
                    var actionControl = new ActionControl();
                    var actionView = new MoveView(_context, _windowManager, actionControl);
                    _views.TryAdd(view, actionView);
                    break;
                //case WindowView.ActionMenuView:
                //    var actionMenu = new ActionMenu() { Parent = Xamarin.Forms.Application.Current };
                //    var actionMenuView = new FormsView(_context, _windowManager, actionMenu);
                //    _views.TryAdd(view, actionMenuView);
                //    break;
                //case WindowView.PromptStringInputView:
                //    var promptStringInput = new PromptStringInput() { Parent = Xamarin.Forms.Application.Current };
                //    var promptStringInputView = new FormsView(_context, _windowManager, promptStringInput);
                //    _views.TryAdd(view, promptStringInputView);
                //    break;
                case WindowView.LogView:
                    var logControl = new LogControl();
                    var logView = new ResizeView(_context, _windowManager, this, logControl);
                    _views.TryAdd(view, logView);
                    break;
            }
        }

        _views[view].Show();
    }

    //private void DrawView_Click(object sender, System.EventArgs e)
    //{
    //    Close(WindowView.DrawView);
    //}

    //public void Close(WindowView view)
    //{
    //    _views[view].Close();
    //}

    //public void Cancel(WindowView view)
    //{
    //    _views[view].CloseCancel();
    //}

    //public async Task<string> PromptInput(string message)
    //{
    //    Show(WindowView.PromptStringInputView);
    //    var viewModel = (PromptStringInputViewModel)_views[WindowView.PromptStringInputView].VisualElement.BindingContext;
    //    viewModel.Message = message;
    //    var formsView = (FormsView)_views[WindowView.PromptStringInputView];
    //    if (await formsView.WaitForClose())
    //    {
    //        return viewModel.Input;
    //    }
    //    return null;
    //}

    //public void DrawClear()
    //{
    //    if (_views.ContainsKey(WindowView.DrawView))
    //    {
    //        var drawControl = (DrawControl)_views[WindowView.DrawView].VisualElement;
    //        drawControl.ClearCircles();
    //        drawControl.ClearRectangles();
    //    }
    //}

    //public void DrawRectangle(int x, int y, int width, int height)
    //{
    //    Show(WindowView.DrawView);
    //    var drawControl = (DrawControl)_views[WindowView.DrawView].VisualElement;
    //    var drawView = (FormsView)_views[WindowView.DrawView];
    //    drawView.SetIsTouchable(true);
    //    drawControl.AddRectangle(x, y, width, height);
    //}

    //public void DrawCircle(int x, int y)
    //{
    //    Show(WindowView.DrawView);
    //    var drawControl = (DrawControl)_views[WindowView.DrawView].VisualElement;
    //    var drawView = (FormsView)_views[WindowView.DrawView];
    //    drawView.SetIsTouchable(true);
    //    drawControl.AddCircle(x, y);
    //}

    //public async Task<Bounds> DrawUserRectangle()
    //{
    //    Show(WindowView.UserDrawView);
    //    var drawControl = (DrawControl)_views[WindowView.UserDrawView].VisualElement;
    //    var formsView = (FormsView)_views[WindowView.UserDrawView];
    //    if (await formsView.WaitForClose())
    //    {
    //        return new Bounds() { X = drawControl.RectX, Y = drawControl.RectY, Width = drawControl.RectWidth, Height = drawControl.RectHeight };
    //    }
    //    return null;
    //}


    ////https://stackoverflow.com/questions/3407256/height-of-status-bar-in-android
    //public (int x, int y) GetTopLeft()
    //{
    //    Console.WriteLine("[*****YeetMacro*****] WindowManagerService GetTopLeft Start");
    //    Console.WriteLine("[*****YeetMacro*****] WindowManagerService _accessibilityService: " + _accessibilityService);
    //    var currentPackage = _accessibilityService.CurrentPackage;
    //    Console.WriteLine("[*****YeetMacro*****] WindowManagerService _accessibilityService.CurrentPackage: " + _accessibilityService.CurrentPackage);

    //    if (_packageToStatusBarHeight.ContainsKey(currentPackage))
    //    {
    //        return _packageToStatusBarHeight[currentPackage];
    //    }

    //    var loc = new int[2];
    //    Console.WriteLine("[*****YeetMacro*****] WindowManagerService GetLocationOnScreen Start");
    //    _windowView.GetLocationOnScreen(loc);
    //    Console.WriteLine("[*****YeetMacro*****] WindowManagerService GetLocationOnScreen End");
    //    var topLeft = (x: loc[0], y: loc[1]);
    //    Console.WriteLine($"[*****YeetMacro*****] x{topLeft.x} y{topLeft.y}");

    //    if (currentPackage != "unknown")
    //    {
    //        _packageToStatusBarHeight.TryAdd(currentPackage, topLeft);
    //    }


    //    Console.WriteLine("[*****YeetMacro*****] WindowManagerService GetTopLeft End");
    //    return topLeft;
    //}

    //public void LaunchYeetMacro()
    //{
    //    ServiceHelper.LaunchApp(_context.PackageName);
    //}

    //public bool ProjectionServiceEnabled { get => _mediaProjectionService.Enabled; }

    //public void RequestAccessibilityPermissions()
    //{
    //    _accessibilityService.Start();
    //}

    //public void RevokeAccessibilityPermissions()
    //{
    //    _accessibilityService.Stop();
    //}

    //public void StartProjectionService()
    //{
    //    _mediaProjectionService.Start();
    //    _context.StartForegroundServiceCompat<ForegroundService>();
    //}

    //public void StopProjectionService()
    //{
    //    _context.StartForegroundServiceCompat<ForegroundService>(ForegroundService.EXIT_ACTION);
    //}

    //public async Task<List<Point>> GetMatches(PatternBase template, int limit = 1)
    //{
    //    try
    //    {
    //        Console.WriteLine("[*****YeetMacro*****] WindowManagerService GetMatches Start");
    //        var boundsPadding = 4;
    //        var templateBitmap = BitmapFactory.DecodeStream(new MemoryStream(template.ImageData));
    //        var templateResolutionWidth = template.Resolution.Width;
    //        var templateResolutionHeight = template.Resolution.Height;
    //        var targetResolutionWidth = DeviceDisplay.MainDisplayInfo.Width;
    //        var targetResolutionHeight = DeviceDisplay.MainDisplayInfo.Height;
    //        //https://stackoverflow.com/questions/33750272/how-to-resize-images-in-xamarin-forms
    //        var xRatio = targetResolutionWidth / templateResolutionWidth;
    //        var yRatio = targetResolutionHeight / templateResolutionHeight;
    //        var ratio = Math.Min(xRatio, yRatio);
    //        var targetWidth = (float)(templateBitmap.Width * ratio);
    //        var targetHeight = (float)(templateBitmap.Height * ratio);
    //        var resized = Bitmap.CreateScaledBitmap(templateBitmap, (int)targetWidth, (int)targetHeight, true);
    //        Console.WriteLine("[*****YeetMacro*****] WindowManagerService GetMatches TransformBounds Start");
    //        var calcBounds = TransformBounds(template.Bounds, template.Resolution);
    //        Console.WriteLine("[*****YeetMacro*****] WindowManagerService GetMatches TransformBounds End");

    //        Bitmap imageBitmap = null;
    //        try
    //        {
    //            var strokeThickness = 3;
    //            imageBitmap = template.Bounds != null ?
    //            await _mediaProjectionService.GetCurrentImageBitmap(
    //                (int)template.Bounds.X + strokeThickness - 1,
    //                (int)template.Bounds.Y + strokeThickness - 1,
    //                (int)template.Bounds.Width - strokeThickness + 1,
    //                (int)template.Bounds.Height - strokeThickness - 1) :
    //            await _mediaProjectionService.GetCurrentImageBitmap();

    //            //var imageStream = template.Bounds != null ?
    //            //await _mediaProjectionService.GetCurrentImageStream(
    //            //    (int)template.Bounds.X + strokeThickness - 1,
    //            //    (int)template.Bounds.Y + strokeThickness - 1,
    //            //    (int)template.Bounds.Width - strokeThickness + 1,
    //            //    (int)template.Bounds.Height - strokeThickness - 1) :
    //            //await _mediaProjectionService.GetCurrentImageStream();

    //            //imageBitmap = BitmapFactory.DecodeStream(imageStream);

    //            //await _mediaProjectionService.GetCurrentImageBitmap(
    //            //    (int)(calcBounds.X - boundsPadding),
    //            //    (int)(calcBounds.Y - boundsPadding),
    //            //    (int)(calcBounds.Width + boundsPadding),
    //            //    (int)(calcBounds.Height + boundsPadding)) :
    //            //await _mediaProjectionService.GetCurrentImageBitmap();
    //        }
    //        catch (Exception ex)
    //        {
    //            return new List<Point>();
    //        }

    //        //{
    //        //    var name = "haystack.png";
    //        //    var picturesPath = "/storage/emulated/0/Pictures";
    //        //    var filePath = System.IO.Path.Combine(picturesPath, name);

    //        //    MemoryStream ms = new MemoryStream();
    //        //    imageBitmap.Compress(CompressFormat.Png, 100, ms);
    //        //    ms.Position = 0;

    //        //    byte[] bArray = new byte[ms.Length];
    //        //    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
    //        //    {
    //        //        using (ms)
    //        //        {
    //        //            ms.Read(bArray, 0, (int)ms.Length);
    //        //        }
    //        //        int length = bArray.Length;
    //        //        fs.Write(bArray, 0, length);
    //        //    }
    //        //}

    //        //{
    //        //    var name = "resized_original.png";
    //        //    var picturesPath = "/storage/emulated/0/Pictures";
    //        //    var filePath = System.IO.Path.Combine(picturesPath, name);

    //        //    MemoryStream ms = new MemoryStream();
    //        //    templateBitmap.Compress(CompressFormat.Png, 100, ms);
    //        //    ms.Position = 0;

    //        //    byte[] bArray = new byte[ms.Length];
    //        //    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
    //        //    {
    //        //        using (ms)
    //        //        {
    //        //            ms.Read(bArray, 0, (int)ms.Length);
    //        //        }
    //        //        int length = bArray.Length;
    //        //        fs.Write(bArray, 0, length);
    //        //    }
    //        //}

    //        //{
    //        //    var name = "resized_needle.png";
    //        //    var picturesPath = "/storage/emulated/0/Pictures";
    //        //    var filePath = System.IO.Path.Combine(picturesPath, name);

    //        //    MemoryStream ms = new MemoryStream();
    //        //    resized.Compress(CompressFormat.Png, 100, ms);
    //        //    ms.Position = 0;

    //        //    byte[] bArray = new byte[ms.Length];
    //        //    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
    //        //    {
    //        //        using (ms)
    //        //        {
    //        //            ms.Read(bArray, 0, (int)ms.Length);
    //        //        }
    //        //        int length = bArray.Length;
    //        //        fs.Write(bArray, 0, length);
    //        //    }
    //        //}

    //        return new List<Point>();

    //        var points = XamarinApp.Droid.Helpers.BitmapHelper.SearchBitmap(resized, imageBitmap, 0.0);

    //        if (imageBitmap == null) return new List<Point>();

    //        //Console.WriteLine("[*****YeetMacro*****] MediaProjectionService GetMatches GetPointsWithMatchTemplate Start");
    //        //var points = XamarinApp.Droid.OpenCv.Utils.GetPointsWithMatchTemplate(imageBitmap, resized, limit);
    //        //Console.WriteLine("[*****YeetMacro*****] MediaProjectionService GetMatches GetPointsWithMatchTemplate End");

    //        if (template.Bounds != null)
    //        {
    //            var newPoints = new List<Point>();
    //            for (int i = 0; i < points.Count; i++)
    //            {
    //                var point = points[i];
    //                newPoints.Add(new Point(
    //                    (int)(point.X + (calcBounds.X - boundsPadding)),
    //                    (int)(point.Y + (calcBounds.Y - boundsPadding))));
    //            }
    //            return newPoints;
    //        }
    //        return points;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("[*****YeetMacro*****] MediaProjectionService GetMatches Exception");
    //        Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
    //        return new List<Point>();
    //    }
    //}

    //public Bounds TransformBounds(Bounds originalBounds, Resolution originalResolution)
    //{
    //    try
    //    {
    //        Console.WriteLine("[*****YeetMacro*****] WindowManagerService TransformBounds Start");
    //        var templateResolutionWidth = originalResolution.Width;
    //        var templateResolutionHeight = originalResolution.Height;
    //        var targetResolutionWidth = DeviceDisplay.MainDisplayInfo.Width;
    //        var targetResolutionHeight = DeviceDisplay.MainDisplayInfo.Height;
    //        var xRatio = targetResolutionWidth / templateResolutionWidth;
    //        var yRatio = targetResolutionHeight / templateResolutionHeight;
    //        Console.WriteLine("[*****YeetMacro*****] WindowManagerService TransformBounds GetTopLeft Start");
    //        var topLeft = GetTopLeft();
    //        Console.WriteLine("[*****YeetMacro*****] WindowManagerService TransformBounds GetTopLeft End");
    //        var ratio = Math.Min(xRatio, yRatio);
    //        var x = (int)(originalBounds.X * ratio);
    //        var y = (int)(originalBounds.Y * ratio);
    //        var width = (int)(originalBounds.Width * xRatio);
    //        var height = (int)(originalBounds.Height * yRatio);
    //        if (xRatio > yRatio)    //assuming content gets centered
    //        {
    //            var leftoverWidth = targetResolutionWidth - templateResolutionWidth - topLeft.x;
    //            var offsetX = leftoverWidth / 2.0 + topLeft.x;
    //            x += (int)offsetX;
    //            //width += (int)offsetX;
    //        }
    //        else if (yRatio > xRatio)    //assuming content gets centered
    //        {
    //            var leftoverHeigh = targetResolutionHeight - templateResolutionHeight - topLeft.y;
    //            var offsetY = leftoverHeigh / 2.0 + topLeft.y;
    //            y += (int)offsetY;
    //            //height += (int)offsetY;
    //        }


    //        Console.WriteLine("[*****YeetMacro*****] WindowManagerService TransformBounds End");
    //        return new Bounds() { X = x, Y = y, Width = width, Height = height };
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("[*****YeetMacro*****] WindowManagerService TransformBounds Exception");
    //        Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
    //        return originalBounds;
    //    }
    //}
}