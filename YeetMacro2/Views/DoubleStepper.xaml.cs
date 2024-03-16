namespace YeetMacro2.Views;

public partial class DoubleStepper : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(DoubleStepper), 0d, BindingMode.TwoWay);
    public static readonly BindableProperty ShowIncrementProperty =
        BindableProperty.Create(nameof(ShowIncrement), typeof(bool), typeof(DoubleStepper), false);
    public static readonly BindableProperty IncrementProperty =
        BindableProperty.Create(nameof(Increment), typeof(double), typeof(DoubleStepper), 1d, BindingMode.TwoWay);

    public double Value
    {
        get { return (double)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }
    public bool ShowIncrement
    {
        get { return (bool)GetValue(ShowIncrementProperty); }
        set { SetValue(ShowIncrementProperty, value); }
    }
    public double Increment
    {
        get { return (double)GetValue(IncrementProperty); }
        set { SetValue(IncrementProperty, value); }
    }

    public DoubleStepper()
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