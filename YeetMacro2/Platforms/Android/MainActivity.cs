﻿using Android.App;
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
using YeetMacro2.Data.Models;

namespace YeetMacro2;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    readonly EventBroadcastReceiver receiver = new();

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
            ServiceHelper.GetService<LogServiceViewModel>().LogDebug("CurrentDomain UnhandledException: " + args.ExceptionObject.ToString());
            //if (args.IsTerminating)
            //{
            //    ServiceHelper.GetService<YeetAccessibilityService>().Stop();
            //}
            ServiceHelper.GetService<LogServiceViewModel>().LogException(args.ExceptionObject as Exception);
        };

        // https://gist.github.com/mattjohnsonpint/7b385b7a2da7059c4a16562bc5ddb3b7
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            ServiceHelper.GetService<LogServiceViewModel>().LogDebug("UnhandledExceptionRaiser UnhandledException: " + args.Exception.Message);
            ServiceHelper.GetService<LogServiceViewModel>().LogException(args.Exception );
            args.Handled = true; // Prevent app termination
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            ServiceHelper.GetService<LogServiceViewModel>().LogDebug("TaskScheduler UnhandledUnobservedTaskExceptionException: " + args.Exception.Message);
            ServiceHelper.GetService<LogServiceViewModel>().LogException(args.Exception);
            args.SetObserved(); // Mark as observed to prevent termination
        };

        // https://stackoverflow.com/questions/74165500/net-maui-how-to-ensure-that-android-platform-specific-code-is-only-executed-on
        //if (OperatingSystem.IsAndroidVersionAtLeast(30)) // hide navigation bars
        //{
        //    Window?.InsetsController?.Hide(WindowInsets.Type.NavigationBars());
        //    Window?.InsetsController?.Hide(WindowInsets.Type.SystemBars());
        //}

        base.OnCreate(savedInstanceState);
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        ServiceHelper.GetService<LogServiceViewModel>().LogDebug($"MainActivity.OnActivityResult requestCode: {requestCode}");
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
        ServiceHelper.GetService<LogServiceViewModel>().LogDebug($"MainActivity.OnStart");
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        DeviceDisplay_MainDisplayInfoChanged(null, new DisplayInfoChangedEventArgs(DeviceDisplay.MainDisplayInfo));
        base.OnStart();
    }

    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        DisplayHelper.DisplayRotation = e.DisplayInfo.Rotation;
        DisplayHelper.DisplayInfo = e.DisplayInfo;
        WeakReferenceMessenger.Default.Send(e);
    }

    protected override void OnResume()
    {
        ServiceHelper.GetService<LogServiceViewModel>().LogDebug($"MainActivity.OnResume");
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        base.OnResume();
    }

    protected override void OnPause()
    {
        ServiceHelper.GetService<LogServiceViewModel>().LogDebug($"MainActivity.OnPause");
        DeviceDisplay.MainDisplayInfoChanged -= DeviceDisplay_MainDisplayInfoChanged;
        base.OnPause();
    }

    protected override void OnDestroy()
    {
        ServiceHelper.GetService<LogServiceViewModel>().LogDebug($"MainActivity.OnDestroy");
        DeviceDisplay.MainDisplayInfoChanged -= DeviceDisplay_MainDisplayInfoChanged;
        base.OnDestroy();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
        ServiceHelper.GetService<LogServiceViewModel>().LogDebug($"MainActivity.OnRequestPermissionsResult " + requestCode + " " + grantResults); 
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}
