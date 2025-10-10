using YeetMacro2.Data.Models;
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
        _mainGrid = CreateMainGrid();
        Content = _mainGrid;
    }

    private Grid CreateMainGrid()
    {
        var grid = new Grid();

        // Main content grid
        var contentGrid = new Grid();
        contentGrid.SetBinding(Grid.IsVisibleProperty, new Binding(
            "NodeManager.ShowExport",
            source: this,
            converter: new InverseBoolConverter()));

        // Create the main VirtualListView
        _mainListView = CreateMainListView();
        contentGrid.Children.Add(_mainListView);

        // Create menu
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
                    new RowDefinition { Height = GridLength.Auto }
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
            grid.Add(subNodesGrid, 1, 1);

            return grid;
        });
    }

    private Grid CreateMenuGrid()
    {
        var menuGrid = new Grid
        {
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.End
        };
        menuGrid.SetBinding(Grid.BindingContextProperty, new Binding(".", source: this));
        menuGrid.SetBinding(Grid.IsVisibleProperty, new Binding("IsMenuVisible", source: this));

        var menuStack = new VerticalStackLayout();

        // Menu toggle button
        var menuToggle = new ToggleImageView
        {
            FontFamily = "FASolid",
            Glyph = Solid.Bars
        };
        menuToggle.ToggledColor = Application.Current.Resources["Primary"] as Color;

        // Menu items container
        var menuItemsStack = new VerticalStackLayout();
        menuItemsStack.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(
            "IsToggled",
            source: menuToggle));

        // Extra menu items placeholder
        _extraMenuItemsContentView = new ContentView();
        menuItemsStack.Children.Add(_extraMenuItemsContentView);

        // Add all menu items
        AddMenuItem(menuItemsStack, MaterialOutlined.Unfold_more_double, "NodeManager.ExpandAllCommand", 
            "NodeManager.IsList", true);
        AddMenuItem(menuItemsStack, MaterialOutlined.Unfold_less_double, "NodeManager.CollapseAllCommand",
            "NodeManager.IsList", true);
        
        // Move operations
        AddMenuItem(menuItemsStack, MaterialSharp.Vertical_align_top, "NodeManager.MoveNodeTopCommand",
            "NodeManager.SelectedNode", true, "NodeManager.SelectedNode");
        AddMenuItem(menuItemsStack, Solid.ArrowUp, "NodeManager.MoveNodeUpCommand",
            "NodeManager.SelectedNode", true, "NodeManager.SelectedNode", "FASolid");
        AddMenuItem(menuItemsStack, Solid.ArrowDown, "NodeManager.MoveNodeDownCommand",
            "NodeManager.SelectedNode", true, "NodeManager.SelectedNode", "FASolid");
        AddMenuItem(menuItemsStack, MaterialSharp.Vertical_align_bottom, "NodeManager.MoveNodeBottomCommand",
            "NodeManager.SelectedNode", true, "NodeManager.SelectedNode");
        
        // Edit operations
        AddMenuItem(menuItemsStack, Solid.Pencil, "NodeManager.RenameNodeCommand",
            "NodeManager.SelectedNode", true, "NodeManager.SelectedNode", "FASolid");
        
        // Copy/Paste operations
        var copyGrid = new Grid();
        copyGrid.SetBinding(Grid.IsVisibleProperty, new Binding(
            "NodeManager.HasCopyClipboard",
            converter: new InverseBoolConverter()));
        AddMenuItem(copyGrid, MaterialOutlined.Content_copy, "NodeManager.CopyNodeCommand",
            "NodeManager.SelectedNode", true, "NodeManager.SelectedNode");
        menuItemsStack.Children.Add(copyGrid);

        var pasteGrid = new Grid();
        pasteGrid.SetBinding(Grid.IsVisibleProperty, new Binding("NodeManager.HasCopyClipboard"));
        
        var pasteParent = CreateImageView(MaterialOutlined.Content_paste, "NodeManager.PasteNodeCommand");
        pasteParent.SetBinding(ImageView.IsVisibleProperty, new Binding("NodeManager.SelectedNode.IsParentNode"));
        pasteGrid.Children.Add(pasteParent);
        
        var pasteNormal = CreateImageView(MaterialOutlined.Content_paste, "NodeManager.PasteNodeCommand");
        pasteNormal.SetBinding(ImageView.IsVisibleProperty, new Binding(
            "NodeManager.SelectedNode",
            converter: new NullToBoolConverter { IsInverse = true }));
        pasteGrid.Children.Add(pasteNormal);
        
        menuItemsStack.Children.Add(pasteGrid);

        var clearCopyView = CreateImageView(MaterialOutlined.Content_paste_off, "NodeManager.ClearCopyNodeCommand");
        clearCopyView.SetBinding(ImageView.IsVisibleProperty, new Binding("NodeManager.HasCopyClipboard"));
        menuItemsStack.Children.Add(clearCopyView);

        // Delete
        var deleteView = CreateImageView(Solid.TrashCan, "NodeManager.DeleteNodeCommand", "FASolid");
        deleteView.Color = Colors.Red;
        deleteView.SetBinding(ImageView.IsVisibleProperty, new Binding(
            "NodeManager.SelectedNode",
            converter: new NullToBoolConverter { IsInverse = true }));
        deleteView.SetBinding(ImageView.CommandParameterProperty, new Binding("NodeManager.SelectedNode"));
        menuItemsStack.Children.Add(deleteView);
        
        // Add
        var addView = CreateImageView(Solid.Plus, "NodeManager.AddNodeCommand", "FASolid");
        addView.SetBinding(ImageView.IsVisibleProperty, new Binding("NodeManager.SelectedNode.IsParentNode"));
        menuItemsStack.Children.Add(addView);
        
        // Sort
        AddMenuItem(menuItemsStack, MaterialOutlined.Sort, "NodeManager.RefreshCollectionsCommand",
            "NodeManager.IsList", false);

        menuStack.Children.Add(menuItemsStack);
        menuStack.Children.Add(menuToggle);
        
        menuGrid.Children.Add(menuStack);
        return menuGrid;
    }

    private void AddMenuItem(Layout parent, string glyph, string commandPath, 
        string visibilityPath = null, bool inverseVisibility = false, 
        string commandParameterPath = null, string fontFamily = "MaterialOutlined")
    {
        var view = CreateImageView(glyph, commandPath, fontFamily);
        
        if (!string.IsNullOrEmpty(visibilityPath))
        {
            var converter = inverseVisibility ? new NullToBoolConverter { IsInverse = true } : new NullToBoolConverter { IsInverse = false };
            view.SetBinding(ImageView.IsVisibleProperty, new Binding(visibilityPath, converter: converter));
        }
        
        if (!string.IsNullOrEmpty(commandParameterPath))
        {
            view.SetBinding(ImageView.CommandParameterProperty, new Binding(commandParameterPath));
        }
        
        parent.Children.Add(view);
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