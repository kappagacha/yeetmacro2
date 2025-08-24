using YeetMacro2.Platforms.Android.ViewModels;

namespace YeetMacro2.Platforms.Android.Views;

public partial class PromptSelectOption : ContentView
{
	public PromptSelectOption()
	{
		InitializeComponent();
	}
	
	// Typed alias for BindingContext to avoid binding warnings
	public PromptSelectOptionViewModel ViewModel => BindingContext as PromptSelectOptionViewModel;
}