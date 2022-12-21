using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using YeetMacro2.Data.Services;
using YeetMacro2.ViewModels;
namespace YeetMacro2.Services;

public static class ServiceRegistrationHelper
{
    public static MauiAppBuilder RegisterServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<IToastService, ToastService>();
        mauiAppBuilder.Services.AddAutoMapper(typeof(App).GetTypeInfo().Assembly);
        mauiAppBuilder.Services.AddYeetMacroData(setup =>
        {
            SQLitePCL.Batteries_V2.Init();
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
            setup.UseSqlite($"Filename={dbPath}");
        });

        return mauiAppBuilder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<HomeViewModel>();
        mauiAppBuilder.Services.AddSingleton<ActionViewModel>();
        mauiAppBuilder.Services.AddSingleton<ActionMenuViewModel>();
        mauiAppBuilder.Services.AddSingleton<LogViewModel>();
        mauiAppBuilder.Services.AddSingleton<PromptStringInputViewModel>();
        mauiAppBuilder.Services.AddSingleton<PatternTreeViewViewModel>();
        mauiAppBuilder.Services.AddSingleton<IMacroService, MacroService>();

        return mauiAppBuilder;
    }
}
