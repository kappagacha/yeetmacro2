using SkiaSharp;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Views;

public partial class PatternView : ContentView
{
    private SKPoint _lastTouchPoint;
    private SKColor _pickedColor;
    public static readonly BindableProperty PatternProperty =
            BindableProperty.Create("Pattern", typeof(Pattern), typeof(ImageView), null, propertyChanged: PatternPropertyChanged);
    private static void PatternPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        
    }

    public Pattern Pattern
    {
        get { return (Pattern)GetValue(PatternProperty); }
        set { SetValue(PatternProperty, value); }
    }

    public PatternView()
	{
		InitializeComponent();
	}

    private void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        if (Pattern == null || Pattern.ImageData == null) return;

        var imageData = Pattern.ImageData;

        // https://stackoverflow.com/questions/62143553/xamarin-import-skbitmap-from-local-file-not-resource
        // https://stackoverflow.com/questions/58828149/how-to-set-resize-quality-in-skiasharp
        var surface = e.Surface;
        var canvas = surface.Canvas;
        var imageInfo = e.Info;
        var bitmap = SKBitmap.Decode(imageData);
        int targetWidth, targetHeight;
        if (bitmap.Width >= bitmap.Height)
        {
            targetWidth = imageInfo.Width;
            targetHeight = Convert.ToInt32(bitmap.Height * targetWidth / (double)bitmap.Width);
        } 
        else
        {
            targetHeight = imageInfo.Height;
            targetWidth = Convert.ToInt32(bitmap.Width * targetHeight / (double)bitmap.Height);
        }

        // https://social.msdn.microsoft.com/Forums/en-US/851f6f9a-d762-405e-9c80-6356c576ccc8/how-can-i-scale-an-skbitmap-to-the-screen-size?forum=xamarinlibraries
        var resizedBitmap = bitmap.Resize(new SKImageInfo(targetWidth, targetHeight), SKFilterQuality.High);
        canvas.DrawBitmap(resizedBitmap, imageInfo.Width / 2 - resizedBitmap.Width / 2, imageInfo.Height / 2 - resizedBitmap.Height / 2);

        if (_lastTouchPoint == SKPoint.Empty) return;

        using (SKBitmap pointBitmap = new SKBitmap(imageInfo))
        {
            // get the pixel buffer for the bitmap
            IntPtr dstpixels = pointBitmap.GetPixels();

            // read the surface into the bitmap
            surface.ReadPixels(imageInfo,
                dstpixels,
                imageInfo.RowBytes,
                (int)_lastTouchPoint.X, (int)_lastTouchPoint.Y);

            // access the color
            var color = pointBitmap.GetPixel(0, 0);
            if (color != SKColor.Empty)
            {
                _pickedColor = color;
                colorPickCanvas.InvalidateSurface();
                System.Diagnostics.Debug.WriteLine(color);
                colorValue.Text = _pickedColor.ToString();
            }
        }
    }

    // https://github.com/UdaraAlwis/Xamarin-Playground/blob/master/XFColorPickerControl/XFColorPickerControl/Controls/ColorPickerControl.xaml.cs
    private void OnCanvasViewTouch(object sender, SkiaSharp.Views.Maui.SKTouchEventArgs e)
    {
        if (e.ActionType != SkiaSharp.Views.Maui.SKTouchAction.Pressed) return;

        _lastTouchPoint = e.Location;
        e.Handled = true;
        canvasView.InvalidateSurface();
    }

    private void OnColorPickPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        e.Surface.Canvas.Clear(_pickedColor);
    }
}