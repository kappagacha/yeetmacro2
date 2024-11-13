using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Views;

public partial class TodoNodeView : ContentView
{
    public static readonly BindableProperty TodosProperty =
        BindableProperty.Create(nameof(Todos), typeof(TodoNodeManagerViewModel), typeof(TodoNodeView), null);

    public TodoNodeManagerViewModel Todos
    {
        get { return (TodoNodeManagerViewModel)GetValue(TodosProperty); }
        set { SetValue(TodosProperty, value); }
    }

    public static readonly BindableProperty SubViewProperty =
        BindableProperty.Create(nameof(SubView), typeof(TodoJsonParentViewModel), typeof(TodoNodeView), null);
    public TodoJsonParentViewModel SubView
    {
        get { return (TodoJsonParentViewModel)GetValue(SubViewProperty); }
        set { SetValue(SubViewProperty, value); }
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