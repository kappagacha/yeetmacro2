namespace YeetMacro2.Views;

public partial class DailyNodeView : ContentView
{
    public static readonly BindableProperty IsSubViewProperty =
        BindableProperty.Create(nameof(IsSubView), typeof(bool), typeof(DailyNodeView), false);

    public bool IsSubView
    {
        get { return (bool)GetValue(IsSubViewProperty); }
        set { SetValue(IsSubViewProperty, value); }
    }
    public DailyNodeView()
	{
		InitializeComponent();
	}
}