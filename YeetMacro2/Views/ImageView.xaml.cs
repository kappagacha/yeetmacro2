using System.Windows.Input;
using SkiaSharp;
#if ANDROID
using System.Collections.Concurrent;
using Android.Graphics.Drawables;
using static Android.Graphics.Bitmap;
#elif WINDOWS
#endif

namespace YeetMacro2.Views;

public partial class ImageView : ContentView
{
    public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create("FontFamily", typeof(string), typeof(ImageView), null, propertyChanged: ImagePropertyChanged);
    public static readonly BindableProperty GlyphProperty =
            BindableProperty.Create("Glyph", typeof(string), typeof(ImageView), null, propertyChanged: ImagePropertyChanged);
    public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(Color), typeof(ImageView), null, propertyChanged: ImagePropertyChanged);
#if ANDROID
    static ConcurrentDictionary<string, SKBitmap> _keyToBitmap = new();
#endif

    private static void ImagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var imgView = (ImageView)bindable;

        if (!String.IsNullOrWhiteSpace(imgView.FontFamily) && !String.IsNullOrWhiteSpace(imgView.Glyph))
        {
            imgView.canvasView.InvalidateSurface();
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
    public Color Color
    {
        get { return (Color)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }
    public ImageView()
	{
		InitializeComponent();
	}

    // This android workaround exists for the overlay windows not properly loading FontImageSource
    // unless spawned while YeetMacro app is active
    private async void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        if (String.IsNullOrWhiteSpace(FontFamily) || String.IsNullOrWhiteSpace(Glyph)) return;

#if ANDROID
        var compositeKey = $"{FontFamily}-{Glyph}-{Color}";

        if (_keyToBitmap.ContainsKey(compositeKey))
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;
            var skBitmap = _keyToBitmap[compositeKey];
            canvas.Clear();
            canvas.DrawBitmap(skBitmap, info.Rect);
        } 
        else // if bitmap not found, generate then invalidate canvas
        {
            var ctx = new MauiContext(MauiApplication.Current.Services, MauiApplication.Context);
            var fontImageSource = new FontImageSource()
            {
                FontFamily = FontFamily,
                Glyph = Glyph,
                Color = Color
            };
            var drawable = await fontImageSource.GetPlatformImageAsync(ctx);
            var bitmap = ((BitmapDrawable)drawable.Value).Bitmap;
            MemoryStream ms = new MemoryStream();
            bitmap.Compress(CompressFormat.Png, 100, ms);
            bitmap.Dispose();
            ms.Position = 0;
            _keyToBitmap.TryAdd(compositeKey, SKBitmap.Decode(ms));
            canvasView.InvalidateSurface();
        }
#endif
    }
}