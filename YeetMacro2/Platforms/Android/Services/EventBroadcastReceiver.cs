using Android.App;
using Android.Content;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Android.Services;
[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter(new[] { "com.yeetoverflow.AccessibilityService.CHANGED" })]
public class EventBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        try
        {
            Console.WriteLine($"[*****YeetMacro*****] EventBroadcastReceiver: {intent.Action}");
            switch (intent.Action)
            {
                case "com.yeetoverflow.AccessibilityService.CHANGED":
                    bool enabled = intent.GetBooleanExtra("enabled", false);
                    var homeViewModel = ServiceHelper.GetService<AndriodHomeViewModel>();
                    homeViewModel.IsAccessibilityEnabled = enabled;
                    homeViewModel.IsMacroReady = enabled && homeViewModel.IsProjectionServiceEnabled;
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
