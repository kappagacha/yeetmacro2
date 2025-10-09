using Microsoft.EntityFrameworkCore;
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
        }, ServiceLifetime.Transient);

        mauiAppBuilder.Services.AddSingleton<AndriodHomeViewModel>();
        mauiAppBuilder.Services.AddSingleton<OpenCvService>();
        mauiAppBuilder.Services.AddSingleton<ActionViewModel>();
        mauiAppBuilder.Services.AddSingleton<PromptStringInputViewModel>();
        mauiAppBuilder.Services.AddSingleton<PromptSelectOptionViewModel>();
        mauiAppBuilder.Services.AddSingleton<MessageViewModel>();
        mauiAppBuilder.Services.AddSingleton<MediaProjectionService>();
        mauiAppBuilder.Services.AddSingleton<RecorderService>();
        mauiAppBuilder.Services.AddSingleton<YeetAccessibilityService>();
        mauiAppBuilder.Services.AddSingleton<TestViewModel>();
        mauiAppBuilder.Services.AddSingleton<AndroidScreenService>();
        mauiAppBuilder.Services.AddSingleton<IOcrService, AndroidOcrService>();
        //mauiAppBuilder.Services.AddSingleton<IOcrService, OcrService>();
        mauiAppBuilder.Services.AddSingleton<IInputService, AndroidInputService>();
        mauiAppBuilder.Services.AddSingleton<IScreenService>(sp => sp.GetRequiredService<AndroidScreenService>());
        mauiAppBuilder.Services.AddSingleton<IRecorderService>(sp => sp.GetRequiredService<RecorderService>());

        return mauiAppBuilder;
    }
}
