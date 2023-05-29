using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class ActionMenuViewModel : ObservableObject
{
    ILogger _logger;
    IToastService _toastService;
    AndroidWindowManagerService _windowManagerService;
    YeetAccessibilityService _accessibilityService;
    MacroManagerViewModel _macroManagerViewModel;
    public Size CurrentResolution => new Size(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);

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
    public bool HasValidResolution => DeviceDisplay.MainDisplayInfo.Width == _macroManagerViewModel.SelectedMacroSet.Resolution.Width &&
        DeviceDisplay.MainDisplayInfo.Height == _macroManagerViewModel.SelectedMacroSet.Resolution.Height;

    public ActionMenuViewModel(ILogger<ActionViewModel> logger, IToastService toastService, AndroidWindowManagerService windowManagerService,
        YeetAccessibilityService accessibilityService, MacroManagerViewModel macroManagerViewModel)
    {
        _logger = logger;
        _toastService = toastService;
        _windowManagerService = windowManagerService;
        _accessibilityService = accessibilityService;
        _macroManagerViewModel = macroManagerViewModel;
    }

    [RelayCommand]
    public void ManagePatterns()
    {
        _toastService.Show("Manage Patterns");
        _windowManagerService.Show(AndroidWindowView.PatternsNodeView);
        _windowManagerService.Close(AndroidWindowView.ActionMenuView);
    }

    [RelayCommand]
    public void Configure()
    {
        _toastService.Show("Redirecting to YeetMacro...");
        _windowManagerService.LaunchYeetMacro();
        _windowManagerService.Close(AndroidWindowView.ActionMenuView);
    }

    [RelayCommand]
    public void OpenLog()
    {
        _toastService.Show("Opening Log...");
        _windowManagerService.Show(AndroidWindowView.StatusPanelView);
        _windowManagerService.Close(AndroidWindowView.ActionMenuView);
    }

    [RelayCommand]
    public void Exit()
    {
        _toastService.Show("Exiting...");
        _windowManagerService.StopProjectionService();
        _windowManagerService.Close(AndroidWindowView.ActionMenuView);
    }

    [RelayCommand]
    public async Task ScreenCapture()
    {
        _toastService.Show("ScreenCapture...");
        _logger.LogDebug("ScreenCapture");
        await _windowManagerService.ScreenCapture();
    }
}