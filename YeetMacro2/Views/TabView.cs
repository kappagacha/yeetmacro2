﻿using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace YeetMacro2.Views;

[ContentProperty("Items")]
public class TabView : Grid
{
    DataTemplateView _dataTemplateView;
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
            var currentTabItem = (TabItem)GetValue(SelectedTabItemProperty);
            if (currentTabItem != null)
            {
                currentTabItem.IsSelected = false;
            }
            SetValue(SelectedTabItemProperty, value);
            value.IsSelected = true;
            _dataTemplateView.DataTemplate = value.ContentDataTemplate;
        }
    }

    public DataTemplateView Content => _dataTemplateView;

    public TabView()
    {
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

            var indicator = new BoxView();
            Grid.SetRow(indicator, 1);
            grid.Add(indicator);

            var buttonGrid = new Grid();
            var imageButton = new ImageButton();
            imageButton.Margin = 0;
            imageButton.Padding = 0;
            imageButton.Command = new Command(() => SelectedTabItem = (TabItem)grid.BindingContext);
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
                        label.TextColor = indicator.Color = (Color)App.Current.Resources["Primary"];
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
                        label.TextColor = indicator.Color = null;
                    })
                }
            };
            grid.Triggers.Add(selectedTrigger);
            grid.Triggers.Add(deSelectedTrigger);

            return grid;
        }));
        Grid.SetRow(headerContainer, 0);
        this.Add(headerContainer);

        _dataTemplateView = new DataTemplateView();
        Grid.SetRow(_dataTemplateView, 1);
        this.Add(_dataTemplateView);

        Items.CollectionChanged += Items_CollectionChanged;
    }

    private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (SelectedTabItem is null && e.Action == NotifyCollectionChangedAction.Add && Items.Count > 0)
        {
            SelectedTabItem = Items[0];
        }
    }
}

//[ContentProperty(nameof(Content))]
public partial class TabItem : ObservableObject
{
    [ObservableProperty]
    string _header;
    [ObservableProperty]
    bool _isSelected;
    //public DataTemplate HeaderDataTemplate { get; set; }
    //public View Content { get; set; }
    public DataTemplate ContentDataTemplate { get; set; }

    public TabItem()
    {

    }
}

// https://github.com/enisn/UraniumUI/blob/develop/src/UraniumUI/Triggers/GenericTriggerAction.cs
public class GenericTriggerAction<T> : TriggerAction<T>
    where T : BindableObject
{
    private readonly Action<T> action;

    public GenericTriggerAction(Action<T> action)
    {
        this.action = action ?? throw new InvalidOperationException("An action must be defined to run the GenericTriggerAction");
    }

    protected override void Invoke(T sender)
    {
        action(sender);
    }
}