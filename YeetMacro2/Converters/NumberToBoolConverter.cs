using Microsoft.Maui.Controls.Xaml;

namespace YeetMacro2.Converters;

[AcceptEmptyServiceProvider]
public class NumberToBoolConverter : IMarkupExtension, IValueConverter
{
    static readonly NumberToBoolConverter _instance = new();

    public bool IsInverse { get; set; }

    public NumberToBoolConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        int val = System.Convert.ToInt32(value);
        bool result = val != 0;
        return IsInverse ? !result : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool val = (bool)value;
        return val ? 1 : 0;
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return _instance;
    }
}