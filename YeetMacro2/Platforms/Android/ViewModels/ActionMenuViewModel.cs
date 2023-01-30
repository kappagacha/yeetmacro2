using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class ActionMenuViewModel : ObservableObject
{
    IToastService _toastService;
    AndroidWindowManagerService _windowManagerService;
    YeetAccessibilityService _accessibilityService;
    [ObservableProperty]
    bool _showLogView;

    public Resolution CurrentResolution => new Resolution()
    {
        Width = DeviceDisplay.MainDisplayInfo.Width,
        Height = DeviceDisplay.MainDisplayInfo.Height
    };

    public string OverlayArea
    {
        get {
            var topLeft = _windowManagerService.GetTopLeftByPackage();
            return $"x{topLeft.x}y{topLeft.y} w{_windowManagerService.OverlayWidth}h{_windowManagerService.OverlayHeight}";
        }
    }

    public string CurrentPackage => _accessibilityService.CurrentPackage;
    public string DisplayCutoutTop => _windowManagerService.DisplayCutoutTop.ToString();
    public bool HasCutoutTop => _windowManagerService.DisplayCutoutTop > 0;
    // TODO: Make this depend on current MacroSet
    public bool HasValidResolution => DeviceDisplay.MainDisplayInfo.Width == 1080 && DeviceDisplay.MainDisplayInfo.Height == 1920;

    public ActionMenuViewModel(IToastService toastService, AndroidWindowManagerService windowManagerService, YeetAccessibilityService accessibilityService)
    {
        _toastService = toastService;
        _windowManagerService = windowManagerService;
        _accessibilityService = accessibilityService;
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

    [RelayCommand]
    public async Task ScreenCapture()
    {
        _toastService.Show("ScreenCapture...");
        await _windowManagerService.ScreenCapture();
    }

    [RelayCommand]
    public void ToggleLogView()
    {
        if (ShowLogView)
        {
            _windowManagerService.Show(WindowView.LogView);
        }
        else
        {
            _windowManagerService.Close(WindowView.LogView);
        }
    }
}