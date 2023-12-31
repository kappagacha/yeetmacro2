using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
//using OneOf.Types;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Windows.ViewModels;

public partial class WindowsHomeViewModel : ObservableObject
{
    ILogger _logger;
    //int count = 0;
    LogViewModel _logViewModel;
    MacroManagerViewModel _macroManagerViewModel;
    public WindowsHomeViewModel(ILogger<WindowsHomeViewModel> logger, LogViewModel logViewModel, MacroManagerViewModel macroManagerViewModel)
    {
        _logger = logger;
        _logViewModel = logViewModel;
        _macroManagerViewModel = macroManagerViewModel;
    }

    [RelayCommand]
    public void Test()
    {
        //_logViewModel.LogException(new Exception("Test exception"));
        //_logger.LogInformation("{persistLogs}", true);
        //_logger.LogInformation("{macroSet} {script}", "something", "something2");
        //_logger.LogTrace((count++).ToString());
        //dynamic dict = new DynamicDataEntry(new Dictionary<string, object>() { { "test", "testvalue"} });
        //var x = dict.test;
        //dynamic patterns = _macroManagerViewModel.Patterns.Root;
        //var x = patterns.arena;
        //var z = patterns.arena.cpThreshold;
        //var ewer = patterns.lobby.town;

        dynamic settings = _macroManagerViewModel.Settings.Root;
        var x1 = settings.outings.target;
        var x2 = settings.outings.target.Value;
    }
}