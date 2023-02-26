using System.Windows.Input;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Views;

public partial class TreeView : ContentView
{
    ICommand _toggleIsMenuOpenCommand;
    public static readonly BindableProperty IsMenuOpenProperty =
        BindableProperty.Create("IsMenuOpen", typeof(bool), typeof(TreeView), false);
    // https://stackoverflow.com/questions/58022446/multiple-contentpresenters-in-one-controltemplate
    public static readonly BindableProperty ExtraMenuItemsDataTemplateProperty =
        BindableProperty.Create("ExtraMenuItemsDataTemplate", typeof(DataTemplate), typeof(TreeView), null, propertyChanged: ExtraMenuItemsDataTemplatePropertyChanged);
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create("ItemsSource", typeof(object), typeof(TreeView), null);
    private static void ExtraMenuItemsDataTemplatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var treeView = bindable as TreeView;
        // https://github.com/dotnet/maui/blob/main/src/Controls/src/Core/Shell/ShellContent.cs#L81
        treeView.extraMenuItemsContentView.Content = (View)treeView.ExtraMenuItemsDataTemplate.CreateContent();
    }

    public bool IsMenuOpen
    {
        get { return (bool)GetValue(IsMenuOpenProperty); }
        set { SetValue(IsMenuOpenProperty, value); }
    }
    public DataTemplate ExtraMenuItemsDataTemplate
    {
        get { return (DataTemplate)GetValue(ExtraMenuItemsDataTemplateProperty); }
        set { SetValue(ExtraMenuItemsDataTemplateProperty, value); }
    }
    public object ItemsSource
    {
        get { return GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }
    public ICommand ToggleIsMenuOpenCommand
    {
        get => _toggleIsMenuOpenCommand ?? (_toggleIsMenuOpenCommand = new Command(() => IsMenuOpen = !IsMenuOpen));
    }

    public TreeView()
	{
		InitializeComponent();
	}
}