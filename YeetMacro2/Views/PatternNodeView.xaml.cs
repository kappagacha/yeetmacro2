using YeetMacro2.ViewModels;

namespace YeetMacro2.Views;

public partial class PatternNodeView : ContentView
{
    public static readonly BindableProperty MacroSetProperty =
            BindableProperty.Create(nameof(MacroSet), typeof(MacroSetViewModel), typeof(PatternNodeView), null);

    public MacroSetViewModel MacroSet
    {
        get { return (MacroSetViewModel)GetValue(MacroSetProperty); }
        set { SetValue(MacroSetProperty, value); }
    }

    public PatternNodeView()
	{
		InitializeComponent();
	}
}