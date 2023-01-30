using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Xamarin.CommunityToolkit.Effects;
using YeetMacro2.Data.Services;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Services;

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
        mauiAppBuilder.Services.AddSingleton<ActionViewModel>();
        mauiAppBuilder.Services.AddSingleton<ActionMenuViewModel>();
        mauiAppBuilder.Services.AddSingleton<PromptStringInputViewModel>();
        mauiAppBuilder.Services.AddSingleton<PromptSelectOptionViewModel>();
        mauiAppBuilder.Services.AddSingleton<AndroidWindowManagerService>();
        mauiAppBuilder.Services.AddSingleton<MediaProjectionService>();
        mauiAppBuilder.Services.AddSingleton<RecorderService>();
        mauiAppBuilder.Services.AddSingleton<YeetAccessibilityService>();
        mauiAppBuilder.Services.AddSingleton<IScreenService>(sp => sp.GetRequiredService<AndroidWindowManagerService>());
        mauiAppBuilder.Services.AddSingleton<IInputService>(sp => sp.GetRequiredService<AndroidWindowManagerService>());
        mauiAppBuilder.Services.AddSingleton<IRecorderService>(sp => sp.GetRequiredService<RecorderService>());

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
