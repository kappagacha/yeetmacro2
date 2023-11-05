using Android.App;
using Android.Content;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Android.Services;
[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter(new[] { "com.companyname.AccessibilityService.CHANGED", "com.companyname.MediaProjectionService.STARTED" })]
public class EventBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        try
        {
            Console.WriteLine($"[*****YeetMacro*****] EventBroadcastReceiver: {intent.Action}");
            switch (intent.Action)
            {
                case "com.companyname.AccessibilityService.CHANGED":
                    bool enabled = intent.GetBooleanExtra("enabled", false);
                    var homeViewModel = ServiceHelper.GetService<AndriodHomeViewModel>();
                    homeViewModel.IsAccessibilityEnabled = enabled;
                    homeViewModel.IsMacroReady = enabled && homeViewModel.IsProjectionServiceEnabled;
                    break;
                case "com.companyname.MediaProjectionService.STARTED":
                    ServiceHelper.GetService<AndroidScreenService>().Show(AndroidWindowView.ActionView);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] EventBroadcastReceiver " + intent.Action);
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }
    }
}
