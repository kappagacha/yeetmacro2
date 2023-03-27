using System.Globalization;

namespace YeetMacro2.Converters;
public class PassThroughConverter : IMarkupExtension, IMultiValueConverter
{
    static PassThroughConverter _instance = new PassThroughConverter();

    public PassThroughConverter()
    {
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return _instance;
    }
}