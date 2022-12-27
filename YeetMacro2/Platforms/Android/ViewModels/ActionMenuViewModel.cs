using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class ActionMenuViewModel : ObservableObject
{
    IToastService _toastService;
    IWindowManagerService _windowManagerService;

    public ActionMenuViewModel(IToastService toastService, IWindowManagerService windowManagerService)
    {
        _toastService = toastService;
        _windowManagerService = windowManagerService;
    }

    [RelayCommand]
    public void ManagePatterns()
    {
        _toastService.Show("Manage Patterns");
        _windowManagerService.Show(WindowView.PatternsTreeView);
        _windowManagerService.Close(WindowView.ActionMenuView);
    }

    [RelayCommand]
    public void Configure()
    {
        _toastService.Show("Redirecting to YeetMacro...");
        _windowManagerService.LaunchYeetMacro();
        _windowManagerService.Close(WindowView.ActionMenuView);
    }

    [RelayCommand]
    public void OpenLog()
    {
        _toastService.Show("Opening Log...");
        _windowManagerService.Show(WindowView.LogView);
        _windowManagerService.Close(WindowView.ActionMenuView);
    }

    [RelayCommand]
    public void Exit()
    {
        _toastService.Show("Exiting...");
        _windowManagerService.StopProjectionService();
        _windowManagerService.Close(WindowView.ActionMenuView);
    }
}