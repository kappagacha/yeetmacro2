using Microsoft.Extensions.Logging;
using YeetMacro2.Services;
using YeetMacro2.Platforms;
using CommunityToolkit.Maui;

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

        builder.UseMauiApp<App>();
        builder.UseMauiCommunityToolkit();
        builder.RegisterViewModels();
		builder.RegisterPlatformServices();

        return builder.Build();
	}
}
