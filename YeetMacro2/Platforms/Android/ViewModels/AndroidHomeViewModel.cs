using Android.Content;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Android.ViewModels;

public partial class AndriodHomeViewModel : ObservableObject
{
    public const int REQUEST_IGNORE_BATTERY_OPTIMIZATIONS = 2;
    [ObservableProperty]
    bool _isProjectionServiceEnabled, _isAccessibilityEnabled, _isAppearing, _showMacroOverlay, _isMacroReady, _showTestView;
    private AndroidScreenService _screenService;
    private MacroManagerViewModel _macroManagerViewModel;
    private YeetAccessibilityService _accessibilityService;

    public string CurrentPackage
    {
        get
        {
            var currentPackage = _accessibilityService.CurrentPackage;
            if (_macroManagerViewModel.SelectedMacroSet?.Package != currentPackage)
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
    public bool IsCurrentPackageValid => _accessibilityService.CurrentPackage == _macroManagerViewModel.SelectedMacroSet?.Package;
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
            var topLeft = _screenService.GetTopLeft();
            return $"x{topLeft.X}y{topLeft.Y} w{_screenService.OverlayWidth}h{_screenService.OverlayHeight}";
        }
    }
    //public string DisplayCutoutTop => _windowManagerService.DisplayCutoutTop.ToString();
    //public bool HasCutoutTop => _windowManagerService.DisplayCutoutTop > 0;
    public bool IsIgnoringBatteryOptimizations
    {
        get
        {
            var pm = (global::Android.OS.PowerManager)Platform.CurrentActivity.GetSystemService(Context.PowerService);
            return pm.IsIgnoringBatteryOptimizations(AppInfo.PackageName);
        }
    }
    public AndriodHomeViewModel(AndroidScreenService screenService, YeetAccessibilityService accessibilityService, 
        MacroManagerViewModel macroManagerViewModel)
    {
        _screenService = screenService;
        _accessibilityService = accessibilityService;
        _macroManagerViewModel = macroManagerViewModel;
    }

    public void InvokeOnPropertyChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
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
            await _screenService.StartProjectionService();
            if (_macroManagerViewModel.InDebugMode) _screenService.Show(AndroidWindowView.DebugDrawView);
            if (_macroManagerViewModel.ShowStatusPanel) _screenService.Show(AndroidWindowView.StatusPanelView);
        }
        else
        {
            _screenService.StopProjectionService();
        }
    }

    [RelayCommand]
    public void ToggleAccessibilityPermissions()
    {
        if (IsAppearing) return;

        IsAccessibilityEnabled = !IsAccessibilityEnabled;
        if (IsAccessibilityEnabled)
        {
            _accessibilityService.Start();
        }
        else
        {
            _accessibilityService.Stop();
        }
    }

    [RelayCommand]
    public void ToggleShowMacroOverlay()
    {
        ShowMacroOverlay = !ShowMacroOverlay;
        if (ShowMacroOverlay)
        {
            _screenService.Show(AndroidWindowView.MacroOverlayView);
        }
        else
        {
            CloseMacroOverlay();
        }
    }

    [RelayCommand]
    public void CloseMacroOverlay()
    {
        _screenService.Close(AndroidWindowView.MacroOverlayView);
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
        _screenService.ResetActionViewLocation();
    }

    [RelayCommand]
    public void ToggleShowTestView()
    {
        ShowTestView = !ShowTestView;
        if (ShowTestView)
        {
            _screenService.Show(AndroidWindowView.TestView);
        }
        else
        {
            _screenService.Close(AndroidWindowView.TestView);
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
    // https://stackoverflow.com/questions/39256501/check-if-battery-optimization-is-enabled-or-not-for-an-app
    public void RequestIgnoreBatteryOptimizations()
    {
        if (IsIgnoringBatteryOptimizations) return;
        var intent = new Intent(global::Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations, global::Android.Net.Uri.Parse($"package:{AppInfo.PackageName}"));
        Platform.CurrentActivity.StartActivityForResult(intent, REQUEST_IGNORE_BATTERY_OPTIMIZATIONS);
    }

    [RelayCommand]
    public void OnAppear()
    {
        IsAppearing = true;
        IsAccessibilityEnabled = _accessibilityService.HasAccessibilityPermissions;
        IsMacroReady = IsProjectionServiceEnabled && IsAccessibilityEnabled;
        //if (!IsProjectionServiceEnabled) await ToggleProjectionService();
        IsAppearing = false;
    }
}

