using System.Windows.Input;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Views;

public partial class NodeView : ContentView
{
    ICommand _toggleIsMenuOpenCommand;
    public static readonly BindableProperty IsMenuVisibleProperty =
        BindableProperty.Create("IsMenuVisible", typeof(bool), typeof(NodeView), true);
    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(NodeView), null);
    public static readonly BindableProperty ExpanderTemplateProperty =
        BindableProperty.Create("ExpanderTemplate", typeof(DataTemplate), typeof(NodeView), null);
    // https://stackoverflow.com/questions/58022446/multiple-contentpresenters-in-one-controltemplate
    public static readonly BindableProperty ExtraMenuItemsDataTemplateProperty =
        BindableProperty.Create("ExtraMenuItemsDataTemplate", typeof(DataTemplate), typeof(NodeView), null, propertyChanged: ExtraMenuItemsDataTemplatePropertyChanged);
    public static readonly BindableProperty NodeManagerProperty =
        BindableProperty.Create("NodeManager", typeof(NodeManagerViewModel), typeof(NodeView), null);
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create("ItemsSource", typeof(object), typeof(NodeView), null);

    private static void ExtraMenuItemsDataTemplatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var nodeView = bindable as NodeView;
        // https://github.com/dotnet/maui/blob/main/src/Controls/src/Core/Shell/ShellContent.cs#L81
        nodeView.extraMenuItemsContentView.Content = (View)nodeView.ExtraMenuItemsDataTemplate.CreateContent();
    }
    public bool IsMenuVisible
    {
        get { return (bool)GetValue(IsMenuVisibleProperty); }
        set { SetValue(IsMenuVisibleProperty, value); }
    }
    public DataTemplate ExtraMenuItemsDataTemplate
    {
        get { return (DataTemplate)GetValue(ExtraMenuItemsDataTemplateProperty); }
        set { SetValue(ExtraMenuItemsDataTemplateProperty, value); }
    }

    public DataTemplate ItemTemplate
    {
        get { return (DataTemplate)GetValue(ItemTemplateProperty); }
        set { SetValue(ItemTemplateProperty, value); }
    }
    public DataTemplate ExpanderTemplate
    {
        get { return (DataTemplate)GetValue(ExpanderTemplateProperty); }
        set { SetValue(ExpanderTemplateProperty, value); }
    }
    public NodeManagerViewModel NodeManager
    {
        get { return (NodeManagerViewModel)GetValue(NodeManagerProperty); }
        set { SetValue(NodeManagerProperty, value); }
    }

    public object ItemsSource
    {
        get { return GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }

    public NodeView()
	{
		InitializeComponent();
	}

    private void ExportEditor_SelectAll(object sender, TappedEventArgs e)
    {
        if (exportEditor.Text == null) return;
        exportEditor.Focus();
        exportEditor.CursorPosition = 0;
        exportEditor.SelectionLength = exportEditor.Text.Length;

        Task.Run(async () => await Clipboard.SetTextAsync(exportEditor.Text));
    }
}