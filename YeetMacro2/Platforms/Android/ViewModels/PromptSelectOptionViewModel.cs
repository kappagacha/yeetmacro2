using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Platforms.Android.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class PromptSelectOptionViewModel : ObservableObject
{
    AndroidWindowManagerService _windowManagerService;
    [ObservableProperty]
    string _message = "Please select option";
    [ObservableProperty]
    IEnumerable<string> _options;
    [ObservableProperty]
    string _selectedOption;

    public PromptSelectOptionViewModel(AndroidWindowManagerService windowManagerService)
    {
        _windowManagerService = windowManagerService;
    }

    [RelayCommand]
    private void Select(string option)
    {
        SelectedOption = option;
        _windowManagerService.Close(WindowView.PromptSelectOptionView);
    }

    [RelayCommand]
    private void Cancel()
    {
        _windowManagerService.Cancel(WindowView.PromptSelectOptionView);
    }
}
