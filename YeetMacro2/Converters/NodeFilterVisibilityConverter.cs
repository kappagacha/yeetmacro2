using Microsoft.Maui.Controls.Xaml;

namespace YeetMacro2.Converters;

[AcceptEmptyServiceProvider]
public class NodeFilterVisibilityConverter : IMarkupExtension, IMultiValueConverter
{
    static readonly NodeFilterVisibilityConverter _instance = new();

    public NodeFilterVisibilityConverter()
    {
    }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // values[0]: Node.Tags (string[])
        // values[1]: NodeManager.SelectedFilterTags (ObservableCollection<string>)
        // values[2]: NodeManager.NameFilter (string)
        // values[3]: Node.Name (string)
        if (values == null || values.Length < 4)
            return true; // Show by default

        var nodeTags = values[0] as string[];
        var filterTags = values[1] as System.Collections.IEnumerable;
        var nameFilter = values[2] as string;
        var nodeName = values[3] as string;

        var hasTagFilter = false;
        var hasNameFilter = !string.IsNullOrWhiteSpace(nameFilter);
        var tagMatch = true;
        var nameMatch = true;

        // Check tag filter
        if (filterTags != null)
        {
            var filterTagsList = filterTags.Cast<string>().ToList();
            if (filterTagsList.Count > 0)
            {
                hasTagFilter = true;
                // If node has no tags and filter is active, hide it
                if (nodeTags == null || nodeTags.Length == 0)
                {
                    tagMatch = false;
                }
                else
                {
                    // Check if node has any of the filter tags
                    tagMatch = false;
                    foreach (var nodeTag in nodeTags)
                    {
                        if (filterTagsList.Contains(nodeTag))
                        {
                            tagMatch = true;
                            break;
                        }
                    }
                }
            }
        }

        // Check name filter
        if (hasNameFilter)
        {
            if (string.IsNullOrWhiteSpace(nodeName) || !nodeName.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
            {
                nameMatch = false;
            }
        }

        // If no filters are active, show all nodes
        if (!hasTagFilter && !hasNameFilter)
            return true;

        // Show if both tag and name filters match (if they are active)
        return tagMatch && nameMatch;
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
