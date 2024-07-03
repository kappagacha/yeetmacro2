using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Views;

public partial class TodoNodeView : ContentView
{
    public static readonly BindableProperty TodosProperty =
        BindableProperty.Create(nameof(Todos), typeof(TodoNodeManagerViewModel), typeof(TodoNodeView), null);

    public static readonly BindableProperty IsSubViewProperty =
        BindableProperty.Create(nameof(IsSubView), typeof(bool), typeof(TodoNodeView), false);

    public TodoNodeManagerViewModel Todos
    {
        get { return (TodoNodeManagerViewModel)GetValue(TodosProperty); }
        set { SetValue(TodosProperty, value); }
    }
    public bool IsSubView
    {
        get { return (bool)GetValue(IsSubViewProperty); }
        set { SetValue(IsSubViewProperty, value); }
    }
    public TodoNodeView()
    {
        InitializeComponent();
    }

    private void JsonText_SelectAll(object sender, EventArgs e)
    {
        if (dataEditor.Text == null) return;
        dataEditor.Focus();
        dataEditor.CursorPosition = 0;
        dataEditor.SelectionLength = dataEditor.Text.Length;
    }
}