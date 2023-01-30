namespace YeetMacro2.Converters;
public class InverseBoolConverter : IMarkupExtension, IValueConverter
{
    static InverseBoolConverter _instance = new InverseBoolConverter();

    public InverseBoolConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool val = (bool)value;
        return !val;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool val = (bool)value;
        return !val;
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return _instance;
    }
}