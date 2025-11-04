using System.Collections.ObjectModel;
using System.Windows.Input;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Views;

public partial class TagSelector : ContentView
{
    public static readonly BindableProperty AvailableTagsProperty =
        BindableProperty.Create(nameof(AvailableTags), typeof(ObservableCollection<NodeTag>), typeof(TagSelector), null, propertyChanged: OnAvailableTagsChanged);

    public static readonly BindableProperty NodeTagsProperty =
        BindableProperty.Create(nameof(NodeTags), typeof(object), typeof(TagSelector), null, propertyChanged: OnNodeTagsChanged);

    public static readonly BindableProperty ToggleTagCommandProperty =
        BindableProperty.Create(nameof(ToggleTagCommand), typeof(ICommand), typeof(TagSelector), null);

    public static readonly BindableProperty IsVerticalProperty =
        BindableProperty.Create(nameof(IsVertical), typeof(bool), typeof(TagSelector), false);

    public static readonly BindableProperty TagChangedCommandProperty =
        BindableProperty.Create(nameof(TagChangedCommand), typeof(ICommand), typeof(TagSelector), null);

    private ObservableCollection<TagSelectorItem> _tagSelectorItems = new();
    private ObservableCollection<string> _boundObservableCollection;

    public ObservableCollection<NodeTag> AvailableTags
    {
        get => (ObservableCollection<NodeTag>)GetValue(AvailableTagsProperty);
        set => SetValue(AvailableTagsProperty, value);
    }

    public object NodeTags
    {
        get => GetValue(NodeTagsProperty);
        set => SetValue(NodeTagsProperty, value);
    }

    public ICommand ToggleTagCommand
    {
        get => (ICommand)GetValue(ToggleTagCommandProperty);
        set => SetValue(ToggleTagCommandProperty, value);
    }

    public bool IsVertical
    {
        get => (bool)GetValue(IsVerticalProperty);
        set => SetValue(IsVerticalProperty, value);
    }

    public ICommand TagChangedCommand
    {
        get => (ICommand)GetValue(TagChangedCommandProperty);
        set => SetValue(TagChangedCommandProperty, value);
    }

    public ObservableCollection<TagSelectorItem> TagSelectorItems => _tagSelectorItems;

    public TagSelector()
    {
        InitializeComponent();
        ToggleTagCommand = new Command<TagSelectorItem>(OnToggleTag);
    }

    private static void OnAvailableTagsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TagSelector selector && newValue is ObservableCollection<NodeTag> tags)
        {
            selector.UpdateAvailableTags(tags);
        }
    }

    private void UpdateAvailableTags(ObservableCollection<NodeTag> tags)
    {
        _tagSelectorItems.Clear();
        foreach (var tag in tags.OrderBy(t => t.Position))
        {
            _tagSelectorItems.Add(new TagSelectorItem { Tag = tag });
        }

        // Update selection state based on NodeTags type
        if (NodeTags is string[] arrayTags)
        {
            UpdateSelectionState(arrayTags);
        }
        else if (NodeTags is ObservableCollection<string> collectionTags)
        {
            UpdateSelectionState(collectionTags.ToArray());
        }
    }

    private static void OnNodeTagsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TagSelector selector)
        {
            // Unsubscribe from old ObservableCollection if it exists
            if (oldValue is ObservableCollection<string> oldCollection)
            {
                oldCollection.CollectionChanged -= selector.OnBoundCollectionChanged;
            }

            // Handle new value
            if (newValue is string[] nodeTags)
            {
                selector.UpdateSelectionState(nodeTags);
            }
            else if (newValue is ObservableCollection<string> newCollection)
            {
                selector._boundObservableCollection = newCollection;
                newCollection.CollectionChanged += selector.OnBoundCollectionChanged;
                selector.UpdateSelectionState(newCollection.ToArray());
            }
        }
    }

    private void OnBoundCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_boundObservableCollection != null)
        {
            UpdateSelectionState(_boundObservableCollection.ToArray());
        }
    }

    private void UpdateSelectionState(string[] nodeTags)
    {
        if (_tagSelectorItems == null) return;

        var tagSet = nodeTags?.ToHashSet() ?? new HashSet<string>();
        foreach (var item in _tagSelectorItems)
        {
            // Use tag name instead of FontFamily-Glyph
            var tagName = item.Tag.Name;
            item.IsSelected = tagSet.Contains(tagName);
        }
    }

    private void OnToggleTag(TagSelectorItem item)
    {
        if (item == null) return;

        item.IsSelected = !item.IsSelected;

        // Use tag name instead of FontFamily-Glyph
        var tagName = item.Tag.Name;

        if (_boundObservableCollection != null)
        {
            // Working with ObservableCollection
            if (item.IsSelected && !_boundObservableCollection.Contains(tagName))
            {
                _boundObservableCollection.Add(tagName);
            }
            else if (!item.IsSelected && _boundObservableCollection.Contains(tagName))
            {
                _boundObservableCollection.Remove(tagName);
            }
        }
        else
        {
            // Working with string array
            var currentTags = (NodeTags as string[])?.ToList() ?? new List<string>();

            if (item.IsSelected && !currentTags.Contains(tagName))
            {
                currentTags.Add(tagName);
            }
            else if (!item.IsSelected && currentTags.Contains(tagName))
            {
                currentTags.Remove(tagName);
            }

            NodeTags = currentTags.ToArray();
        }

        // Notify that tags have changed
        TagChangedCommand?.Execute(null);
    }
}

public class TagSelectorItem : BindableObject
{
    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(TagSelectorItem), false);

    public NodeTag Tag { get; set; }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
}
