using SkiaSharp;
using System.Windows.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.Views;

public partial class PatternView : ContentView
{
    private SKPoint _lastTouchPoint;
    private SKColor _pickedColor;
    private Byte[] _currentImageData;
    public static readonly BindableProperty PatternNodeProperty =
        BindableProperty.Create(nameof(PatternNode), typeof(PatternNode), typeof(ImageView));
    public static readonly BindableProperty PatternProperty =
            BindableProperty.Create(nameof(Pattern), typeof(Pattern), typeof(ImageView), null, propertyChanged: PatternPropertyChanged);
    public static readonly BindableProperty SavePatternCommandProperty =
        BindableProperty.Create(nameof(SavePatternCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty SelectPatternCommandProperty =
        BindableProperty.Create(nameof(SelectPatternCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty CapturePatternCommandProperty =
        BindableProperty.Create(nameof(CapturePatternCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty SetPatternBoundsCommandProperty =
        BindableProperty.Create(nameof(SetPatternBoundsCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty ClickPatternCommandProperty =
        BindableProperty.Create(nameof(ClickPatternCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty TestPatternCommandProperty =
        BindableProperty.Create(nameof(TestPatternCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty AddPatternCommandProperty =
        BindableProperty.Create(nameof(AddPatternCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty DeletePatternCommandProperty =
        BindableProperty.Create(nameof(DeletePatternCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty ApplyColorThresholdCommandProperty =
        BindableProperty.Create(nameof(ApplyColorThresholdCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty TestPatternTextMatchCommandProperty =
        BindableProperty.Create(nameof(TestPatternTextMatchCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty ApplyPatternTextMatchCommandProperty =
        BindableProperty.Create(nameof(ApplyPatternTextMatchCommand), typeof(ICommand), typeof(ImageView));
    public static readonly BindableProperty ApplyPatternOffsetCommandProperty =
        BindableProperty.Create(nameof(ApplyPatternOffsetCommand), typeof(ICommand), typeof(ImageView));


    private static void PatternPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var patternView = bindable as PatternView;
        var pattern = newValue as Pattern;
        patternView.UpdateCanvas(pattern);
    }

    private void UpdateCanvas(Pattern pattern)
    {
        if (pattern != null && !pattern.ColorThreshold.IsActive && pattern.ImageData != null)
        {
            colorThresholdVariancePct.Value = (int)pattern.ColorThreshold.VariancePct;
            colorThresholdColor.Text = pattern.ColorThreshold.Color;
            _currentImageData = pattern.ImageData;
        }
        else if (pattern != null && pattern.ColorThreshold.IsActive && pattern.ColorThreshold.ImageData != null)
        {
            colorThresholdVariancePct.Value = (int)pattern.ColorThreshold.VariancePct;
            colorThresholdColor.Text = pattern.ColorThreshold.Color;
            _currentImageData = pattern.ColorThreshold.ImageData;
        }
        else
        {
            _currentImageData = null;
        }
        canvasView.InvalidateSurface();
    }

    public PatternNode PatternNode
    {
        get { return (PatternNode)GetValue(PatternNodeProperty); }
        set { SetValue(PatternNodeProperty, value); }
    }
    public Pattern Pattern
    {
        get { return (Pattern)GetValue(PatternProperty); }
        set { SetValue(PatternProperty, value); }
    }
    public ICommand SavePatternCommand
    {
        get { return (ICommand)GetValue(SavePatternCommandProperty); }
        set { SetValue(SavePatternCommandProperty, value); }
    }
    public ICommand SelectPatternCommand
    {
        get { return (ICommand)GetValue(SelectPatternCommandProperty); }
        set { SetValue(SelectPatternCommandProperty, value); }
    }
    public ICommand CapturePatternCommand
    {
        get { return (ICommand)GetValue(CapturePatternCommandProperty); }
        set { SetValue(CapturePatternCommandProperty, value); }
    }
    public ICommand SetPatternBoundsCommand
    {
        get { return (ICommand)GetValue(SetPatternBoundsCommandProperty); }
        set { SetValue(SetPatternBoundsCommandProperty, value); }
    }
    public ICommand ClickPatternCommand
    {
        get { return (ICommand)GetValue(ClickPatternCommandProperty); }
        set { SetValue(ClickPatternCommandProperty, value); }
    }
    public ICommand TestPatternCommand
    {
        get { return (ICommand)GetValue(TestPatternCommandProperty); }
        set { SetValue(TestPatternCommandProperty, value); }
    }
    public ICommand AddPatternCommand
    {
        get { return (ICommand)GetValue(AddPatternCommandProperty); }
        set { SetValue(AddPatternCommandProperty, value); }
    }
    public ICommand DeletePatternCommand
    {
        get { return (ICommand)GetValue(DeletePatternCommandProperty); }
        set { SetValue(DeletePatternCommandProperty, value); }
    }
    public ICommand ApplyColorThresholdCommand
    {
        get { return (ICommand)GetValue(ApplyColorThresholdCommandProperty); }
        set { SetValue(ApplyColorThresholdCommandProperty, value); }
    }
    public ICommand TestPatternTextMatchCommand
    {
        get { return (ICommand)GetValue(TestPatternTextMatchCommandProperty); }
        set { SetValue(TestPatternTextMatchCommandProperty, value); }
    }
    public ICommand ApplyPatternTextMatchCommand
    {
        get { return (ICommand)GetValue(ApplyPatternTextMatchCommandProperty); }
        set { SetValue(ApplyPatternTextMatchCommandProperty, value); }
    }
    public ICommand ApplyPatternOffsetCommand
    {
        get { return (ICommand)GetValue(ApplyPatternOffsetCommandProperty); }
        set { SetValue(ApplyPatternOffsetCommandProperty, value); }
    }
    public PatternView()
    {
        InitializeComponent();
    }

    private void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        e.Surface.Canvas.Clear();
        if (_currentImageData == null || _currentImageData.Length == 0) return;

        // https://stackoverflow.com/questions/62143553/xamarin-import-skbitmap-from-local-file-not-resource
        // https://stackoverflow.com/questions/58828149/how-to-set-resize-quality-in-skiasharp
        var surface = e.Surface;
        var canvas = surface.Canvas;
        var imageInfo = e.Info;
        var bitmap = SKBitmap.Decode(_currentImageData);
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

        using SKBitmap pointBitmap = new(imageInfo);
        
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
            colorThresholdColor.Text = _pickedColor.ToString();
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

    private void ColorThresholdIsActive_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        UpdateCanvas(Pattern);
    }

    private async void OffsetCalcType_Clicked(object sender, EventArgs e)
    {
        var pattern = ((ImageButton)sender).BindingContext as Pattern;
        var options = Enum.GetValues<OffsetCalcType>().Select(oct => oct.ToString()).ToArray();
        var selectedOption = await ServiceHelper.GetService<IInputService>().SelectOption("Select option", options);
        if (!String.IsNullOrEmpty(selectedOption) && selectedOption != "cancel" && selectedOption != "ok")
        {
            pattern.OffsetCalcType = Enum.Parse<OffsetCalcType>(selectedOption);
            SavePatternCommand.Execute(new object[] { pattern, PatternNode });
        }
    }
}