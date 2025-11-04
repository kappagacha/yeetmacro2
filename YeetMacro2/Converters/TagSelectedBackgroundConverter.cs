using Microsoft.Maui.Controls.Xaml;

namespace YeetMacro2.Converters;

[AcceptEmptyServiceProvider]
public class TagSelectedBackgroundConverter : IMarkupExtension, IValueConverter
{
    static readonly TagSelectedBackgroundConverter _instance = new();

    public TagSelectedBackgroundConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool isSelected = value is bool b && b;

        // Return Primary color if selected, Transparent if not
        if (isSelected)
        {
            return Application.Current?.Resources["Primary"] as Color ?? Colors.Blue;
        }
        return Colors.Transparent;
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
