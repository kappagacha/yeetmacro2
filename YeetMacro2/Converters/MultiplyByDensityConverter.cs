using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeetMacro2.Converters;

public class MultiplyByDensityConverter : IMarkupExtension, IValueConverter
{
    static MultiplyByDensityConverter _instance = new MultiplyByDensityConverter();

    public MultiplyByDensityConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        double val = System.Convert.ToDouble(value);
        return val * (DeviceDisplay.MainDisplayInfo.Density == 0.0 ? 1.0 : DeviceDisplay.MainDisplayInfo.Density);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return _instance;
    }
}
