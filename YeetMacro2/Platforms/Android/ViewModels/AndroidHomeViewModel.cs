using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Android.ViewModels;

public partial class AndriodHomeViewModel : ObservableObject
{
    [ObservableProperty]
    bool _isProjectionServiceEnabled, _isAccessibilityEnabled, _isAppearing, _showMacroOverlay, _isMacroReady, _showTestView, _isIgnoringBatteryOptimization;
    private AndroidWindowManagerService _windowManagerService;
    private MacroManagerViewModel _macroManagerViewModel;

    public string CurrentPackage
    {
        get
        {
            var currentPackage = AndroidServiceHelper.AccessibilityService?.CurrentPackage;
            if (currentPackage is not null && _macroManagerViewModel.SelectedMacroSet?.Package != currentPackage)
            {
                var matchingMacroSet = _macroManagerViewModel.MacroSets.FirstOrDefault(ms => ms.Package == currentPackage);
                if (matchingMacroSet != null)
                {
                    _macroManagerViewModel.SelectedMacroSet = matchingMacroSet;
                }
            }

            return currentPackage;
        }
    }
    public bool IsCurrentPackageValid => AndroidServiceHelper.AccessibilityService?.CurrentPackage == _macroManagerViewModel.SelectedMacroSet?.Package;
    public Size CurrentResolution => new Size(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);
    public string WidthStatus
    {
        get
        {
            if (_macroManagerViewModel.SelectedMacroSet is null) return "Invalid";
            if (_macroManagerViewModel.SelectedMacroSet.SupportsGreaterWidth && DeviceDisplay.MainDisplayInfo.Width > _macroManagerViewModel.SelectedMacroSet.Resolution.Width) return "Acceptable";
            return DeviceDisplay.MainDisplayInfo.Width == _macroManagerViewModel.SelectedMacroSet.Resolution.Width ? "Valid" : "Invalid";
        }
    }
    public string HeightStatus
    {
        get
        {
            if (_macroManagerViewModel.SelectedMacroSet is null) return "Invalid";
            if (_macroManagerViewModel.SelectedMacroSet.SupportsGreaterHeight && DeviceDisplay.MainDisplayInfo.Height > _macroManagerViewModel.SelectedMacroSet.Resolution.Height) return "Acceptable";
            return DeviceDisplay.MainDisplayInfo.Height == _macroManagerViewModel.SelectedMacroSet.Resolution.Height ? "Valid" : "Invalid";
        }
    }
    public MacroManagerViewModel MacroManagerViewModel => _macroManagerViewModel;
    public string OverlayArea
    {
        get
        {
            var topLeft = _windowManagerService.GetTopLeftByPackage();
            return $"x{topLeft.x}y{topLeft.y} w{_windowManagerService.OverlayWidth}h{_windowManagerService.OverlayHeight}";
        }
    }
    //public string DisplayCutoutTop => _windowManagerService.DisplayCutoutTop.ToString();
    //public bool HasCutoutTop => _windowManagerService.DisplayCutoutTop > 0;
    public AndriodHomeViewModel(AndroidWindowManagerService windowManagerService, MacroManagerViewModel macroManagerViewModel)
    {
        _windowManagerService = windowManagerService;
        _macroManagerViewModel = macroManagerViewModel;
    }

    [RelayCommand]
    private async Task CopyDb()
    {
        if (await Permissions.RequestAsync<Permissions.StorageWrite>() == PermissionStatus.Granted)
        {
            var fromPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
            var picturesPath = "/storage/emulated/0/Pictures";
            var toPath = Path.Combine(picturesPath, "yeetmacro.db3");

            File.Copy(fromPath, toPath, true);
        }
    }

    [RelayCommand]
    private void DeleteDb()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
        File.Delete(dbPath);
    }

    [RelayCommand]
    public async Task ToggleProjectionService()
    {
        IsProjectionServiceEnabled = !IsProjectionServiceEnabled;
        if (IsProjectionServiceEnabled)
        {
            await _windowManagerService.StartProjectionService();
            if (_macroManagerViewModel.InDebugMode) _windowManagerService.Show(AndroidWindowView.DebugDrawView);
            if (_macroManagerViewModel.ShowStatusPanel) _windowManagerService.Show(AndroidWindowView.StatusPanelView);
        }
        else
        {
            _windowManagerService.StopProjectionService();
        }
    }

    [RelayCommand]
    public void ToggleAccessibilityPermissions()
    {
        if (IsAppearing) return;

        IsAccessibilityEnabled = !IsAccessibilityEnabled;
        if (IsAccessibilityEnabled)
        {
            _windowManagerService.RequestAccessibilityPermissions();
        }
        else
        {
            _windowManagerService.RevokeAccessibilityPermissions();
        }
    }

    [RelayCommand]
    public void ToggleShowMacroOverlay()
    {
        ShowMacroOverlay = !ShowMacroOverlay;
        if (ShowMacroOverlay)
        {
            _windowManagerService.Show(AndroidWindowView.MacroOverlayView);
        }
        else
        {
            CloseMacroOverlay();
        }
    }

    [RelayCommand]
    public void CloseMacroOverlay()
    {
        _windowManagerService.Close(AndroidWindowView.MacroOverlayView);
    }

    [RelayCommand]
    public async Task ToggleIsMacroReady()
    {
        if (IsMacroReady)
        {
            IsMacroReady = false;
            if (IsProjectionServiceEnabled)
            {
                await ToggleProjectionService();
            }
        }
        else
        {
            IsMacroReady = true;
            if (!IsProjectionServiceEnabled)
            {
                await ToggleProjectionService();
            }
            if (!IsAccessibilityEnabled)
            {
                ToggleAccessibilityPermissions();
            }
        }
    }

    [RelayCommand]
    public void ResetActionViewLocation()
    {
        _windowManagerService.ResetActionViewLocation();
    }

    [RelayCommand]
    public void ToggleShowTestView()
    {
        ShowTestView = !ShowTestView;
        if (ShowTestView)
        {
            _windowManagerService.Show(AndroidWindowView.TestView);
        }
        else
        {
            _windowManagerService.Close(AndroidWindowView.TestView);
        }
    }

    [RelayCommand]
    public void Exit()
    {
        Application.Current.Quit();
    }

    [RelayCommand]
    public void ThrowException()
    {
        throw new Exception("Test Exception from AndroidHomeViewModel", new Exception("This is an inner exception"));
    }

    [RelayCommand]
    public void RequestIgnoreBatteryOptimizations()
    {
        _windowManagerService.RequestIgnoreBatteryOptimizations();
    }

    [RelayCommand]
    public void OnAppear()
    {
        IsAppearing = true;
        IsProjectionServiceEnabled = _windowManagerService.ProjectionServiceEnabled;
        IsAccessibilityEnabled = AndroidServiceHelper.AccessibilityService?.HasAccessibilityPermissions ?? false;
        IsMacroReady = IsProjectionServiceEnabled && IsAccessibilityEnabled;
        //if (!IsProjectionServiceEnabled) await ToggleProjectionService();
        IsIgnoringBatteryOptimization = _windowManagerService.IsIgnoringBatteryOptimizations;
        IsAppearing = false;
    }
}

