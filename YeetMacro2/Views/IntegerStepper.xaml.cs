namespace YeetMacro2.Views;

public partial class IntegerStepper : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(int), typeof(IntegerStepper), 0, BindingMode.TwoWay);
    public static readonly BindableProperty ShowIncrementProperty =
        BindableProperty.Create(nameof(ShowIncrement), typeof(bool), typeof(IntegerStepper), false);
    public static readonly BindableProperty IncrementProperty =
        BindableProperty.Create(nameof(Increment), typeof(int), typeof(IntegerStepper), 1, BindingMode.TwoWay);

    public int Value
    {
        get { return (int)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }
    public bool ShowIncrement
    {
        get { return (bool)GetValue(ShowIncrementProperty); }
        set { SetValue(ShowIncrementProperty, value); }
    }
    public int Increment
    {
        get { return (int)GetValue(IncrementProperty); }
        set { SetValue(IncrementProperty, value); }
    }

    public IntegerStepper()
	{
		InitializeComponent();
    }

    private void Increment_Clicked(object sender, EventArgs e)
    {
        Value += Increment;
    }

    private void Decrement_Clicked(object sender, EventArgs e)
    {
        Value -= Increment;
    }
}