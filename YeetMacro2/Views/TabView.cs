using Microsoft.Maui.Layouts;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace YeetMacro2.Views;

[ContentProperty(nameof(Items))]
public class TabView : Grid
{
    readonly ContentView _contentView;
    readonly ConcurrentDictionary<TabItem, View> _tabItemToView;

    public static readonly BindableProperty ItemsProperty =
        BindableProperty.Create(nameof(Items), typeof(ObservableCollection<TabItem>), typeof(TabView), new ObservableCollection<TabItem>());
    public static readonly BindableProperty SelectedTabItemProperty =
        BindableProperty.Create(nameof(SelectedTabItem), typeof(TabItem), typeof(TabView));

    public ObservableCollection<TabItem> Items
    {
        get { return (ObservableCollection<TabItem>)GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value); }
    }

    public TabItem SelectedTabItem
    {
        get { return (TabItem)GetValue(SelectedTabItemProperty); }
        set
        {
            var newTabItem = value;
            var currentTabItem = (TabItem)GetValue(SelectedTabItemProperty);
            if (currentTabItem == newTabItem) return;

            if (currentTabItem != null)
            {
                currentTabItem.IsSelected = false;
            }

            SetValue(SelectedTabItemProperty, newTabItem);
            newTabItem.IsSelected = true;

            if (!_tabItemToView.ContainsKey(newTabItem))
            {
                _tabItemToView.TryAdd(newTabItem, newTabItem.ContentDataTemplate is not null ? (View)newTabItem.ContentDataTemplate.CreateContent() : newTabItem.Content);
                newTabItem.InitCommand?.Execute(this);
            }
            _contentView.Content = _tabItemToView[newTabItem];
        }
    }

    public View Content => _contentView.Content;

    public TabView()
    {
        _tabItemToView = new ConcurrentDictionary<TabItem, View>();

        this.AddRowDefinition(new RowDefinition(34));
        this.AddRowDefinition(new RowDefinition(GridLength.Star));

        var headerContainer = new FlexLayout() { AlignContent = FlexAlignContent.Start, AlignItems = FlexAlignItems.Start, Wrap = FlexWrap.Wrap, JustifyContent = FlexJustify.SpaceEvenly };
        BindableLayout.SetItemsSource(headerContainer, Items);
        BindableLayout.SetItemTemplate(headerContainer, new DataTemplate(() =>
        {
            var grid = new Grid();
            grid.AddRowDefinition(new RowDefinition(30));
            grid.AddRowDefinition(new RowDefinition(4));

            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding(nameof(TabItem.Header)));
            label.FontAttributes = FontAttributes.Bold;
            Grid.SetRow(label, 0);
            grid.Add(label);

            var indicator = new BoxView
            {
                Color = (Color)App.Current.Resources["Primary"]
            };
            Grid.SetRow(indicator, 1);
            grid.Add(indicator);

            var buttonGrid = new Grid();
            var imageButton = new ImageButton
            {
                Margin = 0,
                Padding = 0,
                Command = new Command(() => SelectedTabItem = (TabItem)grid.BindingContext)
            };
            buttonGrid.Add(imageButton);
            Grid.SetRow(buttonGrid, 0);
            Grid.SetRowSpan(buttonGrid, 2);
            grid.Add(buttonGrid);

            var selectedTrigger = new DataTrigger(typeof(Grid))
            {
                Binding = new Binding(nameof(TabItem.IsSelected), BindingMode.OneWay),
                Value = true,
                EnterActions =
                {
                    new GenericTriggerAction<Grid>((grid) =>
                    {
                        label.TextColor = (Color)App.Current.Resources["Primary"];
                        indicator.IsVisible = true;
                    })
                }
            };
            var deSelectedTrigger = new DataTrigger(typeof(Grid))
            {
                Binding = new Binding(nameof(TabItem.IsSelected), BindingMode.OneWay),
                Value = false,
                EnterActions =
                {
                    new GenericTriggerAction<Grid>((grid) =>
                    {
                        label.TextColor = Colors.White;
                        indicator.IsVisible = false;
                    })
                }
            };
            grid.Triggers.Add(selectedTrigger);
            grid.Triggers.Add(deSelectedTrigger);

            return grid;
        }));
        Grid.SetRow(headerContainer, 0);
        this.Add(headerContainer);

        _contentView = new ContentView();
        Grid.SetRow(_contentView, 1);
        this.Add(_contentView);

        Items.CollectionChanged += Items_CollectionChanged;
        BindingContextChanged += TabView_BindingContextChanged;
    }

    private void TabView_BindingContextChanged(object sender, EventArgs e)
    {
        foreach (TabItem tabItem in Items)
        {
            tabItem.BindingContext = this.BindingContext;
        }
    }

    private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (SelectedTabItem is null && e.Action == NotifyCollectionChangedAction.Add && Items.Count > 0)
        {
            SelectedTabItem = Items[0];
        }
    }
}

[ContentProperty(nameof(Content))]
public partial class TabItem : BindableObject
{
    public static readonly BindableProperty HeaderProperty =
        BindableProperty.Create(nameof(Header), typeof(string), typeof(TabItem));
    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(TabItem));
    public static readonly BindableProperty InitCommandProperty =
        BindableProperty.Create(nameof(InitCommand), typeof(ICommand), typeof(TabItem));

    public string Header
    {
        get { return (string)GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }
    public bool IsSelected
    {
        get { return (bool)GetValue(IsSelectedProperty); }
        set { SetValue(IsSelectedProperty, value); }
    }
    public ICommand InitCommand
    {
        get { return (ICommand)GetValue(InitCommandProperty); }
        set { SetValue(InitCommandProperty, value); }
    }

    public DataTemplate ContentDataTemplate { get; set; }
    public View Content { get; set; }

    public TabItem()
    {

    }
}

// https://github.com/enisn/UraniumUI/blob/develop/src/UraniumUI/Triggers/GenericTriggerAction.cs
public class GenericTriggerAction<T>(Action<T> action) : TriggerAction<T>
    where T : BindableObject
{
    private readonly Action<T> action = action ?? throw new InvalidOperationException("An action must be defined to run the GenericTriggerAction");

    protected override void Invoke(T sender)
    {
        action(sender);
    }
}
