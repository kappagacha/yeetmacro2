using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Platforms.Android.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class PromptStringInputViewModel : ObservableObject
{
    AndroidWindowManagerService _windowManagerService;
    [ObservableProperty]
    string _message = "Please input string";
    [ObservableProperty]
    string _input;

    public PromptStringInputViewModel(AndroidWindowManagerService windowManagerService)
    {
        _windowManagerService = windowManagerService;
    }

    [RelayCommand]
    private void Ok()
    {
        _windowManagerService.Close(AndroidWindowView.PromptStringInputView);
    }

    [RelayCommand]
    private void Cancel()
    {
        _windowManagerService.Cancel(AndroidWindowView.PromptStringInputView);
    }
}
