using Android.App;
using Android.Runtime;

namespace YeetMacro2;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    public override void OnCreate()
    {
        // Handle error with IElement.ToPlatform
        SetTheme(Resource.Style.Theme_MaterialComponents_DayNight);
        base.OnCreate();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
