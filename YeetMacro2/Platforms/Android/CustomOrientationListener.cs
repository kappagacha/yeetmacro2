using Android.Content;
using Android.Views;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Android;
public class CustomOrientationListener : OrientationEventListener
{
    public CustomOrientationListener(Context context) : base(context)
    {
    }

    public override void OnOrientationChanged(int orientation)
    {
        if (orientation == -1) return;

        ServiceHelper.GetService<LogServiceViewModel>().LogInfo($"Orientation: ${orientation}");

        switch (orientation)
        {
            case 0: PatternHelper.DisplayRotation = DisplayRotation.Rotation0; break;
            case 90: PatternHelper.DisplayRotation = DisplayRotation.Rotation90; break;
            case 180: PatternHelper.DisplayRotation = DisplayRotation.Rotation180; break;
            case 270: PatternHelper.DisplayRotation = DisplayRotation.Rotation270; break;
        }
    }
}
