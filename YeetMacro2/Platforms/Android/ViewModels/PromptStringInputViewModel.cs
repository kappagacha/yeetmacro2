using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Platforms.Android.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class PromptStringInputViewModel : ObservableObject
{
    AndroidScreenService _screenService;
    [ObservableProperty]
    string _message = "Please input string";
    [ObservableProperty]
    string _input;

    public PromptStringInputViewModel(AndroidScreenService screenService)
    {
        _screenService = screenService;
    }

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
