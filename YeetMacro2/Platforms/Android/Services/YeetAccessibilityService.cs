using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Views.Accessibility;
using Microsoft.Extensions.Logging;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Services;
//http://www.spikie.be/post/2017/07/01/AndroidFloatingWidgetsInXamarin.html
[Service(Label = "YeetMacro Service", Exported = true, Permission = global::Android.Manifest.Permission.BindAccessibilityService)]
[IntentFilter(new string[] { "android.accessibilityservice.AccessibilityService" })]
[MetaData("android.accessibilityservice", Resource = "@xml/yeetmacro_config")]
public class YeetAccessibilityService : AccessibilityService
{
    ILogger _logger;
    MainActivity _context;
    Lazy<ActionViewModel> _actionViewModel;
    private string _currentPackage = "unknown";
    public YeetAccessibilityService()
    {
        _logger = ServiceHelper.GetService<ILogger<MediaProjectionService>>();
    }

    public override void OnCreate()
    {
        _logger.LogTrace("YeetAccessibilityService OnCreate");
        _actionViewModel = ServiceHelper.GetService<Lazy<ActionViewModel>>();
        Stop();
        _context = (MainActivity)Platform.CurrentActivity;
        AndroidServiceHelper.AttachAccessibilityService(this);
        BroadcastEnabled(true);
        base.OnCreate();
    }

    public bool HasAccessibilityPermissions
    {
        get { return AndroidServiceHelper.AccessibilityService is not null; }
    }

    public string CurrentPackage => _currentPackage;

    //https://stackoverflow.com/questions/23504217/how-do-i-get-active-window-that-is-on-foreground
    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {
        try
        {
            if (_actionViewModel.Value.State == ActionState.Running || e.EventType != EventTypes.WindowStateChanged) return;
            _logger.LogTrace($"YeetAccessibilityService WindowStateChanged: {e.PackageName}");
            if (e.PackageName != AppInfo.PackageName && e.PackageName != "com.google.android.gms" &&
                !e.PackageName.StartsWith("com.android"))
            {
                _currentPackage = e.PackageName;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YeetAccessibilityService OnAccessibilityEvent Exception");
        }
    }

    public override void OnInterrupt()
    {
        _logger.LogTrace("YeetAccessibilityService OnInterrupt");
    }

    public override void OnRebind(Intent intent)
    {
        _logger.LogTrace("YeetAccessibilityService OnRebind");
        base.OnRebind(intent);
    }

    public override bool OnUnbind(Intent intent)
    {
        _logger.LogTrace("YeetAccessibilityService OnUnbind");
        Stop();
        BroadcastEnabled(false);
        return base.OnUnbind(intent);
    }

    public void DoClick(Point point)
    {
        if (point.X < 0.0 || point.Y < 0.0) return;

        _logger.LogTrace("YeetAccessibilityService DoClick");
        GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
        try
        {
            global::Android.Graphics.Path swipePath = new global::Android.Graphics.Path();
            swipePath.MoveTo((float)point.X, (float)point.Y);
            swipePath.Close();
            var strokeDescription = new GestureDescription.StrokeDescription(swipePath, 0, 100);
            gestureBuilder.AddStroke(strokeDescription);
            var gesture = gestureBuilder.Build();
            DispatchGesture(gesture, null, null);
            //gestureBuilder.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YeetAccessibilityService DoClick Exception");
        }
        finally
        {
            //gestureBuilder.Dispose();
        }
    }

    // https://github.com/Fate-Grand-Automata/FGA/blob/de9c69e10aec990a061c049f0bf3ca3c253d199b/app/src/main/java/com/mathewsachin/fategrandautomata/accessibility/AccessibilityGestures.kt#L61
    public void DoSwipe(Point start, Point end)
    {
        var xDiff = (end.X - start.X);
        var yDiff = (end.Y - start.Y);
        var direction = Math.Atan2(xDiff, yDiff);
        var distanceLeft = Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2));

        var swipeDelay = 1L;
        var swipeDuration = 1L;
        var defaultSwipeDuration = 300;     // milliseconds

        var timesToSwipe = defaultSwipeDuration / (swipeDelay + swipeDuration);
        var thresholdDistance = distanceLeft / timesToSwipe;

        var from = start;
        var mouseDownPath = new global::Android.Graphics.Path();
        mouseDownPath.MoveTo((float)start.X, (float)start.Y);
        var lastStroke = new GestureDescription.StrokeDescription(mouseDownPath, 0, 200, true);
        //mouseDownPath.Dispose();
        PerformGesture(lastStroke);

        while (distanceLeft > 0)
        {
            var distanceToScroll = Math.Min(thresholdDistance, distanceLeft);

            var x = from.X + distanceToScroll * Math.Sin(direction);
            var y = from.Y + distanceToScroll * Math.Cos(direction);
            var to = new Point(x, y);

            var swipePath = new global::Android.Graphics.Path();
            swipePath.MoveTo((float)from.X, (float)from.Y);
            swipePath.LineTo((float)to.X, (float)to.Y);

            lastStroke = lastStroke.ContinueStroke(swipePath, swipeDelay, swipeDuration, true);
            PerformGesture(lastStroke);
            //swipePath.Dispose();
            from = to;
            distanceLeft -= distanceToScroll;
        }

        var mouseUpPath = new global::Android.Graphics.Path();
        mouseUpPath.MoveTo((float)from.X, (float)from.Y);

        lastStroke = lastStroke.ContinueStroke(mouseUpPath, 1, 400L, false);
        PerformGesture(lastStroke);
        //mouseUpPath.Dispose();
        //lastStroke.Dispose();
    }

    private void PerformGesture(GestureDescription.StrokeDescription strokeDescription)
    {
        var gestureBuilder = new GestureDescription.Builder();
        var gestureDescription = gestureBuilder.AddStroke(strokeDescription).Build();
        DispatchGesture(gestureDescription, null, null);
        //gestureDescription.Dispose();
        //gestureBuilder.Dispose();
    }

    public void Start()
    {
        _logger.LogTrace("YeetAccessibilityService Start");
        AndroidServiceHelper.StartAccessibilityService();
    }

    public void Stop()
    {
        try
        {
            _logger.LogTrace("YeetAccessibilityService Stop");
            DisableSelf();
            AndroidServiceHelper.DetachAccessibilityService();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YeetAccessibilityService Stop Exception");
        }
    }

    private void BroadcastEnabled(bool enabled)
    {
        try
        {
            _logger.LogTrace("YeetAccessibilityService BroadcastEnabled");
            Intent enabledChanged = new Intent("com.companyname.AccessibilityService.CHANGED");
            enabledChanged.PutExtra("enabled", enabled);
            _context?.SendBroadcast(enabledChanged);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YeetAccessibilityService BroadcastEnabled Exception");
        }
    }
}
