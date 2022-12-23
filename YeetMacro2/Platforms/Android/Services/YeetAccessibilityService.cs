using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Views.Accessibility;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Services;
//http://www.spikie.be/post/2017/07/01/AndroidFloatingWidgetsInXamarin.html
[Service(Label = "YeetMacro Service", Exported = true, Permission = global::Android.Manifest.Permission.BindAccessibilityService)]
[IntentFilter(new string[] { "android.accessibilityservice.AccessibilityService" })]
[MetaData("android.accessibilityservice", Resource = "@xml/yeetmacro_config")]
public class YeetAccessibilityService : AccessibilityService, IAccessibilityService
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
                if (e.PackageName != "com.companyname.xamarinapp")
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

    public void DoClick(float x, float y)
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService DoClick");
            if (_instance == null || x < 0.0f || y < 0.0f)
            {
                return;
            }

            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService DoClick Start");
            global::Android.Graphics.Path swipePath = new global::Android.Graphics.Path();
            swipePath.MoveTo(x, y);
            GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
            gestureBuilder.AddStroke(new GestureDescription.StrokeDescription(swipePath, 0, 100));
            _instance.DispatchGesture(gestureBuilder.Build(), null, null);
            swipePath.Close();
            swipePath.Dispose();

            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService DoClick DispatchGesture");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] YeetAccessibilityService OnAccessibilityEvent Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }
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
            _instance?.Dispose();
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
