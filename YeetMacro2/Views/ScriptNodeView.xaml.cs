namespace YeetMacro2.Views;

public partial class ScriptNodeView : ContentView
{
    public static readonly BindableProperty ShowExecuteButtonProperty =
        BindableProperty.Create("ShowExecuteButton", typeof(bool), typeof(ScriptNodeView), false);
    public bool ShowExecuteButton
    {
        get { return (bool)GetValue(ShowExecuteButtonProperty); }
        set { SetValue(ShowExecuteButtonProperty, value); }
    }
    public ScriptNodeView()
	{
		InitializeComponent();
	}
}