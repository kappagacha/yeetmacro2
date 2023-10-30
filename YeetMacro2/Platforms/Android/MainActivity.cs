using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.Runtime;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;
using Android.OS;
using Org.Opencv.Android;
using YeetMacro2.ViewModels;
using YeetMacro2.Platforms.Android.ViewModels;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Android.Views;

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
            Console.WriteLine("[*****YeetMacro*****] CurrentDomain UnhandledException: " + args.ExceptionObject.ToString());
            if (args.IsTerminating)
            {
                ServiceHelper.GetService<YeetAccessibilityService>().Stop();
            }
            ServiceHelper.GetService<LogViewModel>().LogException(args.ExceptionObject as Exception);
        };

        // https://gist.github.com/mattjohnsonpint/7b385b7a2da7059c4a16562bc5ddb3b7
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            Console.WriteLine("[*****YeetMacro*****] UnhandledExceptionRaiser UnhandledException: " + args.Exception.Message);
            ServiceHelper.GetService<LogViewModel>().LogException(args.Exception );
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            Console.WriteLine("[*****YeetMacro*****] TaskScheduler UnobservedTaskException: " + args.Exception.Message);
            ServiceHelper.GetService<LogViewModel>().LogException(args.Exception);
        };

        base.OnCreate(savedInstanceState);
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        switch (requestCode)
        {
            case Platforms.Android.Services.MediaProjectionService.REQUEST_MEDIA_PROJECTION:
                ServiceHelper.GetService<MediaProjectionService>().Start(resultCode, data);
                break;
            case AndriodHomeViewModel.REQUEST_IGNORE_BATTERY_OPTIMIZATIONS:
                ServiceHelper.GetService<AndriodHomeViewModel>().InvokeOnPropertyChanged(nameof(AndriodHomeViewModel.IsIgnoringBatteryOptimizations));
                break;
        }
    }

    protected override void OnStart()
    {
        Console.WriteLine("[*****YeetMacro*****] MainActivity OnStart");
        base.OnStart();
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

    protected override void OnDestroy()
    {
        Console.WriteLine("[*****YeetMacro*****] MainActivity OnDestroy");
        base.OnDestroy();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        var size = GetScreenResolution();
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<Size>(this, nameof(DisplayInfo), size, size), nameof(DisplayInfo));
    }

    // https://stackoverflow.com/questions/11252067/how-do-i-get-the-screensize-programmatically-in-android
    // https://stackoverflow.com/questions/63719160/getsize-deprecated-in-api-level-30
    public Size GetScreenResolution()
    {
        var windowManager = this.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.R)
        {
            var rect = windowManager.CurrentWindowMetrics.Bounds;
            return new Size(rect.Width(), rect.Height());
        }
        else
        {
            Display display = windowManager.DefaultDisplay;
            var point = new global::Android.Graphics.Point();
            display.GetSize(point);
            return new Size(point.X, point.Y);
        }
    }
}
