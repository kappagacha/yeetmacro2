using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Platforms.Android.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class PromptSelectOptionViewModel : ObservableObject
{
    AndroidScreenService _screenService;
    [ObservableProperty]
    string _message = "Please select option";
    [ObservableProperty]
    IEnumerable<string> _options;
    [ObservableProperty]
    string _selectedOption;

    public PromptSelectOptionViewModel(AndroidScreenService screenService)
    {
        _screenService = screenService;
    }

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
