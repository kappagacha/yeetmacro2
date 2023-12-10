namespace YeetMacro2.Views;

public partial class DailyNodeView : ContentView
{
    public static readonly BindableProperty IsSubViewProperty =
        BindableProperty.Create("IsSubView", typeof(bool), typeof(SettingNodeView), false);

    public bool IsSubView
    {
        get { return (bool)GetValue(IsSubViewProperty); }
        set { SetValue(IsSubViewProperty, value); }
    }
    public DailyNodeView()
	{
		InitializeComponent();
	}

    private void DailyTemplateEditor_SelectAll(object sender, EventArgs e)
    {
        if (dailyTemplateEditor.Text == null) return;
        dailyTemplateEditor.Focus();
        dailyTemplateEditor.CursorPosition = 0;
        dailyTemplateEditor.SelectionLength = dailyTemplateEditor.Text.Length;
    }
}