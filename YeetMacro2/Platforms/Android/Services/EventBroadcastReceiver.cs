using Android.App;
using Android.Content;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Android.Services;
[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter(new[] { "com.companyname.ForegroundService.EXIT", "com.companyname.AccessibilityService.CHANGED" })]
public class EventBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] EventBroadcastReceiver");
            switch (intent.Action)
            {
                case "com.companyname.ForegroundService.EXIT":
                    ServiceHelper.GetService<AndriodHomeViewModel>().IsProjectionServiceEnabled = false;
                    ServiceHelper.GetService<MacroManagerViewModel>().ShowLogView = false;
                    ServiceHelper.GetService<MacroManagerViewModel>().InDebugMode = false;
                    break;
                case "com.companyname.AccessibilityService.CHANGED":
                    bool enabled = intent.GetBooleanExtra("enabled", false);
                    ServiceHelper.GetService<AndriodHomeViewModel>().IsAccessibilityEnabled = enabled;
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
