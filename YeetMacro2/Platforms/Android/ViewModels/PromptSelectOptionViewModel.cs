using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Models;
using YeetMacro2.Platforms.Android.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class PromptSelectOptionViewModel(AndroidScreenService screenService) : ObservableObject
{
    readonly AndroidScreenService _screenService = screenService;
    [ObservableProperty]
    string _message = "Please select option";
    [ObservableProperty]
    object _options;
    [ObservableProperty]
    string _selectedOption;

    [RelayCommand]
    private void Select(string option)
    {
        SelectedOption = option;
        _screenService.Close(AndroidWindowView.PromptSelectOptionView);
    }

    [RelayCommand]
    private void Cancel()
    {
        _screenService.Cancel(AndroidWindowView.PromptSelectOptionView);
    }
}
