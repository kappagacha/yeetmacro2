using Microsoft.Maui.Controls.Xaml;

namespace YeetMacro2.Converters;

[AcceptEmptyServiceProvider]
public class HasAnyTagConverter : IMarkupExtension, IMultiValueConverter
{
    static readonly HasAnyTagConverter _instance = new();

    public HasAnyTagConverter()
    {
    }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // values[0]: Node.Tags (string[])
        // values[1]: SelectedFilterTags (ObservableCollection<string> or List<string>)

        if (values == null || values.Length < 2)
            return true; // Show by default if no filter

        var nodeTags = values[0] as string[];
        var filterTags = values[1] as System.Collections.IEnumerable;

        // If no filter tags selected, show all nodes
        if (filterTags == null)
            return true;

        var filterTagsList = filterTags.Cast<string>().ToList();
        if (filterTagsList.Count == 0)
            return true;

        // If node has no tags and filter is active, hide it
        if (nodeTags == null || nodeTags.Length == 0)
            return false;

        // Check if node has any of the filter tags
        foreach (var nodeTag in nodeTags)
        {
            if (filterTagsList.Contains(nodeTag))
                return true;
        }

        return false;
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
