using YeetMacro2.ViewModels;

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

    public static readonly BindableProperty MacroSetProperty =
        BindableProperty.Create(nameof(MacroSet), typeof(MacroSetViewModel), typeof(DailyNodeView), null, propertyChanged: MacroSet_Changed);

    public MacroSetViewModel MacroSet
    {
        get { return (MacroSetViewModel)GetValue(MacroSetProperty); }
        set { SetValue(MacroSetProperty, value); }
    }

    private static void MacroSet_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is MacroSetViewModel macroSet)
        {
            var dailyNodeView = bindable as DailyNodeView;
            dailyNodeView.BindingContext = macroSet;
        }
    }

    public DailyNodeView()
	{
		InitializeComponent();
	}
}