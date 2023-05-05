using Microsoft.Extensions.Logging;
using YeetMacro2.Services;
using YeetMacro2.Platforms;
using CommunityToolkit.Maui;
using UraniumUI;
using SkiaSharp.Views.Maui.Controls.Hosting;
using UraniumUI.Views;

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
                // https://enisn-projects.io/docs/en/uranium/latest/theming/Icons#fontawesome
                fonts.AddFontAwesomeIconFonts();
                // https://enisn-projects.io/docs/en/uranium/latest/theming/Icons#material-icons
                fonts.AddMaterialIconFonts();
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

        return builder.Build();
	}
}
