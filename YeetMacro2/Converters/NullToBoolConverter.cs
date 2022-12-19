using System.Collections.Concurrent;

namespace YeetMacro2.Converters;

public class NullToBoolConverter : IMarkupExtension, IValueConverter
{
    //Key pattern should be a cartesian product of all available public properties
    static ConcurrentDictionary<String, NullToBoolConverter> _converters = new ConcurrentDictionary<string, NullToBoolConverter>();

    public bool IsInverse { get; set; }
    public NullToBoolConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool val = value == null;
        return IsInverse ? !val : val;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        var anonKey = new { IsInverse = IsInverse };
        String key = anonKey.ToString();
        if (!_converters.ContainsKey(key))
        {
            _converters.TryAdd(key, this);
        }
        return _converters[key];
    }
}
