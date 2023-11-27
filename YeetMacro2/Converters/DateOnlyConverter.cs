namespace YeetMacro2.Converters;
public class DateOnlyConverter : IMarkupExtension, IValueConverter
{
    static DateOnlyConverter _instance = new DateOnlyConverter();

    public DateOnlyConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var dateOnly = (DateOnly)value;
        return dateOnly.ToDateTime(TimeOnly.MinValue);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var dateTime = (DateTime)value;
        return DateOnly.FromDateTime(dateTime);
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return _instance;
    }
}