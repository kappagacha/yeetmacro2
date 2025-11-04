using Microsoft.Maui.Controls.Xaml;

namespace YeetMacro2.Converters;

[AcceptEmptyServiceProvider]
public class AllTrueConverter : IMarkupExtension, IMultiValueConverter
{
    static readonly AllTrueConverter _instance = new();

    public AllTrueConverter()
    {
    }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (values == null || values.Length == 0)
            return false;

        foreach (var value in values)
        {
            if (value is bool boolValue && !boolValue)
                return false;
        }

        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return _instance;
    }
}
