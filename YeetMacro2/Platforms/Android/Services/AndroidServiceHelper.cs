using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;

namespace YeetMacro2.Platforms.Android.Services;
public static class AndroidServiceHelper
{
    static YeetAccessibilityService _accessibilityService;
    static ForegroundService _foregroundService;
    public static YeetAccessibilityService AccessibilityService { get => _accessibilityService; }
    public static ForegroundService ForegroundService { get => _foregroundService; }
    public static void StartAccessibilityService()
    {
        Platform.CurrentActivity.StartActivity(new Intent(Settings.ActionAccessibilitySettings));
    }
    public static void AttachAccessibilityService(YeetAccessibilityService accessibilityService)
    {
        _accessibilityService = accessibilityService;
    }
    public static void DetachAccessibilityService()
    {
        _accessibilityService = null;
    }
    public static void AttachForegroundService(ForegroundService foregroundService)
    {
        _foregroundService = foregroundService;
    }
    public static void DetachForegroundService()
    {
        _foregroundService = null;
    }

    //https://stackoverflow.com/questions/63594273/xamarin-forms-how-to-open-an-app-from-another-app
    public static Task<bool> LaunchApp(string packageName)
    {
        bool result = false;

        try
        {
            PackageManager pm = global::Android.App.Application.Context.PackageManager;

            if (IsAppInstalled(packageName))
            {
                Intent intent = pm.GetLaunchIntentForPackage(packageName);
                if (intent != null)
                {
                    intent.SetFlags(ActivityFlags.NewTask);
                    global::Android.App.Application.Context.StartActivity(intent);
                }
            }
            else
            {
                Intent intent = pm.GetLaunchIntentForPackage("the package name of play store on your device");
                if (intent != null)
                {
                    intent.SetFlags(ActivityFlags.NewTask);
                    global::Android.App.Application.Context.StartActivity(intent);
                }
            }
        }
        catch (ActivityNotFoundException)
        {
            result = false;
        }

        return Task.FromResult(result);
    }

    private static bool IsAppInstalled(string packageName)
    {
        PackageManager pm = global::Android.App.Application.Context.PackageManager;
        bool installed = false;
        try
        {
            pm.GetPackageInfo(packageName, PackageInfoFlags.Activities);
            installed = true;
        }
        catch (PackageManager.NameNotFoundException)
        {
            installed = false;
        }

        return installed;
    }

    //https://stackoverflow.com/questions/3600713/size-of-android-notification-bar-and-title-bar
    public static int GetStatusBarHeight(Activity context)
    {
        int result = 0;
        int resourceId = context.Resources.GetIdentifier("status_bar_height", "dimen", "android");
        if (resourceId > 0)
        {
            result = context.Resources.GetDimensionPixelSize(resourceId);
        }
        return result;
    }

    //https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/services/foreground-services
    public static void StartForegroundServiceCompat<T>(this Context context, string action = null, Bundle args = null) where T : Service
    {
        var intent = new Intent(context, typeof(T));

        if (action != null)
        {
            intent.SetAction(action);
        }

        if (args != null)
        {
            intent.PutExtras(args);
        }

        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
        {
            context.StartForegroundService(intent);
        }
        else
        {
            context.StartService(intent);
        }
    }
}
