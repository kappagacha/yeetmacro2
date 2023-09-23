using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Windows.ViewModels;

public partial class WindowsHomeViewModel : ObservableObject
{
    ILogger _logger;
    int count = 0;
    LogViewModel _logViewModel;
    public WindowsHomeViewModel(ILogger<WindowsHomeViewModel> logger, LogViewModel logViewModel)
    {
        _logger = logger;
        _logViewModel = logViewModel;
    }

    [RelayCommand]
    public void Test()
    {
        _logViewModel.LogException(new Exception("Test exception"));
        //_logger.LogInformation("{persistLogs}", true);
        //_logger.LogInformation("{macroSet} {script}", "something", "something2");
        //_logger.LogTrace((count++).ToString());
    }
}