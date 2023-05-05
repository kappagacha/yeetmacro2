using System.Windows.Input;

namespace YeetMacro2.Views;

public partial class NodeView : ContentView
{
    ICommand _toggleIsMenuOpenCommand;
    public static readonly BindableProperty IsMenuOpenProperty =
        BindableProperty.Create("IsMenuOpen", typeof(bool), typeof(NodeView), false);
    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(NodeView), null);
    // https://stackoverflow.com/questions/58022446/multiple-contentpresenters-in-one-controltemplate
    public static readonly BindableProperty ExtraMenuItemsDataTemplateProperty =
        BindableProperty.Create("ExtraMenuItemsDataTemplate", typeof(DataTemplate), typeof(NodeView), null, propertyChanged: ExtraMenuItemsDataTemplatePropertyChanged);
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create("ItemsSource", typeof(object), typeof(NodeView), null);

    private static void ExtraMenuItemsDataTemplatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var nodeView = bindable as NodeView;
        // https://github.com/dotnet/maui/blob/main/src/Controls/src/Core/Shell/ShellContent.cs#L81
        nodeView.extraMenuItemsContentView.Content = (View)nodeView.ExtraMenuItemsDataTemplate.CreateContent();
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

    public DataTemplate ItemTemplate
    {
        get { return (DataTemplate)GetValue(ItemTemplateProperty); }
        set { SetValue(ItemTemplateProperty, value); }
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
    }
}