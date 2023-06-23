using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Platforms.Android.ViewModels;

public partial class AndriodHomeViewModel : ObservableObject
{
    [ObservableProperty]
    bool _isProjectionServiceEnabled, _isAccessibilityEnabled, _isAppearing, _showMacroOverlay, _isMacroReady;
    private AndroidWindowManagerService _windowManagerService;
    private YeetAccessibilityService _accessibilityService;
    private MacroManagerViewModel _macroManagerViewModel;
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
    public bool HasValidResolution => DeviceDisplay.MainDisplayInfo.Width == _macroManagerViewModel.SelectedMacroSet.Resolution.Width &&
        DeviceDisplay.MainDisplayInfo.Height == _macroManagerViewModel.SelectedMacroSet.Resolution.Height;
    public MacroManagerViewModel MacroManagerViewModel => _macroManagerViewModel;
    public string OverlayArea
    {
        get
        {
            var topLeft = _windowManagerService.GetTopLeftByPackage();
            return $"x{topLeft.x}y{topLeft.y} w{_windowManagerService.OverlayWidth}h{_windowManagerService.OverlayHeight}";
        }
    }
    public string DisplayCutoutTop => _windowManagerService.DisplayCutoutTop.ToString();
    public bool HasCutoutTop => _windowManagerService.DisplayCutoutTop > 0;
    public AndriodHomeViewModel(AndroidWindowManagerService windowManagerService, YeetAccessibilityService accessibilityService, 
        MacroManagerViewModel macroManagerViewModel)
    {
        _windowManagerService = windowManagerService;
        _accessibilityService = accessibilityService;
        _macroManagerViewModel = macroManagerViewModel;
    }

    [RelayCommand]
    private async void CopyDb()
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
    public void OnAppear()
    {
        IsAppearing = true;
        IsProjectionServiceEnabled = _windowManagerService.ProjectionServiceEnabled;
        IsAccessibilityEnabled = _accessibilityService.HasAccessibilityPermissions;
        IsMacroReady = IsProjectionServiceEnabled && IsAccessibilityEnabled;
        IsAppearing = false;

        //if (!IsProjectionServiceEnabled)
        //{
        //    ToggleProjectionService();
        //}
    }
}

