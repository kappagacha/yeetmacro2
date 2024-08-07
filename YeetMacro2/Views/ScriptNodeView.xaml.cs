namespace YeetMacro2.Views;

public partial class ScriptNodeView : ContentView
{
    public static readonly BindableProperty ShowExecuteButtonProperty =
        BindableProperty.Create(nameof(ShowExecuteButton), typeof(bool), typeof(ScriptNodeView), false);
    public bool ShowExecuteButton
    {
        get { return (bool)GetValue(ShowExecuteButtonProperty); }
        set { SetValue(ShowExecuteButtonProperty, value); }
    }
    public ScriptNodeView()
    {
        InitializeComponent();
    }

    private void ScriptEditor_SelectAll(object sender, EventArgs e)
    {
        if (scriptEditor.Text == null) return;
        scriptEditor.Focus();
        scriptEditor.CursorPosition = 0;
        scriptEditor.SelectionLength = scriptEditor.Text.Length;
    }
}