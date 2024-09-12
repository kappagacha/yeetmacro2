using YeetMacro2.ViewModels;

namespace YeetMacro2.Views;

public partial class PatternNodeView : ContentView
{
    public static readonly BindableProperty MacroSetProperty =
            BindableProperty.Create(nameof(MacroSet), typeof(MacroSetViewModel), typeof(PatternNodeView), null, propertyChanged: MacroSet_Changed);

    public MacroSetViewModel MacroSet
    {
        get { return (MacroSetViewModel)GetValue(MacroSetProperty); }
        set { SetValue(MacroSetProperty, value); }
    }

    private static void MacroSet_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is MacroSetViewModel macroSet)
        {
            var patternNodeView = bindable as PatternNodeView;
            patternNodeView.BindingContext = macroSet;
        }
    }

    public PatternNodeView()
	{
		InitializeComponent();
	}
}