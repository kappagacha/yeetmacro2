using CommunityToolkit.Maui;
using System.Reflection;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Platforms;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Services;

public static class ServiceRegistrationHelper
{
    public static MauiAppBuilder RegisterServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddTransient<IMauiInitializeService, AppInitializer>();
        mauiAppBuilder.Services.AddSingleton<IToastService, ToastService>();
        mauiAppBuilder.Services.AddSingleton<IScriptsService, ScriptsService>();
        mauiAppBuilder.Services.AddAutoMapper(typeof(App).GetTypeInfo().Assembly);

        return mauiAppBuilder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<MacroManagerViewModel>();
        mauiAppBuilder.Services.AddSingleton<PatternTreeViewViewModelFactory>();
        mauiAppBuilder.Services.AddSingleton<ScriptsViewModelFactory>();
        mauiAppBuilder.Services.AddSingleton<LogViewModel>();
        mauiAppBuilder.Services.AddSingleton<IMacroService, MacroService>();

        return mauiAppBuilder;
    }
}

public class AppInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<YeetMacroDbContext>();

        if (dbContext.MacroSets.Any()) return;

        var nodeService = services.GetService<INodeService<PatternNode, PatternNode>>();
        var konosubaPatternNode = new PatternNode() { Name = "root" };
        var disgaeaPatternNode = new PatternNode() { Name = "root" };
        nodeService.Insert(konosubaPatternNode);
        nodeService.Insert(disgaeaPatternNode);

        var konsobaFdMacroSet = new MacroSet() { Name = "Konosuba FD", RootPatternNodeId = konosubaPatternNode.NodeId };
        var disgaeaRpgMacroSet = new MacroSet() { Name = "Disgaea RPG", RootPatternNodeId = disgaeaPatternNode.NodeId };
        dbContext.MacroSets.AddRange(konsobaFdMacroSet, disgaeaRpgMacroSet);
        dbContext.SaveChanges();
    }
}