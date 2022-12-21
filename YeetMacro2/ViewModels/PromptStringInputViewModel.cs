using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;
public partial class PromptStringInputViewModel : ObservableObject
{
    IWindowManagerService _windowManagerService;
    [ObservableProperty]
    string _message = "Please input string";
    [ObservableProperty]
    string _input;

    public PromptStringInputViewModel()
    {
    }

    public PromptStringInputViewModel(IWindowManagerService windowManagerService)
    {
        _windowManagerService = windowManagerService;
    }

    [RelayCommand]
    private void Ok()
    {
        _windowManagerService.Close(WindowView.PromptStringInputView);
    }

    [RelayCommand]
    private void Cancel()
    {
        _windowManagerService.Cancel(WindowView.PromptStringInputView);
    }
}
