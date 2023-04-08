using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class ActionMenuViewModel : ObservableObject
{
    ILogger _logger;
    IToastService _toastService;
    IScriptsService _scriptsService;
    AndroidWindowManagerService _windowManagerService;
    YeetAccessibilityService _accessibilityService;
    [ObservableProperty]
    bool _showLogView, _inDebugMode;

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

    public ActionMenuViewModel(ILogger<ActionViewModel> logger, IToastService toastService, AndroidWindowManagerService windowManagerService,
        YeetAccessibilityService accessibilityService, IScriptsService scriptsService)
    {
        _logger = logger;
        _toastService = toastService;
        _windowManagerService = windowManagerService;
        _accessibilityService = accessibilityService;
        _scriptsService = scriptsService;
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
        _windowManagerService.Show(AndroidWindowView.LogView);
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

    [RelayCommand]
    public void ToggleLogView()
    {
        if (ShowLogView)
        {
            _windowManagerService.Show(AndroidWindowView.LogView);
        }
        else
        {
            _windowManagerService.Close(AndroidWindowView.LogView);
        }
    }

    [RelayCommand]
    public void ToggleDebugMode()
    {
        _scriptsService.InDebugMode = InDebugMode;
        if (!InDebugMode)
        {
            _windowManagerService.Close(AndroidWindowView.DebugDrawView);
        } else
        {
            _windowManagerService.Show(AndroidWindowView.DebugDrawView);
        }
    }
}