using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Filters;
using System.Reflection;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
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
        mauiAppBuilder.Logging.AddLogViewModelSink();

        return mauiAppBuilder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<MacroManagerViewModel>();
        mauiAppBuilder.Services.AddSingleton<NodeViewModelFactory>();
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

        var patternNodeService = services.GetService<INodeService<PatternNode, PatternNode>>();
        var disgaeaPatternNode = patternNodeService.GetRoot(-1);
        var konosubaPatternNode = patternNodeService.GetRoot(-1);


        var scriptNodeService = services.GetService<INodeService<ScriptNode, ScriptNode>>();
        var disgaeaScriptNode = scriptNodeService.GetRoot(-1);
        var konosubaScriptNode = scriptNodeService.GetRoot(-1);

        var settingNodeService = services.GetService<INodeService<ParentSetting, SettingNode>>();
        var disgaeaSettingNode = settingNodeService.GetRoot(-1);
        var konosubaSettingNode = settingNodeService.GetRoot(-1);

        var disgaeaRpgMacroSet = new MacroSet() { Name = "Disgaea RPG", RootPatternNodeId = disgaeaPatternNode.NodeId, RootScriptNodeId = disgaeaScriptNode.NodeId, RootSettingNodeId = disgaeaSettingNode.NodeId };
        var konsobaFdMacroSet = new MacroSet() { Name = "Konosuba FD", RootPatternNodeId = konosubaPatternNode.NodeId, RootScriptNodeId = konosubaScriptNode.NodeId, RootSettingNodeId = konosubaSettingNode.NodeId };

        dbContext.MacroSets.AddRange(disgaeaRpgMacroSet, konsobaFdMacroSet);
        dbContext.SaveChanges();
    }
}

//https://github.com/serilog/serilog/wiki/Developing-a-sink
public class LogViewModelSink : ILogEventSink
{
    LogViewModel _logViewModel;
    public LogViewModelSink(LogViewModel logViewModel)
    {
        _logViewModel = logViewModel;
    }

    public void Emit(LogEvent logEvent)
    {
        switch (logEvent.Level)
        {
            case LogEventLevel.Debug:
                _logViewModel.Debug = logEvent.MessageTemplate.Text;
                break;
            case LogEventLevel.Information:
                _logViewModel.Info= logEvent.MessageTemplate.Text;
                break;
        }
    }
}

// https://github.com/adiamante/yeetoverflow/blob/main/YeetOverFlow.Logging/YeetLoggerServiceCollectionExtensions.cs
// https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/SerilogLoggingBuilderExtensions.cs
public static class LogViewModelSinkExtensions
{
    public static ILoggingBuilder AddLogViewModelSink(this ILoggingBuilder builder, Action<LoggerConfiguration> setupAction = null)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.Services.AddSingleton<ILoggerProvider, SerilogLoggerProvider>(sp =>
        {
            var logViewModel = sp.GetRequiredService<LogViewModel>();

            // https://improveandrepeat.com/2014/08/structured-logging-with-serilog/
            // https://github.com/serilog/serilog/wiki/Formatting-Output
            // https://github.com/serilog/serilog-formatting-compact
            var configuration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.LogViewModelSink(logViewModel)
                // https://github.com/serilog/serilog/wiki/Configuration-Basics#filters
                .Filter.ByExcluding(Matching.WithProperty<string>("SourceContext", sctx => sctx.StartsWith("Microsoft.")));

            setupAction?.Invoke(configuration);
            var logger = configuration.CreateLogger();

            return new SerilogLoggerProvider(logger, true);
        });


        builder.AddFilter<SerilogLoggerProvider>(null, LogLevel.Debug);

        return builder;
    }

    public static LoggerConfiguration LogViewModelSink(
              this LoggerSinkConfiguration loggerConfiguration,
              LogViewModel logViewModel)
    {
        return loggerConfiguration.Sink(new LogViewModelSink(logViewModel));
    }
}