using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Extensions.Logging;
using Serilog.Filters;
using System.Reflection;
using TesseractOcrMaui;
using YeetMacro2.ViewModels;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Services;

public static class ServiceRegistrationHelper
{
    public static MauiAppBuilder RegisterServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddTransient<IMauiInitializeService, AppInitializer>();
        mauiAppBuilder.Services.AddSingleton<IToastService, ToastService>();
        mauiAppBuilder.Services.AddSingleton<IScriptService, ScriptService>();
        mauiAppBuilder.Services.AddHttpClient();
        mauiAppBuilder.Services.AddSingleton<IHttpService, HttpService>();
        //mauiAppBuilder.Services.AddSingleton<IOcrService, OcrService>();
        mauiAppBuilder.Services.AddAutoMapper(typeof(App).GetTypeInfo().Assembly);
        mauiAppBuilder.Services.AddLazyResolution();
        mauiAppBuilder.Logging.AddLogViewModelSink();
        mauiAppBuilder.Services.AddSingleton<MacroService>();

        mauiAppBuilder.Services.AddTesseractOcr(files => files.AddFile("eng.traineddata"));

        return mauiAppBuilder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<MacroManagerViewModel>();
        mauiAppBuilder.Services.AddSingleton<NodeManagerViewModelFactory>();
        mauiAppBuilder.Services.AddSingleton<StatusPanelViewModel>();
        mauiAppBuilder.Services.AddSingleton<LogViewModel>();

        return mauiAppBuilder;
    }
}

public class AppInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
#if ANDROID
        //try
        //{

        //    var assets = MauiApplication.Current.Assets;
        //    //var x = Android.Graphics.Typeface.CreateFromAsset(assets, "/data/user/0/com.companyname.yeetmacro2/cache/MaterialIconsOutlined-Regular.otf");
        //    var x = Android.Graphics.Typeface.CreateFromAsset(assets, "MaterialOutlined");
        //} 
        //catch (Exception ex)
        //{

        //}

#endif
        //var assemblies = new Assembly[] { typeof(MaterialIconsConfigurationExtensions).Assembly, typeof(FontAwesomeConfigurationExtensions).Assembly };
        //foreach (var assembly in assemblies)
        //{
        //    string[] fontFiles = assembly.GetManifestResourceNames();
        //    var regex = new Regex(@"([^\.]+)\.(otf|ttf)$");
        //    foreach (var fontFile in fontFiles)
        //    {
        //        var match = regex.Match(fontFile);
        //        if (match.Success)
        //        {
        //            var fontData = assembly.GetManifestResourceStream(fontFile);
        //            var targetPath = Path.Combine(FileSystem.Current.CacheDirectory, $"{match.Groups[1].Value}.{match.Groups[2].Value}");
        //            Console.WriteLine("***********");
        //            Console.WriteLine(targetPath);
        //            if (File.Exists(targetPath)) continue;
        //            var fileStream = File.Create(targetPath);
        //            fontData.CopyTo(fileStream);
        //        }
        //    }
        //}
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

// https://github.com/adiamante/yeetoverflow/blob/main/YeetOverFlow.Logging/YeetLoggerServiceCollectionExtensions.cs
// https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/SerilogLoggingBuilderExtensions.cs
public static class StatusPanelModelSinkExtensions
{
    public static ILoggingBuilder AddLogViewModelSink(this ILoggingBuilder builder, Action<LoggerConfiguration> setupAction = null)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        
        builder.Services.AddSingleton<ILoggerProvider, SerilogLoggerProvider>(sp =>
        {
            var logViewModel = sp.GetService<LogViewModel>();
            // https://improveandrepeat.com/2014/08/structured-logging-with-serilog/
            // https://github.com/serilog/serilog/wiki/Formatting-Output
            // https://github.com/serilog/serilog-formatting-compact
            var configuration = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .WriteTo.LogViewModelSink(logViewModel)
                // https://github.com/serilog/serilog/wiki/Configuration-Basics#filters
                .Filter.ByExcluding(Matching.WithProperty<string>("SourceContext", sctx => sctx.StartsWith("Microsoft.")));
                
            setupAction?.Invoke(configuration);
            var logger = configuration.CreateLogger();
            return new SerilogLoggerProvider(logger, true);
        });

        builder.AddFilter<SerilogLoggerProvider>(null, LogLevel.Trace);

        return builder;
    }

    public static LoggerConfiguration LogViewModelSink(
              this LoggerSinkConfiguration loggerConfiguration,
              LogViewModel logViewModel)
    {
        return loggerConfiguration.Sink(logViewModel);
    }
}