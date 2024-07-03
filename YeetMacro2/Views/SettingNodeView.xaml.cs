namespace YeetMacro2.Views;

public partial class SettingNodeView : ContentView
{
    public static readonly BindableProperty IsSubViewProperty =
        BindableProperty.Create(nameof(IsSubView), typeof(bool), typeof(SettingNodeView), false);
    public bool IsSubView
    {
        get { return (bool)GetValue(IsSubViewProperty); }
        set { SetValue(IsSubViewProperty, value); }
    }
    public SettingNodeView()
    {
        InitializeComponent();
    }
}