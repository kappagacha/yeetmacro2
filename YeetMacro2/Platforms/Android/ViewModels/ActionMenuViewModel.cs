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
    AndroidScreenService _screenService;
    YeetAccessibilityService _accessibilityService;
    MacroManagerViewModel _macroManagerViewModel;
    public Size CurrentResolution => new Size(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);

    public string OverlayArea
    {
        get {
            var topLeft = _screenService.GetTopLeft();
            var currentResolution = _screenService.CalcResolution;
            return $"x{topLeft.X}y{topLeft.Y} w{currentResolution.Width}h{currentResolution.Height}";
        }
    }

    public string CurrentPackage => _accessibilityService.CurrentPackage;
    //public string DisplayCutoutTop => _windowManagerService.DisplayCutoutTop.ToString();
    //public bool HasCutoutTop => _windowManagerService.DisplayCutoutTop > 0;
    public bool HasValidResolution => DeviceDisplay.MainDisplayInfo.Width == (_macroManagerViewModel.SelectedMacroSet?.Resolution.Width ?? -1.0) &&
        DeviceDisplay.MainDisplayInfo.Height == (_macroManagerViewModel.SelectedMacroSet?.Resolution.Height ?? -1.0);

    public ActionMenuViewModel(ILogger<ActionViewModel> logger, IToastService toastService, AndroidScreenService screenService,
         YeetAccessibilityService accessibilityService, MacroManagerViewModel macroManagerViewModel)
    {
        _logger = logger;
        _toastService = toastService;
        _screenService = screenService;
        _accessibilityService = accessibilityService;
        _macroManagerViewModel = macroManagerViewModel;
    }

    [RelayCommand]
    public void ManagePatterns()
    {
        _toastService.Show("Manage Patterns");
        _screenService.Show(AndroidWindowView.PatternsNodeView);
    }

    [RelayCommand]
    public void Configure()
    {
        _toastService.Show("Redirecting to YeetMacro...");
        AndroidServiceHelper.LaunchApp(Platform.CurrentActivity.PackageName);
    }

    [RelayCommand]
    public void OpenLog()
    {
        _toastService.Show("Opening Log...");
        _screenService.Show(AndroidWindowView.StatusPanelView);
    }

    [RelayCommand]
    public void Exit()
    {
        _toastService.Show("Exiting...");
        _screenService.StopProjectionService();
    }

    [RelayCommand]
    public void ScreenCapture()
    {
        _toastService.Show("ScreenCapture...");
        _logger.LogDebug("ScreenCapture");
        _screenService.ScreenCapture();
    }
}