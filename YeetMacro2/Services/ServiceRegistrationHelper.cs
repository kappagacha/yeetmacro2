using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Filters;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Services;

public static class ServiceRegistrationHelper
{
    public static MauiAppBuilder RegisterServices(this MauiAppBuilder mauiAppBuilder)
    {
        //mauiAppBuilder.Services.AddTransient<IMauiInitializeService, AppInitializer>();
        mauiAppBuilder.Services.AddSingleton<IToastService, ToastService>();
        mauiAppBuilder.Services.AddSingleton<IScriptService, ScriptService>();
        mauiAppBuilder.Services.AddHttpClient();
        mauiAppBuilder.Services.AddSingleton<IHttpService, HttpService>();
        mauiAppBuilder.Services.AddAutoMapper(typeof(App).GetTypeInfo().Assembly);
        mauiAppBuilder.Services.AddLazyResolution();
        mauiAppBuilder.Logging.AddLogViewModelSink();

        return mauiAppBuilder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<MacroManagerViewModel>();
        mauiAppBuilder.Services.AddSingleton<NodeViewModelFactory>();
        mauiAppBuilder.Services.AddSingleton<StatusPanelViewModel>();
        mauiAppBuilder.Services.AddSingleton<LogViewModel>();

        return mauiAppBuilder;
    }
}

public class AppInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<YeetMacroDbContext>();

        if (dbContext.MacroSets.Any()) return;

        var macroSetRepository = services.GetRequiredService<IRepository<MacroSet>>();
        var patternNodeService = services.GetRequiredService<INodeService<PatternNode, PatternNode>>();
        var scriptNodeService = services.GetRequiredService<INodeService<ScriptNode, ScriptNode>>();
        var settingNodeService = services.GetRequiredService<INodeService<ParentSetting, SettingNode>>();
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            Converters = {
                new JsonStringEnumConverter()
            },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var macroSets = ServiceHelper.ListAssets("MacroSets");
        foreach (var folder in macroSets)
        {
            var macroSetJson = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", folder, "macroSet.json"));
            var macroSet = JsonSerializer.Deserialize<MacroSet>(macroSetJson, jsonSerializerOptions);
            var pattternJson = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", folder, "patterns.json"));
            var rootPattern = patternNodeService.GetRoot(0);
            var tempPatternTree = PatternNodeViewModel.FromJson(pattternJson);
            foreach (var pattern in tempPatternTree.Root.Nodes)
            {
                pattern.RootId = rootPattern.NodeId;
                pattern.ParentId = rootPattern.NodeId;
                rootPattern.Nodes.Add(pattern);
                patternNodeService.Insert(pattern);
            }
            macroSet.RootPatternNodeId = rootPattern.NodeId;

            var scriptList = ServiceHelper.ListAssets(Path.Combine("MacroSets", folder, "scripts"));
            var rootScripts = scriptNodeService.GetRoot(0);
            foreach (var scriptFile in scriptList)
            {
                var scriptText = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", folder, "scripts", scriptFile));
                var script = new ScriptNode()
                {
                    Name = Path.GetFileNameWithoutExtension(scriptFile),
                    Text = scriptText,
                    RootId = rootScripts.NodeId,
                    ParentId = rootScripts.NodeId
                };

                rootScripts.Nodes.Add(script);
                scriptNodeService.Insert(script);
            }

            macroSet.RootScriptNodeId = rootScripts.NodeId;

            var settingJson = ServiceHelper.GetAssetContent(Path.Combine("MacroSets", folder, "settings.json"));
            var rootSetting = settingNodeService.GetRoot(0);
            var tempSettingTree = SettingNodeViewModel.FromJson(settingJson);
            foreach (var setting in tempSettingTree.Root.Nodes)
            {
                setting.RootId = rootSetting.NodeId;
                setting.ParentId = rootSetting.NodeId;
                rootSetting.Nodes.Add(setting);
                settingNodeService.Insert(setting);
            }
            macroSet.RootSettingNodeId = rootSetting.NodeId;

            macroSetRepository.Insert(macroSet);
            macroSetRepository.Save();
        }
    }
}

// https://stackoverflow.com/questions/62217815/interface-a-circular-dependency-was-detected-for-the-service-of-type/62254531#62254531
public static class LazyResolutionMiddlewareExtensions
{
    public static IServiceCollection AddLazyResolution(this IServiceCollection services)
    {
        return services.AddTransient(
            typeof(Lazy<>),
            typeof(LazilyResolved<>));
    }
}

public class LazilyResolved<T> : Lazy<T>
{
    public LazilyResolved(IServiceProvider serviceProvider)
        : base(serviceProvider.GetRequiredService<T>)
    {
    }
}

//https://github.com/serilog/serilog/wiki/Developing-a-sink
public class LogViewModelSink : ILogEventSink
{
    Lazy<LogViewModel> _logViewModel;
    public LogViewModelSink(IServiceProvider serviceProvider)
    {
        // Using Lazy to resolve later because of circular dependency
        _logViewModel = serviceProvider.GetService<Lazy<LogViewModel>>();
    }

    public void Emit(LogEvent logEvent)
    {
        switch (logEvent.Level)
        {
            case LogEventLevel.Debug:
                _logViewModel.Value.Debug = logEvent.MessageTemplate.Text;
                break;
            case LogEventLevel.Information:
                _logViewModel.Value.Info = logEvent.MessageTemplate.Text;
                break;
        }

        Debug.WriteLine("{0} {1}", logEvent.Level, logEvent.MessageTemplate.Text);
    }
}

// https://github.com/adiamante/yeetoverflow/blob/main/YeetOverFlow.Logging/YeetLoggerServiceCollectionExtensions.cs
// https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/SerilogLoggingBuilderExtensions.cs
public static class StatusPanelModelSinkExtensions
{
    public static ILoggingBuilder AddLogViewModelSink(this ILoggingBuilder builder, Action<LoggerConfiguration> setupAction = null)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.Services.AddSingleton<ILoggerProvider, SerilogLoggerProvider>(sp =>
        {
            // https://improveandrepeat.com/2014/08/structured-logging-with-serilog/
            // https://github.com/serilog/serilog/wiki/Formatting-Output
            // https://github.com/serilog/serilog-formatting-compact
            var configuration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.LogViewModelSink(sp)
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
              IServiceProvider serviceProvider)
    {
        return loggerConfiguration.Sink(new LogViewModelSink(serviceProvider));
    }
}