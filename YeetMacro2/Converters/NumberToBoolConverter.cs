namespace YeetMacro2.Converters;
public class NumberToBoolConverter : IMarkupExtension, IValueConverter
{
    static NumberToBoolConverter _instance = new NumberToBoolConverter();

    public NumberToBoolConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        int val = System.Convert.ToInt32(value);
        return val != 0;
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