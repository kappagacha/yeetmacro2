using YeetMacro2.Platforms.Android.ViewModels;

namespace YeetMacro2.Platforms.Android.Views;

public partial class AndroidHomePage : ContentPage
{
	public AndroidHomePage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is AndriodHomeViewModel viewModel)
		{
			viewModel.AppearCommand.Execute(null);
		}
	}
}