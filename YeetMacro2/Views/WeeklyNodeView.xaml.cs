using YeetMacro2.ViewModels;

namespace YeetMacro2.Views;

public partial class WeeklyNodeView : ContentView
{
    public static readonly BindableProperty IsSubViewProperty =
        BindableProperty.Create(nameof(IsSubView), typeof(bool), typeof(WeeklyNodeView), false);

    public bool IsSubView
    {
        get { return (bool)GetValue(IsSubViewProperty); }
        set { SetValue(IsSubViewProperty, value); }
    }

    public static readonly BindableProperty MacroSetProperty =
        BindableProperty.Create(nameof(MacroSet), typeof(MacroSetViewModel), typeof(WeeklyNodeView), null, propertyChanged: MacroSet_Changed);

    public MacroSetViewModel MacroSet
    {
        get { return (MacroSetViewModel)GetValue(MacroSetProperty); }
        set { SetValue(MacroSetProperty, value); }
    }

    private static void MacroSet_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is MacroSetViewModel macroSet)
        {
            var weeklyNodeView = bindable as WeeklyNodeView;
            weeklyNodeView.BindingContext = macroSet;
        }
    }

    public WeeklyNodeView()
	{
		InitializeComponent();
	}
}