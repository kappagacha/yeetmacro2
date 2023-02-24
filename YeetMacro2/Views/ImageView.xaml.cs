using Android.Graphics.Drawables;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System.Reflection;
using System.Windows.Input;
using YeetMacro2.Services;
using static Android.Graphics.Bitmap;

namespace YeetMacro2.Views;

public partial class ImageView : ContentView
{
    public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create("FontFamily", typeof(string), typeof(ImageView), null, propertyChanged: ImagePropertyChanged);
    public static readonly BindableProperty GlyphProperty =
            BindableProperty.Create("Glyph", typeof(string), typeof(ImageView), null, propertyChanged: ImagePropertyChanged);
    private static void ImagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var imgView = (ImageView)bindable;
    //var a = Assembly.GetAssembly(typeof(UraniumUI.Icons.FontAwesome.Solid));
    //var x = a.GetManifestResourceNames();
    //foreach (var mrn in x)
    //{
    //    var stream = a.GetManifestResourceStream(mrn);
    //}
    //https://github.com/dotnet/maui/discussions/8154
    //https://github.com/dotnet/maui/blob/e7812b0f00ffebfe753cc8f35479aa7a4fbf6135/src/Core/src/Hosting/ImageSources/ImageSourcesMauiAppBuilderExtensions.cs#L14-L20
    //https://github.com/dotnet/maui/blob/e7812b0f00ffebfe753cc8f35479aa7a4fbf6135/src/Core/src/ImageSources/FontImageSourceService/FontImageSourceService.cs
    //latest lead
    //https://github.com/dotnet/maui/blob/e7812b0f00ffebfe753cc8f35479aa7a4fbf6135/src/Core/src/Fonts/FontManager.Android.cs#L118
        var svc = ServiceHelper.GetService<IImageSourceService<IFontImageSource>>;
        if (!String.IsNullOrWhiteSpace(imgView.FontFamily) && !String.IsNullOrWhiteSpace(imgView.Glyph))
        {
            
            var source = new FontImageSource() 
            {
                FontFamily = imgView.FontFamily,
                Glyph = imgView.Glyph
            };
            var ctx = new MauiContext(ServiceHelper.Current);

            source.LoadImage(ctx, (drawable) =>
            {
                var bitmap = ((BitmapDrawable)drawable.Value).Bitmap;
                MemoryStream ms = new MemoryStream();
                bitmap.Compress(CompressFormat.Jpeg, 100, ms);
                bitmap.Dispose();
                ms.Position = 0;
                //imgView.Dispatcher.Dispatch(() => imgView.image.Source = ImageSource.FromStream(() => ms));
                imgView.Dispatcher.Dispatch(() => imgView.image.Source = source);
            });
            //imgView.image.Source = source;
        }
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create("Command", typeof(ICommand), typeof(ImageView), null);
    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create("CommandParameter", typeof(object), typeof(ImageView), null);
    public string FontFamily
    {
        get { return (string)GetValue(FontFamilyProperty); }
        set { SetValue(FontFamilyProperty, value); }
    }
    public string Glyph
    {
        get { return (string)GetValue(GlyphProperty); }
        set { SetValue(GlyphProperty, value); }
    }
    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }
    public object CommandParameter
    {
        get { return GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty, value); }
    }

    public ImageView()
	{
		InitializeComponent();
	}
}