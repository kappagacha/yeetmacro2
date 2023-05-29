using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Views.Accessibility;

namespace YeetMacro2.Platforms.Android.Services;
//http://www.spikie.be/post/2017/07/01/AndroidFloatingWidgetsInXamarin.html
[Service(Label = "YeetMacro Service", Exported = true, Permission = global::Android.Manifest.Permission.BindAccessibilityService)]
[IntentFilter(new string[] { "android.accessibilityservice.AccessibilityService" })]
[MetaData("android.accessibilityservice", Resource = "@xml/yeetmacro_config")]
public class YeetAccessibilityService : AccessibilityService
{
    MainActivity _context;
    private static string _currentPackage = "unknown";
    private static YeetAccessibilityService _instance;  //https://stackoverflow.com/questions/600207/how-to-check-if-a-service-is-running-on-android
    public YeetAccessibilityService()
    {
        Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService Constructor Start");
        Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService Constructor End");
    }

    private void Init()
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService Init");
            _context = (MainActivity)Platform.CurrentActivity;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService Init Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }
    }

    public override void OnCreate()
    {
        Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService OnCreate");
        Stop();
        Init();
        _instance = this;
        BroadcastEnabled();
        base.OnCreate();
    }

    public bool HasAccessibilityPermissions
    {
        get { return _instance != null; }
    }

    public string CurrentPackage => _currentPackage;
    //public string CurrentPackage => "temp";

    //https://stackoverflow.com/questions/23504217/how-do-i-get-active-window-that-is-on-foreground
    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {
        try
        {
            if (e.EventType == EventTypes.WindowStateChanged)
            {
                Console.WriteLine("WindowStateChanged: " + e.PackageName);

                if (e.PackageName != AppInfo.PackageName && e.PackageName != "com.google.android.gms" &&
                    !e.PackageName.StartsWith("com.android"))
                {
                    _currentPackage = e.PackageName;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService OnAccessibilityEvent Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }

    }

    public override void OnInterrupt()
    {
        Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService OnInterrupt");
    }

    public override void OnRebind(Intent intent)
    {
        Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService OnRebind");
        base.OnRebind(intent);
    }

    public override bool OnUnbind(Intent intent)
    {
        Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService OnUnbind");
        Stop();
        BroadcastEnabled();
        return base.OnUnbind(intent);
    }

    public void DoClick(Point point)
    {
        try
        {
            if (_instance == null || point.X < 0.0 || point.Y < 0.0)
            {
                return;
            }

            global::Android.Graphics.Path swipePath = new global::Android.Graphics.Path();
            swipePath.MoveTo((float)point.X, (float)point.Y);
            GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
            gestureBuilder.AddStroke(new GestureDescription.StrokeDescription(swipePath, 0, 100));
            _instance.DispatchGesture(gestureBuilder.Build(), null, null);
            swipePath.Close();
            swipePath.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService OnAccessibilityEvent Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }
    }

    // https://github.com/Fate-Grand-Automata/FGA/blob/de9c69e10aec990a061c049f0bf3ca3c253d199b/app/src/main/java/com/mathewsachin/fategrandautomata/accessibility/AccessibilityGestures.kt#L61
    public void DoSwipe(Point start, Point end)
    {
        if (_instance == null)
        {
            return;
        }

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

            from = to;
            distanceLeft -= distanceToScroll;
        }

        var mouseUpPath = new global::Android.Graphics.Path();
        mouseUpPath.MoveTo((float)from.X, (float)from.Y);

        lastStroke = lastStroke.ContinueStroke(mouseUpPath, 1, 400L, false);
        PerformGesture(lastStroke);
    }

    private void PerformGesture(GestureDescription.StrokeDescription strokeDescription)
    {
        var gestureDescription = new GestureDescription.Builder().AddStroke(strokeDescription).Build();
        _instance.DispatchGesture(gestureDescription, null, null);
    }

    public void Start()
    {
        Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService Start");
        if (_instance == null)
        {
            Init();
            _context.StartActivity(new Intent(Settings.ActionAccessibilitySettings));
        }
    }

    public void Stop()
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService Stop");
            _instance?.DisableSelf();
            _instance = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService Stop Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }
    }

    private void BroadcastEnabled()
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService BroadcastEnabled");
            Intent enabledChanged = new Intent("com.companyname.AccessibilityService.CHANGED");
            enabledChanged.PutExtra("enabled", _instance != null ? true : false);
            _context?.SendBroadcast(enabledChanged);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService BroadcastEnabled Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }
    }
}
