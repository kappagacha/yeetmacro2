using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms;

public static class PlatformServiceRegistrationHelper
{
    public static MauiAppBuilder RegisterPlatformServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<IWindowManagerService, WindowManagerService>();

        return mauiAppBuilder;
    }
}
