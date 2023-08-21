using System.Windows.Input;

#if ANDROID
using System.Collections.Concurrent;
using Android.Graphics.Drawables;
using static Android.Graphics.Bitmap;
using Microsoft.Maui.Graphics.Platform;
#endif

namespace YeetMacro2.Views;

public partial class ImageView : ContentView
{
    public static readonly BindableProperty ImageWidthProperty =
            BindableProperty.Create("ImageWidth", typeof(double?), typeof(ImageView), null);
    public static readonly BindableProperty ImageHeightProperty =
            BindableProperty.Create("ImageHeight", typeof(double?), typeof(ImageView), null);
    public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create("FontFamily", typeof(string), typeof(ImageView), null, propertyChanged: ImagePropertyChanged);
    public static readonly BindableProperty GlyphProperty =
            BindableProperty.Create("Glyph", typeof(string), typeof(ImageView), null, propertyChanged: ImagePropertyChanged);
    public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(Color), typeof(ImageView), null, propertyChanged: ImagePropertyChanged);
    public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create("ImageSource", typeof(ImageSource), typeof(ImageView), null);

#if ANDROID
    static ConcurrentDictionary<string, byte[]> _keyToImageBytes = new();
    static ConcurrentDictionary<string, ControlTemplate> _keyToControlTemplate = new();
#endif

    private async static void ImagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
#if ANDROID
        // This android workaround exists for the overlay windows not properly loading FontImageSource
        // unless spawned while YeetMacro app is active
        var imgView = bindable as ImageView;
        if (String.IsNullOrWhiteSpace(imgView.FontFamily) || String.IsNullOrWhiteSpace(imgView.Glyph)) return;
        
        var compositeKey = $"{imgView.FontFamily}-{(int)imgView.Glyph[0]}-{imgView.Color}";
        Console.WriteLine("compositeKey: " + compositeKey);
        if (!_keyToImageBytes.ContainsKey(compositeKey))
        {
            var ctx = new MauiContext(MauiApplication.Current.Services, MauiApplication.Context);
            var fontImageSource = new FontImageSource()
            {
                FontFamily = imgView.FontFamily,
                Glyph = imgView.Glyph,
                Color = imgView.Color
            };

            MemoryStream ms = new MemoryStream();
            var imageSourceResult = await fontImageSource.GetPlatformImageAsync(ctx);
            var bitmap = ((BitmapDrawable)imageSourceResult.Value).Bitmap;
            bitmap.Compress(CompressFormat.Png, 100, ms);
            bitmap.Dispose();
            ms.Position = 0;

            //var imageBytes = ms.ToArray();
            //_keyToImageBytes.TryAdd(compositeKey, imageBytes);
            //var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            //var template = new ControlTemplate(() => new Image() { Aspect = Aspect.Fill, Source = imageSource });
            var drawable = PlatformImage.FromStream(ms);
            var template = new ControlTemplate(() => new GraphicsView()
            {
                Drawable = drawable,
                InputTransparent = true
            });
            _keyToControlTemplate.TryAdd(compositeKey, template);
            //if (!_keyToImageBytes.TryAdd(compositeKey, imageBytes))
            //{
            //    Console.WriteLine("Fail: " + compositeKey);
            //}
        }

        imgView.contentView.ControlTemplate = _keyToControlTemplate[compositeKey];
        //imgView.contentView.IsVisible = false;
        //imgView.contentView.IsVisible = true;

        //var imageSource = ImageSource.FromStream(() => new MemoryStream(_keyToImageBytes[compositeKey]));
        //imgView.image.Source = imageSource;


        //var skImageView = new SkiaImageView.SKImageView() { Source = _keyToBitmap[compositeKey] };
        //imgView.contentView.Content = skImageView;


        // imageSource needs to be instantiated each time
        //var imageSource = ImageSource.FromStream(() => new MemoryStream(_keyToImageBytes[compositeKey]));
        //var image = new Image() { Aspect = Aspect.Fill, Source = imageSource };
        //imgView.contentView.Content = image;

        //var d = new Drawable();
        //imgView.contentView.Content = new GraphicsView() { Drawable = PlatformImage.FromStream( _keyToDrawable[compositeKey] };

        //_keyToImage.TryAdd(compositeKey, new Image() { Aspect = Aspect.Fill, Source = imageSource });

        //if (!_keyToControlTemplate.TryAdd(compositeKey, template))
        //{
        //    Console.WriteLine("Fail: " + compositeKey);
        //}

        //imgView.contentView.Content = _keyToImage[compositeKey];
        //imgView.contentView.Content = (View)_keyToControlTemplate[compositeKey].CreateContent();
        //imgView.contentView.ControlTemplate = _keyToControlTemplate[compositeKey];

        //imgView.contentView.Content = new Image() { Aspect = Aspect.Fill, Source = imageSource };

        //imgView.ImageSource = null;
        //imgView.ImageSource = imageSource;
        //imgView.ImageSource = null;
        //imgView.ImageSource = imageSource;
#endif
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
    public Color Color
    {
        get { return (Color)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }
    public double? ImageWidth
    {
        get { return (double?)GetValue(ImageWidthProperty); }
        set { SetValue(ImageWidthProperty, value); }
    }
    public double? ImageHeight
    {
        get { return (double?)GetValue(ImageHeightProperty); }
        set { SetValue(ImageHeightProperty, value); }
    }
    public ImageSource ImageSource
    {
        get { return (ImageSource)GetValue(ImageSourceProperty); }
        set 
        {
            SetValue(ImageSourceProperty, value);
            OnPropertyChanged();
        }
    }

    public ImageView()
	{
		InitializeComponent();
	}
}