using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Xamarin.CommunityToolkit.Effects;
using YeetMacro2.Data.Services;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms;

public static class PlatformServiceRegistrationHelper
{
    public static MauiAppBuilder RegisterPlatformServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddYeetMacroData(setup =>
        {
            SQLitePCL.Batteries_V2.Init();
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
            setup.UseSqlite($"Filename={dbPath}");
        });

        mauiAppBuilder.Services.AddSingleton<AndriodHomeViewModel>();
        mauiAppBuilder.Services.AddSingleton<IHomeViewModel, AndriodHomeViewModel>((sp) =>
        {
            return sp.GetService<AndriodHomeViewModel>();
        });
        mauiAppBuilder.Services.AddSingleton<ActionViewModel>();
        mauiAppBuilder.Services.AddSingleton<ActionMenuViewModel>();
        mauiAppBuilder.Services.AddSingleton<IWindowManagerService, WindowManagerService>();
        mauiAppBuilder.Services.AddSingleton<IMediaProjectionService, MediaProjectionService>();
        mauiAppBuilder.Services.AddSingleton<IAccessibilityService, YeetAccessibilityService>();

        // https://github.com/xamarin/XamarinCommunityToolkit/issues/1905
        mauiAppBuilder
        .UseMauiCompatibility()
        .ConfigureMauiHandlers(handlers =>
        {
            handlers.AddCompatibilityRenderers(typeof(Xamarin.CommunityToolkit.Effects.TouchEffect).Assembly);
        })
        .ConfigureEffects(effects =>
        {
            effects.Add(typeof(TouchEffect), typeof(PlatformTouchEffect));
        });

        return mauiAppBuilder;
    }
}
