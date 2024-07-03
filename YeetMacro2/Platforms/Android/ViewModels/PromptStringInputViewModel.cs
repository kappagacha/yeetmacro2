using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Platforms.Android.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class PromptStringInputViewModel(AndroidScreenService screenService) : ObservableObject
{
    readonly AndroidScreenService _screenService = screenService;
    [ObservableProperty]
    string _message = "Please input string";
    [ObservableProperty]
    string _input;

    [RelayCommand]
    private void Ok()
    {
        _screenService.Close(AndroidWindowView.PromptStringInputView);
    }

    [RelayCommand]
    private void Cancel()
    {
        _screenService.Cancel(AndroidWindowView.PromptStringInputView);
    }
}
