using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.Runtime;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;
using Android.OS;
//using OpenCV.Android;
using Org.Opencv.Android;
using YeetMacro2.ViewModels;

namespace YeetMacro2;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{

    EventBroadcastReceiver receiver = new EventBroadcastReceiver();

    protected override void OnCreate(Bundle savedInstanceState)
    {
        Console.WriteLine("[*****YeetMacro*****] MainActivity OnCreate");
        //https://stackoverflow.com/questions/11939192/unsatisfied-link-error-opencv-for-android-non-native
        if (!OpenCVLoader.InitDebug())
        {
            // Handle initialization error
            Console.WriteLine("[*****YeetMacro*****] OpenCVLoader.InitDebug ERROR");
        }

        RegisterReceiver(receiver, new IntentFilter("com.companyname.ForegroundService.EXIT"));
        RegisterReceiver(receiver, new IntentFilter("com.companyname.AccessibilityService.CHANGED"));
        RegisterReceiver(receiver, new IntentFilter("com.companyname.MediaProjectionService.STARTED"));

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Console.WriteLine("[*****YeetMacro*****] UnhandledException: " + args.ExceptionObject.ToString());
            if (args.IsTerminating)
            {
                ServiceHelper.GetService<YeetAccessibilityService>().Stop();
            }
            ServiceHelper.GetService<LogViewModel>().LogException(args.ExceptionObject as Exception);
        };

        base.OnCreate(savedInstanceState);
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        switch (requestCode)
        {
            case YeetMacro2.Platforms.Android.Services.MediaProjectionService.REQUEST_MEDIA_PROJECTION:
                ServiceHelper.GetService<MediaProjectionService>().Start(resultCode, data);
                break;
        }
    }

    protected override void OnResume()
    {
        Console.WriteLine("[*****YeetMacro*****] MainActivity OnResume");
        base.OnResume();
    }

    protected override void OnPause()
    {
        Console.WriteLine("[*****YeetMacro*****] MainActivity OnPause");
        base.OnPause();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}
