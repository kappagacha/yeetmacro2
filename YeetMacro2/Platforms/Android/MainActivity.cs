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
using YeetMacro2.Platforms.Android;

namespace YeetMacro2;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    readonly EventBroadcastReceiver receiver = new();
    private CustomOrientationListener _orientationListener;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        Console.WriteLine("[*****YeetMacro*****] MainActivity OnCreate");
        //https://stackoverflow.com/questions/11939192/unsatisfied-link-error-opencv-for-android-non-native
        if (!OpenCVLoader.InitDebug())
        {
            // Handle initialization error
            Console.WriteLine("[*****YeetMacro*****] OpenCVLoader.InitDebug ERROR");
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            RegisterReceiver(receiver, new IntentFilter("com.yeetoverflow.AccessibilityService.CHANGED"), ReceiverFlags.Exported);
            RegisterReceiver(receiver, new IntentFilter("com.yeetoverflow.MediaProjectionService.STARTED"), ReceiverFlags.Exported);
        }
        else
        {
            RegisterReceiver(receiver, new IntentFilter("com.yeetoverflow.AccessibilityService.CHANGED"));
            RegisterReceiver(receiver, new IntentFilter("com.yeetoverflow.MediaProjectionService.STARTED"));
        }

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Console.WriteLine("[*****YeetMacro*****] CurrentDomain UnhandledException: " + args.ExceptionObject.ToString());
            if (args.IsTerminating)
            {
                ServiceHelper.GetService<YeetAccessibilityService>().Stop();
            }
            ServiceHelper.GetService<LogServiceViewModel>().LogException(args.ExceptionObject as Exception);
        };

        // https://gist.github.com/mattjohnsonpint/7b385b7a2da7059c4a16562bc5ddb3b7
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            Console.WriteLine("[*****YeetMacro*****] UnhandledExceptionRaiser UnhandledException: " + args.Exception.Message);
            ServiceHelper.GetService<LogServiceViewModel>().LogException(args.Exception );
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            Console.WriteLine("[*****YeetMacro*****] TaskScheduler UnobservedTaskException: " + args.Exception.Message);
            ServiceHelper.GetService<LogServiceViewModel>().LogException(args.Exception);
        };

        // https://stackoverflow.com/questions/74165500/net-maui-how-to-ensure-that-android-platform-specific-code-is-only-executed-on
        //if (OperatingSystem.IsAndroidVersionAtLeast(30)) // hide navigation bars
        //{
        //    Window?.InsetsController?.Hide(WindowInsets.Type.NavigationBars());
        //    Window?.InsetsController?.Hide(WindowInsets.Type.SystemBars());
        //}


        _orientationListener = new CustomOrientationListener(this);
        _orientationListener.Enable();

        base.OnCreate(savedInstanceState);
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        //ServiceHelper.GetService<LogServiceViewModel>().LogDebug($"MainActivity.OnActivityResult requestCode: {requestCode}");
        //ServiceHelper.GetService<LogServiceViewModel>().LogDebug($"MainActivity.OnActivityResult resultCode: {resultCode}");
        switch (requestCode)
        {
            case Platforms.Android.Services.MediaProjectionService.REQUEST_MEDIA_PROJECTION:
                ServiceHelper.GetService<MediaProjectionService>().Init(resultCode, data);
                break;
            case AndriodHomeViewModel.REQUEST_IGNORE_BATTERY_OPTIMIZATIONS:
                ServiceHelper.GetService<AndriodHomeViewModel>().InvokeOnPropertyChanged(nameof(AndriodHomeViewModel.IsIgnoringBatteryOptimizations));
                break;
            case AndroidScreenService.OVERLAY_SERVICE_REQUEST:
                WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(this, nameof(AndroidScreenService.CanDrawOverlays), requestCode != 0, resultCode == 0), nameof(AndroidScreenService));
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
        _orientationListener?.Enable();
    }

    protected override void OnPause()
    {
        Console.WriteLine("[*****YeetMacro*****] MainActivity OnPause");
        base.OnPause();
        _orientationListener?.Disable();
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
}
