namespace YeetMacro2.Views;

public partial class WeeklyNodeView : ContentView
{
    public static readonly BindableProperty IsSubViewProperty =
        BindableProperty.Create("IsSubView", typeof(bool), typeof(WeeklyNodeView), false);

    public bool IsSubView
    {
        get { return (bool)GetValue(IsSubViewProperty); }
        set { SetValue(IsSubViewProperty, value); }
    }
    public WeeklyNodeView()
	{
		InitializeComponent();
	}
}