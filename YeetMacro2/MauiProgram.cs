using Microsoft.Extensions.Logging;
using YeetMacro2.Services;
using YeetMacro2.Platforms;
using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Xamarin.CommunityToolkit.Effects;
using UraniumUI;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Controls;

namespace YeetMacro2;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder
			.UseUraniumUI()
			.UseUraniumUIMaterial()
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddUraniumUIHandlers();
			});


		builder
			.UseSkiaSharp();

        builder.UseMauiApp<App>();
        builder.UseMauiCommunityToolkit();
        builder.RegisterViewModels();
		builder.RegisterServices();
		builder.RegisterPlatformServices();

        // https://github.com/xamarin/XamarinCommunityToolkit/issues/1905
        builder
        .UseMauiCompatibility()
		.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddCompatibilityRenderers(typeof(Xamarin.CommunityToolkit.Effects.TouchEffect).Assembly);
		})
		.ConfigureEffects(effects =>
		{
#if ANDROID || IOS
            effects.Add(typeof(TouchEffect), typeof(PlatformTouchEffect));
#endif
        });

        return builder.Build();
	}
}
