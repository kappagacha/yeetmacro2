using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.Runtime;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;

namespace YeetMacro2;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{

    EventBroadcastReceiver receiver = new EventBroadcastReceiver();

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        switch (requestCode)
        {
            case YeetMacro2.Platforms.Android.Services.MediaProjectionService.REQUEST_MEDIA_PROJECTION:
                var mediaProjectionService = ServiceHelper.GetService<IMediaProjectionService>();
                ((MediaProjectionService)mediaProjectionService).Start(resultCode, data);
                break;
        }
    }

    protected override void OnResume()
    {
        Console.WriteLine("[*****YeetMacro*****] MainActivity OnResume");
        base.OnResume();
        RegisterReceiver(receiver, new IntentFilter("com.companyname.ForegroundService.EXIT"));
        RegisterReceiver(receiver, new IntentFilter("com.companyname.AccessibilityService.CHANGED"));
    }

    protected override void OnPause()
    {
        Console.WriteLine("[*****YeetMacro*****] MainActivity OnPause");
        UnregisterReceiver(receiver);
        base.OnPause();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}
