namespace YeetMacro2.Views;

public partial class ToggleImageView : ContentView
{
    public static readonly BindableProperty IsToggledProperty =
            BindableProperty.Create("IsToggled", typeof(bool), typeof(ImageView), false);
    public static readonly BindableProperty ImageWidthProperty =
            BindableProperty.Create("ImageWidth", typeof(double?), typeof(ImageView), null);
    public static readonly BindableProperty ImageHeightProperty =
            BindableProperty.Create("ImageHeight", typeof(double?), typeof(ImageView), null);
    public static readonly BindableProperty TextProperty =
            BindableProperty.Create("Text", typeof(string), typeof(ImageView), null);
    public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create("FontFamily", typeof(string), typeof(ImageView), null);
    public static readonly BindableProperty GlyphProperty =
            BindableProperty.Create("Glyph", typeof(string), typeof(ImageView), null);
    public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(Color), typeof(ImageView), null);
    public static readonly BindableProperty ToggledFontFamilyProperty =
            BindableProperty.Create("ToggledFontFamily", typeof(string), typeof(ImageView), null);
    public static readonly BindableProperty ToggledGlyphProperty =
            BindableProperty.Create("ToggledGlyph", typeof(string), typeof(ImageView), null);
    public static readonly BindableProperty ToggledColorProperty =
            BindableProperty.Create("ToggledColor", typeof(Color), typeof(ImageView), null);
    public static readonly BindableProperty UntoggledFontFamilyProperty =
            BindableProperty.Create("UntoggledFontFamily", typeof(string), typeof(ImageView), null);
    public static readonly BindableProperty UntoggledGlyphProperty =
            BindableProperty.Create("UntoggledGlyph", typeof(string), typeof(ImageView), null);
    public static readonly BindableProperty UntoggledColorProperty =
            BindableProperty.Create("UntoggledColor", typeof(Color), typeof(ImageView), null);
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

    public ToggleImageView()
	{
		InitializeComponent();
    }

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        IsToggled = !IsToggled;
    }
}