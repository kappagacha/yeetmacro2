using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels;
using YeetMacro2.ViewModels.NodeViewModels;
using YeetMacro2.Converters;
using UraniumUI.Icons.FontAwesome;
using UraniumUI.Icons.MaterialSymbols;

namespace YeetMacro2.Views;

public class GenericNodeView<TNode, TNodeViewModel> : ContentView
    where TNode : Node
    where TNodeViewModel : NodeManagerViewModel
{
    private Editor _exportEditor;
    private ContentView _extraMenuItemsContentView;
    private Grid _mainGrid;
    private VirtualListView _mainListView;
    private ActivityIndicator _activityIndicator;

    public GenericNodeView()
    {
        _extraMenuItemsContentView = new ContentView();
        _mainGrid = CreateMainGrid();
        Content = _mainGrid;
    }

    private Grid CreateMainGrid()
    {
        var grid = new Grid();

        // Main content grid with just the ListView
        var contentGrid = new Grid();
        contentGrid.SetBinding(Grid.IsVisibleProperty, new Binding(
            "NodeManager.ShowExport",
            source: this,
            converter: new InverseBoolConverter()));

        // Create the main VirtualListView
        _mainListView = CreateMainListView();
        contentGrid.Children.Add(_mainListView);

        // Create menu - filter menu is now part of this
        var menuGrid = CreateMenuGrid();
        contentGrid.Children.Add(menuGrid);

        grid.Children.Add(contentGrid);

        // Export view
        var exportGrid = CreateExportGrid();
        grid.Children.Add(exportGrid);

        // Activity indicator
        _activityIndicator = new ActivityIndicator();
        _activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, new Binding(
            "NodeManager.IsInitialized",
            source: this,
            converter: new InverseBoolConverter()));
        grid.Children.Add(_activityIndicator);

        return grid;
    }

    private Grid CreateFilterMenuStack(object hideMenuToggle)
    {
        var filterMenuStack = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        // Filter menu items container
        var filterMenuItemsStack = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.End
        };

        // Container for tag selector with fallback message
        var filterContainer = new Grid();

        // Tag filter selector - vertical layout, single-select mode
        var filterTagSelector = new TagSelector
        {
            Padding = new Thickness(5),
            IsVertical = true,
            IsSingleSelect = true
        };

        // Bind directly to MacroSet.Tags
        filterTagSelector.SetBinding(TagSelector.AvailableTagsProperty, new Binding("MacroSet.Tags", source: this));
        filterTagSelector.SetBinding(TagSelector.NodeTagsProperty, new Binding("NodeManager.SelectedFilterTags", source: this));
        filterContainer.Children.Add(filterTagSelector);

        // Message when no tags are available
        var noTagsLabel = new Label
        {
            Text = "No tags available",
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Colors.Gray,
            FontSize = 10,
            Padding = new Thickness(5)
        };

        // Bind to MacroSet.Tags.Count
        var noTagsBinding = new Binding("MacroSet.Tags.Count", source: this);
        noTagsBinding.Converter = new Converters.NumberToBoolConverter { IsInverse = true };
        noTagsLabel.SetBinding(Label.IsVisibleProperty, noTagsBinding);

        filterContainer.Children.Add(noTagsLabel);

        filterMenuItemsStack.Children.Add(filterContainer);

        // Filter menu toggle button (just the icon)
        var filterToggleIcon = new ToggleImageView
        {
            FontFamily = "MaterialOutlined",
            Glyph = MaterialOutlined.Filter_alt
        };
        filterToggleIcon.ToggledColor = Application.Current.Resources["Primary"] as Color;

        // Bind filter menu items visibility to toggle
        filterMenuItemsStack.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(
            "IsToggled",
            source: filterToggleIcon));

        filterMenuStack.Add(filterMenuItemsStack, 0, 0);
        filterMenuStack.Add(filterToggleIcon, 0, 1);

        return filterMenuStack;
    }

    private Grid CreateTagAssignmentMenuStack(object hideMenuToggle)
    {
        var tagAssignmentMenuStack = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        // Tag assignment menu items container
        var tagAssignmentMenuItemsStack = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.End
        };

        // Container for tag selector with fallback message
        var tagAssignmentContainer = new Grid();

        // Tag selector - vertical layout for assigning tags to selected node
        var tagAssignmentSelector = new TagSelector
        {
            Padding = new Thickness(5),
            IsVertical = true
        };

        // Bind to MacroSet tags
        tagAssignmentSelector.SetBinding(TagSelector.AvailableTagsProperty, new Binding("MacroSet.Tags", source: this));

        // Bind to selected node's tags with TwoWay mode
        tagAssignmentSelector.SetBinding(TagSelector.NodeTagsProperty, new Binding("NodeManager.SelectedNode.Tags", source: this, mode: BindingMode.TwoWay));

        // Create command to save selected node when tags change
        tagAssignmentSelector.TagChangedCommand = new Command(() =>
        {
            dynamic nodeManager = NodeManager;
            if (nodeManager?.SelectedNode != null)
            {
                // Update the selected node's Tags property from the TagSelector
                nodeManager.SelectedNode.Tags = tagAssignmentSelector.NodeTags as string[];
                nodeManager.UpdateNodeCommand?.Execute(nodeManager.SelectedNode);
                // Refresh collections to update filter UI
                nodeManager.RefreshCollectionsCommand?.Execute(null);
            }
        });

        tagAssignmentContainer.Children.Add(tagAssignmentSelector);

        // Message when no tags are available
        var noTagsLabel = new Label
        {
            Text = "No tags available",
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Colors.Gray,
            FontSize = 10,
            Padding = new Thickness(5)
        };

        // Bind to MacroSet.Tags.Count
        var noTagsBinding = new Binding("MacroSet.Tags.Count", source: this);
        noTagsBinding.Converter = new Converters.NumberToBoolConverter { IsInverse = true };
        noTagsLabel.SetBinding(Label.IsVisibleProperty, noTagsBinding);

        tagAssignmentContainer.Children.Add(noTagsLabel);

        tagAssignmentMenuItemsStack.Children.Add(tagAssignmentContainer);

        // Tag assignment menu toggle button (tag icon)
        var tagAssignmentToggleIcon = new ToggleImageView
        {
            FontFamily = "MaterialOutlined",
            Glyph = MaterialOutlined.Sell  // Tag icon
        };
        tagAssignmentToggleIcon.ToggledColor = Application.Current.Resources["Primary"] as Color;

        // Bind tag assignment menu items visibility to toggle
        tagAssignmentMenuItemsStack.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(
            "IsToggled",
            source: tagAssignmentToggleIcon));

        tagAssignmentMenuStack.Add(tagAssignmentMenuItemsStack, 0, 0);
        tagAssignmentMenuStack.Add(tagAssignmentToggleIcon, 0, 1);

        // Bind tag assignment menu stack visibility to SelectedNode
        // Only show when a node is selected
        tagAssignmentMenuStack.SetBinding(Grid.IsVisibleProperty, new Binding(
            "NodeManager.SelectedNode", source: this, converter: new NullToBoolConverter { IsInverse = true }));

        return tagAssignmentMenuStack;
    }

    private VirtualListView CreateMainListView()
    {
        var listView = new VirtualListView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        // Bind ItemTemplateSelector (takes priority if set)
        listView.SetBinding(VirtualListView.ItemTemplateSelectorProperty, new Binding(
            "ItemTemplateSelector",
            source: this));
        
        // Bind ItemTemplate (only used if ItemTemplateSelector is null)
        var binding = new Binding("ItemTemplate", source: this);
        binding.TargetNullValue = GetDefaultItemTemplate();
        listView.SetBinding(VirtualListView.ItemTemplateProperty, binding);

        // Set up Adapter with triggers
        var itemsSourceNullTrigger = new DataTrigger(typeof(VirtualListView))
        {
            Binding = new Binding("ItemsSource", source: this, converter: new NullToBoolConverter()),
            Value = true
        };
        itemsSourceNullTrigger.Setters.Add(new Setter
        {
            Property = VirtualListView.AdapterProperty,
            Value = new Binding("NodeManager.Root.Nodes", source: this)
        });

        var itemsSourceNotNullTrigger = new DataTrigger(typeof(VirtualListView))
        {
            Binding = new Binding("ItemsSource", source: this, converter: new NullToBoolConverter()),
            Value = false
        };
        itemsSourceNotNullTrigger.Setters.Add(new Setter
        {
            Property = VirtualListView.AdapterProperty,
            Value = new Binding("ItemsSource", source: this)
        });

        listView.Triggers.Add(itemsSourceNullTrigger);
        listView.Triggers.Add(itemsSourceNotNullTrigger);

        return listView;
    }

    private DataTemplate GetDefaultItemTemplate()
    {
        var parentView = this;
        return new DataTemplate(() =>
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }  // Tags row
                },
                ColumnSpacing = 5
            };

            // Spacer
            var spacer = new Grid { WidthRequest = 15 };
            grid.Add(spacer, 0, 0);

            // Toggle arrow
            var toggleImage = new ToggleImageView
            {
                ImageHeight = 15,
                ImageWidth = 15,
                FontFamily = "MaterialOutlined",
                Glyph =  MaterialOutlined.Keyboard_arrow_right,
                ToggledGlyph = MaterialOutlined.Keyboard_arrow_down,
                IsToggledFromImageOnly = true,
                HeightRequest = 20
            };
            toggleImage.SetBinding(ToggleImageView.IsVisibleProperty, new Binding(
                "Nodes.Count",
                converter: new NumberToBoolConverter()));
            toggleImage.SetBinding(ToggleImageView.IsToggledProperty, new Binding("IsExpanded"));
            grid.Add(toggleImage, 0, 0);

            // Name label and selection
            var nameGrid = new Grid { HeightRequest = 20 };
            
            var nameLabel = new Label
            {
                TextColor = Application.Current.Resources["Primary"] as Color,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Colors.Transparent
            };
            nameLabel.SetBinding(Label.TextProperty, new Binding("Name"));
            
            var selectedTrigger = new DataTrigger(typeof(Label))
            {
                Binding = new Binding("IsSelected"),
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter
            {
                Property = Label.BackgroundColorProperty,
                Value = Colors.Blue
            });
            nameLabel.Triggers.Add(selectedTrigger);
            
            nameGrid.Children.Add(nameLabel);

            var selectButton = new ImageButton
            {
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };
            selectButton.SetBinding(ImageButton.CommandParameterProperty, new Binding("."));
            selectButton.SetBinding(ImageButton.CommandProperty, new Binding(
                "NodeManager.SelectNodeCommand",
                source: parentView));
            
            nameGrid.Children.Add(selectButton);
            grid.Add(nameGrid, 1, 0);

            // Sub-nodes list
            var subNodesGrid = new Grid();
            subNodesGrid.SetBinding(Grid.IsVisibleProperty, new Binding(
                "Nodes.Count",
                converter: new NumberToBoolConverter()));

            var subListView = new VirtualListView
            {
                ItemTemplate = GetDefaultItemTemplate()
            };
            subListView.SetBinding(VirtualListView.AdapterProperty, new Binding("Nodes"));
            subListView.SetBinding(VirtualListView.IsVisibleProperty, new Binding("IsExpanded"));
            subListView.SetBinding(VirtualListView.HeightRequestProperty, new Binding("NodesHeight"));

            subNodesGrid.Children.Add(subListView);
            grid.Add(subNodesGrid, 1, 2);

            // Add filter trigger (tags and name)
            var filterTrigger = new DataTrigger(typeof(Grid))
            {
                Binding = new MultiBinding
                {
                    Bindings =
                    {
                        new Binding("Tags"),
                        new Binding("NodeManager.SelectedFilterTags", source: parentView),
                        new Binding("NodeManager.NameFilter", source: parentView),
                        new Binding("Name")
                    },
                    Converter = new Converters.NodeFilterVisibilityConverter()
                },
                Value = false
            };
            filterTrigger.Setters.Add(new Setter
            {
                Property = Grid.IsVisibleProperty,
                Value = false
            });
            grid.Triggers.Add(filterTrigger);

            return grid;
        });
    }

    private Grid CreateMenuGrid()
    {
        var menuGrid = new Grid
        {
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.End,
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },  // Action buttons
                new RowDefinition { Height = GridLength.Auto }   // Toggle button
            }
        };
        menuGrid.SetBinding(Grid.BindingContextProperty, new Binding(".", source: this));
        menuGrid.SetBinding(Grid.IsVisibleProperty, new Binding("IsMenuVisible", source: this));

        // Vertical stack for action buttons (hidden by default)
        var buttonsStack = new VerticalStackLayout
        {
            Spacing = 5,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.End
        };

        // Create the toggle first so we can bind to it
        var menuToggle = new ToggleImageView
        {
            FontFamily = "FASolid",
            Glyph = Solid.Bars
        };
        menuToggle.ToggledColor = Application.Current.Resources["Primary"] as Color;

        // Bind visibility to the toggle's IsToggled property
        buttonsStack.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(
            "IsToggled",
            source: menuToggle));

        // Position actions button
        var positionButton = CreateImageView(MaterialOutlined.Open_with, "NodeManager.ShowPositionActionsCommand");
        buttonsStack.Add(positionButton);

        // Tag assignment button
        var tagButton = CreateImageView(MaterialOutlined.Sell, "NodeManager.ShowTagActionsCommand");
        tagButton.SetBinding(ImageView.CommandParameterProperty, new Binding("MacroSet", source: this));
        buttonsStack.Add(tagButton);

        // Filter by name button
        var nameFilterButton = CreateImageView(MaterialOutlined.Search, "NodeManager.FilterByNameCommand");
        buttonsStack.Add(nameFilterButton);

        // Filter button
        var filterButton = CreateImageView(MaterialOutlined.Filter_alt, "NodeManager.ShowFilterActionsCommand");
        filterButton.SetBinding(ImageView.CommandParameterProperty, new Binding("MacroSet", source: this));
        buttonsStack.Add(filterButton);

        // Edit actions button (using ellipsis icon)
        var editButton = CreateImageView(Solid.EllipsisVertical, "NodeManager.ShowEditActionsCommand", "FASolid");
        buttonsStack.Add(editButton);

        menuGrid.Add(buttonsStack, 0, 0);
        menuGrid.Add(menuToggle, 0, 1);

        return menuGrid;
    }

    private ImageView CreateImageView(string glyph, string commandPath, string fontFamily = "MaterialOutlined")
    {
        var view = new ImageView
        {
            FontFamily = fontFamily,
            Glyph = glyph
        };
        view.SetBinding(ImageView.CommandProperty, new Binding(commandPath));
        return view;
    }

    private Grid CreateExportGrid()
    {
        var grid = new Grid();
        grid.SetBinding(Grid.IsVisibleProperty, new Binding("NodeManager.ShowExport", source: this));

        _exportEditor = new Editor();
        _exportEditor.SetBinding(Editor.TextProperty, new Binding("NodeManager.ExportValue", source: this));
        grid.Children.Add(_exportEditor);

        var buttonStack = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start
        };

        var selectAllView = new ImageView
        {
            FontFamily = "MaterialOutlined",
            Glyph = MaterialOutlined.Select_all
        };
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += ExportEditor_SelectAll;
        selectAllView.GestureRecognizers.Add(tapGesture);
        buttonStack.Children.Add(selectAllView);

        var closeView = new ImageView
        {
            FontFamily = "MaterialOutlined",
            Glyph = MaterialOutlined.Close,
            Color = Colors.Red
        };
        closeView.SetBinding(ImageView.CommandProperty, new Binding("NodeManager.CloseExportCommand", source: this));
        buttonStack.Children.Add(closeView);

        grid.Children.Add(buttonStack);
        return grid;
    }

    private void ExportEditor_SelectAll(object sender, EventArgs e)
    {
        if (_exportEditor != null)
        {
            _exportEditor.Focus();
            _exportEditor.CursorPosition = 0;
            _exportEditor.SelectionLength = _exportEditor.Text?.Length ?? 0;
        }
    }

    // Bindable properties from original NodeView
    public static readonly BindableProperty NodeManagerProperty = BindableProperty.Create(
        nameof(NodeManager), typeof(TNodeViewModel), typeof(GenericNodeView<TNode, TNodeViewModel>), null);

    public TNodeViewModel NodeManager
    {
        get => (TNodeViewModel)GetValue(NodeManagerProperty);
        set => SetValue(NodeManagerProperty, value);
    }

    public static readonly BindableProperty MacroSetProperty = BindableProperty.Create(
        nameof(MacroSet), typeof(MacroSetViewModel), typeof(GenericNodeView<TNode, TNodeViewModel>), null);

    public MacroSetViewModel MacroSet
    {
        get => (MacroSetViewModel)GetValue(MacroSetProperty);
        set => SetValue(MacroSetProperty, value);
    }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(object), typeof(GenericNodeView<TNode, TNodeViewModel>), null);

    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(GenericNodeView<TNode, TNodeViewModel>), null);

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly BindableProperty ItemTemplateSelectorProperty = BindableProperty.Create(
        nameof(ItemTemplateSelector), typeof(VirtualListViewItemTemplateSelector), typeof(GenericNodeView<TNode, TNodeViewModel>), null,
        propertyChanged: OnItemTemplateSelectorChanged);

    public VirtualListViewItemTemplateSelector ItemTemplateSelector
    {
        get => (VirtualListViewItemTemplateSelector)GetValue(ItemTemplateSelectorProperty);
        set => SetValue(ItemTemplateSelectorProperty, value);
    }
    
    private static void OnItemTemplateSelectorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (GenericNodeView<TNode, TNodeViewModel>)bindable;
        if (newValue != null && view._mainListView != null)
        {
            // Clear ItemTemplate when ItemTemplateSelector is set
            view._mainListView.ItemTemplate = null;
            view._mainListView.ItemTemplateSelector = (VirtualListViewItemTemplateSelector)newValue;
        }
    }

    public static readonly BindableProperty IsMenuVisibleProperty = BindableProperty.Create(
        nameof(IsMenuVisible), typeof(bool), typeof(GenericNodeView<TNode, TNodeViewModel>), true);

    public bool IsMenuVisible
    {
        get => (bool)GetValue(IsMenuVisibleProperty);
        set => SetValue(IsMenuVisibleProperty, value);
    }

    public ContentView ExtraMenuItemsContentView => _extraMenuItemsContentView;

    public static readonly BindableProperty ExtraMenuItemsDataTemplateProperty = BindableProperty.Create(
        nameof(ExtraMenuItemsDataTemplate), typeof(DataTemplate), typeof(GenericNodeView<TNode, TNodeViewModel>), null,
        propertyChanged: OnExtraMenuItemsDataTemplateChanged);

    public DataTemplate ExtraMenuItemsDataTemplate
    {
        get => (DataTemplate)GetValue(ExtraMenuItemsDataTemplateProperty);
        set => SetValue(ExtraMenuItemsDataTemplateProperty, value);
    }

    private static void OnExtraMenuItemsDataTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (GenericNodeView<TNode, TNodeViewModel>)bindable;
        if (newValue is DataTemplate template)
        {
            var content = template.CreateContent();
            if (content is View contentView)
            {
                contentView.BindingContext = view;
                view._extraMenuItemsContentView.Content = contentView;
            }
        }
    }

    public static readonly BindableProperty ExpanderTemplateProperty = BindableProperty.Create(
        nameof(ExpanderTemplate), typeof(DataTemplate), typeof(GenericNodeView<TNode, TNodeViewModel>), null);

    public DataTemplate ExpanderTemplate
    {
        get => (DataTemplate)GetValue(ExpanderTemplateProperty);
        set => SetValue(ExpanderTemplateProperty, value);
    }
}