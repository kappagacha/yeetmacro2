using Microsoft.Extensions.Logging;
using YeetMacro2.Services;
using YeetMacro2.Platforms;
using CommunityToolkit.Maui;
using UraniumUI;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui.Markup;
using MauiIcons.FontAwesome.Brand;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;

#if ANDROID
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif

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
                // https://github.com/dotnet/maui/issues/13239
                fonts.AddFont("OpenSans-Medium.ttf", "sans-serif-medium");
                // https://enisn-projects.io/docs/en/uranium/latest/theming/Icons#fontawesome
                fonts.AddFontAwesomeIconFonts();
                // https://enisn-projects.io/docs/en/uranium/latest/theming/Icons#material-icons
                fonts.AddMaterialIconFonts();
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder
			.UseSkiaSharp();

        builder.UseMauiCommunityToolkit();
        builder.UseMauiCommunityToolkitMarkup();
        builder.RegisterViewModels();
		builder.RegisterServices();
		builder.RegisterPlatformServices();

		builder.UseFontAwesomeBrandMauiIcons();

#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("WhiteUnderline", (h, v) =>
        {
            // https://stackoverflow.com/questions/26574328/changing-edittext-bottom-line-color-with-appcompat-v7
            var drawable = h.PlatformView.Background;
            drawable.SetColorFilter(Colors.White.ToAndroid(), FilterMode.SrcAtop);
            h.PlatformView.SetBackground(drawable);
        });
#endif


        return builder.Build();
	}
}
