using System.Windows.Input;
#if ANDROID
using YeetMacro2.Services;
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
#if ANDROID
    static ConcurrentDictionary<string, MemoryStream> _compositeKeyToImageStream = new();
#endif

    private static void ImagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var imgView = (ImageView)bindable;

        if (!String.IsNullOrWhiteSpace(imgView.FontFamily) && !String.IsNullOrWhiteSpace(imgView.Glyph))
        {
            var source = new FontImageSource() 
            {
                FontFamily = imgView.FontFamily,
                Glyph = imgView.Glyph
            };
#if WINDOWS
            imgView.image.Source = source;
#elif ANDROID
            // This android workaround exists for the overlay windows not properly loading FontImageSource
            // unless spawned from within YeetMacro app first
            var ctx = new MauiContext(ServiceHelper.Current);
            source.LoadImage(ctx, (drawable) =>
            {

                var compositeKey = $"{imgView.FontFamily}-{imgView.Glyph}";
                if (!_compositeKeyToImageStream.ContainsKey(compositeKey))
                {
                    var bitmap = ((BitmapDrawable)drawable.Value).Bitmap;
                    MemoryStream ms = new MemoryStream();
                    bitmap.Compress(CompressFormat.Png, 100, ms);
                    bitmap.Dispose();
                    _compositeKeyToImageStream.TryAdd(compositeKey, ms);
                }

                var resolvedImageStream = _compositeKeyToImageStream[compositeKey];
                resolvedImageStream.Position = 0;
                imgView.Dispatcher.Dispatch(() => imgView.image.Source = ImageSource.FromStream(() => resolvedImageStream));
            });
#endif
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