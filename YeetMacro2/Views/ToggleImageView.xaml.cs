using System.ComponentModel;
using System.Windows.Input;

namespace YeetMacro2.Views;

public partial class ToggleImageView : ContentView
{
    public static readonly BindableProperty IsToggledProperty =
            BindableProperty.Create("IsToggled", typeof(bool), typeof(ToggleImageView), false, BindingMode.TwoWay);
    public static readonly BindableProperty ImageWidthProperty =
            BindableProperty.Create("ImageWidth", typeof(double?), typeof(ToggleImageView), null);
    public static readonly BindableProperty ImageHeightProperty =
            BindableProperty.Create("ImageHeight", typeof(double?), typeof(ToggleImageView), null);
    public static readonly BindableProperty TextProperty =
            BindableProperty.Create("Text", typeof(string), typeof(ToggleImageView), null);
    public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create("FontSize", typeof(double), typeof(ToggleImageView), null);
    public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create("FontFamily", typeof(string), typeof(ToggleImageView), null);
    public static readonly BindableProperty GlyphProperty =
            BindableProperty.Create("Glyph", typeof(string), typeof(ToggleImageView), null);
    public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(Color), typeof(ToggleImageView), null);
    public static readonly BindableProperty ToggledFontFamilyProperty =
            BindableProperty.Create("ToggledFontFamily", typeof(string), typeof(ToggleImageView), null);
    public static readonly BindableProperty ToggledGlyphProperty =
            BindableProperty.Create("ToggledGlyph", typeof(string), typeof(ToggleImageView), null);
    public static readonly BindableProperty ToggledColorProperty =
            BindableProperty.Create("ToggledColor", typeof(Color), typeof(ToggleImageView), null);
    public static readonly BindableProperty UntoggledFontFamilyProperty =
            BindableProperty.Create("UntoggledFontFamily", typeof(string), typeof(ToggleImageView), null);
    public static readonly BindableProperty UntoggledGlyphProperty =
            BindableProperty.Create("UntoggledGlyph", typeof(string), typeof(ToggleImageView), null);
    public static readonly BindableProperty UntoggledColorProperty =
            BindableProperty.Create("UntoggledColor", typeof(Color), typeof(ToggleImageView), null);
    public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(ToggleImageView), null);
    public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create("CommandParameter", typeof(object), typeof(ToggleImageView), null);
    public static readonly BindableProperty IsToggledFromImageOnlyProperty =
            BindableProperty.Create("IsToggledFromImageOnly", typeof(bool), typeof(ToggleImageView), false);
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
    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }
    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get { return (double)GetValue(FontSizeProperty); }
        set { SetValue(FontSizeProperty, value); }
    }
    public bool IsToggled
    {
        get { return (bool)GetValue(IsToggledProperty); }
        set { SetValue(IsToggledProperty, value); }
    }
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
    
    public Color Color
    {
        get { return (Color)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }
    public string ToggledFontFamily
    {
        get { return (string)GetValue(ToggledFontFamilyProperty); }
        set { SetValue(ToggledFontFamilyProperty, value); }
    }
    public string ToggledGlyph
    {
        get { return (string)GetValue(ToggledGlyphProperty); }
        set { SetValue(ToggledGlyphProperty, value); }
    }

    public Color ToggledColor
    {
        get { return (Color)GetValue(ToggledColorProperty); }
        set { SetValue(ToggledColorProperty, value); }
    }
    public string UntoggledFontFamily
    {
        get { return (string)GetValue(UntoggledFontFamilyProperty); }
        set { SetValue(UntoggledFontFamilyProperty, value); }
    }
    public string UntoggledGlyph
    {
        get { return (string)GetValue(UntoggledGlyphProperty); }
        set { SetValue(UntoggledGlyphProperty, value); }
    }
    public Color UntoggledColor
    {
        get { return (Color)GetValue(UntoggledColorProperty); }
        set { SetValue(UntoggledColorProperty, value); }
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
    public bool IsToggledFromImageOnly
    {
        get { return (bool)GetValue(IsToggledFromImageOnlyProperty); }
        set { SetValue(IsToggledFromImageOnlyProperty, value); }
    }
    public ToggleImageView()
	{
		InitializeComponent();

        imageView.PropertyChanged += ImageView_PropertyChanged;
    }

    private void ImageView_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // xaml Binding/Trigger is not able handle null with propert theming color
        if (e.PropertyName == nameof(ImageView.Color) && imageView.Color is null)
        {
            label.TextColor = (Color)Application.Current.Resources[Application.Current.UserAppTheme == AppTheme.Dark ? "White" : "Black"];
        }
        else
        {
            label.TextColor = imageView.Color;
        }
    }

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        IsToggled = !IsToggled;
    }
}