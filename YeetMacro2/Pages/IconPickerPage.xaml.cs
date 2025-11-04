using YeetMacro2.ViewModels;

namespace YeetMacro2.Pages;

public partial class IconPickerPage : ContentPage
{
    public IconPickerPage(IconPickerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
