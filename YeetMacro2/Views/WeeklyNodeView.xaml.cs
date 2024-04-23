namespace YeetMacro2.Views;

public partial class WeeklyNodeView : ContentView
{
    public static readonly BindableProperty IsSubViewProperty =
        BindableProperty.Create("IsSubView", typeof(bool), typeof(SettingNodeView), false);

    public bool IsSubView
    {
        get { return (bool)GetValue(IsSubViewProperty); }
        set { SetValue(IsSubViewProperty, value); }
    }
    public WeeklyNodeView()
	{
		InitializeComponent();
	}

    private void TemplateEditor_SelectAll(object sender, EventArgs e)
    {
        if (templateEditor.Text == null) return;
        templateEditor.Focus();
        templateEditor.CursorPosition = 0;
        templateEditor.SelectionLength = templateEditor.Text.Length;
    }
}