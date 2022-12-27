using System.Windows.Input;

namespace YeetMacro2.Controls;

public partial class CustomLabel : ContentView
{
    public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create("ImageSource", typeof(string), typeof(CustomLabel), null);
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create("Text", typeof(string), typeof(CustomLabel), null);
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create("Command", typeof(ICommand), typeof(CustomLabel), null);
    public static readonly BindableProperty LongPressCommandProperty =
        BindableProperty.Create("LongPressCommand", typeof(ICommand), typeof(CustomLabel), null);
    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create("CommandParameter", typeof(object), typeof(CustomLabel), null);
    public static readonly BindableProperty ImageColorProperty =
        BindableProperty.Create("ImageColor", typeof(Color), typeof(CustomLabel), Colors.Blue);
    public static readonly BindableProperty ImageWidthProperty =
        BindableProperty.Create("ImageWidth", typeof(double), typeof(CustomLabel), 20 * (DeviceDisplay.MainDisplayInfo.Density != 0.0 ? DeviceDisplay.MainDisplayInfo.Density : 1));
    public static readonly BindableProperty ImageHeightProperty =
        BindableProperty.Create("ImageHeight", typeof(double), typeof(CustomLabel), 20 * (DeviceDisplay.MainDisplayInfo.Density != 0.0 ? DeviceDisplay.MainDisplayInfo.Density : 1));

    public string ImageSource
    {
        get { return (string)GetValue(ImageSourceProperty); }
        set { SetValue(ImageSourceProperty, value); }
    }
    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }
    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    public ICommand LongPressCommand
    {
        get { return (ICommand)GetValue(LongPressCommandProperty); }
        set { SetValue(LongPressCommandProperty, value); }
    }
    public object CommandParameter
    {
        get { return GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty, value); }
    }
    public Color ImageColor
    {
        get { return (Color)GetValue(ImageColorProperty); }
        set { SetValue(ImageColorProperty, value); }
    }
    public double ImageWidth
    {
        get { return (double)GetValue(ImageWidthProperty); }
        set { SetValue(ImageWidthProperty, value); }
    }
    public double ImageHeight
    {
        get { return (double)GetValue(ImageHeightProperty); }
        set { SetValue(ImageHeightProperty, value); }
    }

    public CustomLabel()
	{
        InitializeComponent();
	}
}