using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Windows.ViewModels;

public partial class WindowsHomeViewModel(LogServiceViewModel LogServiceViewModel, MacroManagerViewModel macroManagerViewModel) : ObservableObject
{
    //int count = 0;
    readonly LogServiceViewModel _logServiceViewModel = LogServiceViewModel;
    readonly MacroManagerViewModel _macroManagerViewModel = macroManagerViewModel;

    [RelayCommand]
    public void Test()
    {
        _logServiceViewModel.LogException(new Exception("Test exception"));
    }
}