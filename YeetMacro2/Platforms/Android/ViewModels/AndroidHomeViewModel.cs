using Android.Content;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.ViewModels;
using YeetMacro2.Services;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Platforms.Android.ViewModels;

public partial class AndriodHomeViewModel : ObservableObject
{
    public const int REQUEST_IGNORE_BATTERY_OPTIMIZATIONS = 2;
    [ObservableProperty]
    bool _isProjectionServiceEnabled, _isAccessibilityEnabled, _isAppearing, _showMacroOverlay,
         _showPatternNodeView, _showStatusPanel, _isMacroReady, _showTestView,
         _showSettingNodeView, _showDailyNodeView, _showWeeklyNodeView;
    private readonly MainActivity _context;
    private readonly AndroidScreenService _screenService;
    private readonly MacroManagerViewModel _macroManagerViewModel;
    private readonly YeetAccessibilityService _accessibilityService;
    private readonly IToastService _toastService;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsCurrentPackageValid))]
    string _currentPackage;
    [ObservableProperty]
    Size _currentResolution;
    [ObservableProperty]
    DisplayRotation _displayRotation;
    [ObservableProperty]
    string _widthStatus = "Invalid", _heightStatus = "Invalid";
    public bool IsCurrentPackageValid => CurrentPackage == _macroManagerViewModel.SelectedMacroSet?.Package;
    public MacroManagerViewModel MacroManagerViewModel => _macroManagerViewModel;
    //public string OverlayArea
    //{
    //    get
    //    {
    //        var topLeft = _screenService.GetTopLeft();
    //        var currentResolution = _screenService.CurrentResolution;
    //        return $"x{topLeft.X}y{topLeft.Y} w{currentResolution.Width}h{currentResolution.Height}";
    //    }
    //}
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
        MacroManagerViewModel macroManagerViewModel, IToastService toastService)
    {
        _context = (MainActivity)Platform.CurrentActivity;
        _screenService = screenService;
        _accessibilityService = accessibilityService;
        _macroManagerViewModel = macroManagerViewModel;
        _toastService = toastService;

        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, string>(this, nameof(AndroidScreenService), (r, propertyChangedMessage) =>
        {
            if (IsAppearing) return;

            if (propertyChangedMessage.NewValue)
            {
                ShowActionView();
            }
            else
            {
                IsProjectionServiceEnabled = IsMacroReady = false;
            }
        });

        WeakReferenceMessenger.Default.Register<ForegroundService>(this, (r, foregroundService) =>
        {
            if (IsAppearing) return;

            if (foregroundService.IsRunning)
            {
                IsProjectionServiceEnabled = true;
                ShowActionView();
            }
            else
            {
                IsProjectionServiceEnabled = IsMacroReady = false;
            }
        });

        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, string>(this, nameof(MacroManagerViewModel), (r, propertyChangedMessage) =>
        {
            if (propertyChangedMessage.PropertyName != nameof(MacroManagerViewModel.InDebugMode)) return;

            if (IsProjectionServiceEnabled && propertyChangedMessage.NewValue)
            {
                _screenService.Show(AndroidWindowView.DebugDrawView);
            }
            else
            {
                _screenService.Close(AndroidWindowView.DebugDrawView);
            }
        });

        WeakReferenceMessenger.Default.Register<string, string>(this, nameof(YeetAccessibilityService), (r, currentPackage) =>
        {
            if (_macroManagerViewModel.SelectedMacroSet?.Package != currentPackage)
            {
                var matchingMacroSet = _macroManagerViewModel.MacroSets.FirstOrDefault(ms => ms.Package == currentPackage);
                if (matchingMacroSet != null)
                {
                    _screenService.CloseAll();
                    _macroManagerViewModel.SelectedMacroSet = matchingMacroSet;
                }
            }

            CurrentPackage = currentPackage;
        });

        WeakReferenceMessenger.Default.Register<MacroSetViewModel>(this, (r, macroSet) =>
        {
            _screenService.RefreshActionViewLocation();
        });

        ShowStatusPanel = Preferences.Default.Get(nameof(ShowStatusPanel), false);

        WeakReferenceMessenger.Default.Register<DisplayInfoChangedEventArgs>(this, (r, e) =>
        {
            CurrentResolution = DisplayHelper.UsableResolution;
            DisplayRotation = DisplayHelper.DisplayRotation;

            if (_macroManagerViewModel.SelectedMacroSet is null) WidthStatus = "Invalid";
            else if (_macroManagerViewModel.SelectedMacroSet.SupportsGreaterWidth && DisplayHelper.PhysicalResolution.Width > _macroManagerViewModel.SelectedMacroSet.Resolution.Width) WidthStatus = "Acceptable";
            else if (DisplayHelper.PhysicalResolution.Width == _macroManagerViewModel.SelectedMacroSet.Resolution.Width) WidthStatus = "Valid";
            else WidthStatus = "Invalid";

            if (_macroManagerViewModel.SelectedMacroSet is null) HeightStatus = "Invalid";
            else if (_macroManagerViewModel.SelectedMacroSet.SupportsGreaterHeight && DisplayHelper.PhysicalResolution.Height > _macroManagerViewModel.SelectedMacroSet.Resolution.Height) HeightStatus = "Acceptable";
            else if (DisplayHelper.PhysicalResolution.Height == _macroManagerViewModel.SelectedMacroSet.Resolution.Height) HeightStatus = "Valid";
            else HeightStatus = "Invalid";
        });
    }

    private void ShowActionView()
    {
        _screenService.Show(AndroidWindowView.ActionView);
        if (_macroManagerViewModel.InDebugMode) _screenService.Show(AndroidWindowView.DebugDrawView);
        if (ShowStatusPanel) _screenService.Show(AndroidWindowView.StatusPanelView);
    }

    public void InvokeOnPropertyChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
    }

    [RelayCommand]
    private async Task CopyDb()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(30) && !global::Android.OS.Environment.IsExternalStorageManager)
        {
            var intent = new Intent();
            intent.SetAction(global::Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
            var uri = global::Android.Net.Uri.FromParts("package", Platform.CurrentActivity.PackageName, null);
            intent.SetData(uri);
            Platform.CurrentActivity.StartActivity(intent);
            return;
        }
        else if (!OperatingSystem.IsAndroidVersionAtLeast(30) && await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
        {
            return;
        }

        var fromPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
        var picturesPath = "/storage/emulated/0/Pictures";
        var toPath = Path.Combine(picturesPath, "yeetmacro.db3");

        File.Copy(fromPath, toPath, true);
        _toastService.Show($"Local database copied to ${toPath}");
    }

    [RelayCommand]
    private void DeleteDb()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
        File.Delete(dbPath);
        _toastService.Show($"Local database deleted from ${dbPath}");
    }

    [RelayCommand]
    private void ToggleIsProjectionServiceEnabled()
    {
        if (!IsProjectionServiceEnabled)
        {
            _screenService.StartProjectionService();
        }
        else
        {
            _screenService.StopProjectionService();
        }
    }

    [RelayCommand]
    private async Task ToggleIsAccessibilityEnabled()
    {
        if (!IsAccessibilityEnabled)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                // https://support.google.com/googleplay/android-developer/answer/10964491?sjid=10240033564714765854-NC
                var allow = await Application.Current.Windows[0].Page.DisplayAlert("Accessibility Permission",
    @"YeetMacro requests AccessibilityService for the following:
* perform taps and swipes for script automation
* detects current package for context switching (decides available scripts)

No data is being stored or collected.

If you agree, please tap OK then grant Accessibility service permission to YeetMacro in the next screen", "OK", "cancel");
                if (!allow) return;
            }

            _accessibilityService.Start();
        }
        else
        {
            _accessibilityService.Stop();
        }
    }

    partial void OnShowMacroOverlayChanged(bool value)
    {
        if (value)
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

    partial void OnShowPatternNodeViewChanged(bool value)
    {
        if (value)
        {
            _screenService.Show(AndroidWindowView.PatternNodeView);
        }
        else
        {
            _screenService.Close(AndroidWindowView.PatternNodeView);
        }
    }

    partial void OnShowSettingNodeViewChanged(bool value)
    {
        if (value)
        {
            _screenService.Show(AndroidWindowView.SettingNodeView);
        }
        else
        {
            _screenService.Close(AndroidWindowView.SettingNodeView);
        }
    }

    partial void OnShowDailyNodeViewChanged(bool value)
    {
        if (value)
        {
            _screenService.Show(AndroidWindowView.DailyNodeView);
        }
        else
        {
            _screenService.Close(AndroidWindowView.DailyNodeView);
        }
    }

    partial void OnShowWeeklyNodeViewChanged(bool value)
    {
        if (value)
        {
            _screenService.Show(AndroidWindowView.WeeklyNodeView);
        }
        else
        {
            _screenService.Close(AndroidWindowView.WeeklyNodeView);
        }
    }

    partial void OnShowStatusPanelChanged(bool value)
    {
        Preferences.Default.Set(nameof(ShowStatusPanel), value);

        if (!IsProjectionServiceEnabled) return;

        if (value)
        {
            _screenService.Show(AndroidWindowView.StatusPanelView);
        }
        else
        {
            _screenService.Close(AndroidWindowView.StatusPanelView);
        }
    }

    partial void OnIsProjectionServiceEnabledChanged(bool value)
    {
        if (value && ShowStatusPanel)
        {
            _screenService.Show(AndroidWindowView.StatusPanelView);
        }
        else
        {
            _screenService.Close(AndroidWindowView.StatusPanelView);
        }

        if (value && MacroManagerViewModel.InDebugMode)
        {
            _screenService.Show(AndroidWindowView.DebugDrawView);
        }
        else
        {
            _screenService.Close(AndroidWindowView.DebugDrawView);
        }
    }

    [RelayCommand]
    public async Task ToggleIsMacroReady()
    {
        if (IsMacroReady)
        {
            if (IsProjectionServiceEnabled)
            {
                ToggleIsProjectionServiceEnabled();
            }
        }
        else
        {
            if (!IsProjectionServiceEnabled)
            {
                ToggleIsProjectionServiceEnabled();
                return;
            }
            if (!IsAccessibilityEnabled)
            {
                await ToggleIsAccessibilityEnabled();
            }
        }

        IsMacroReady = IsProjectionServiceEnabled && IsAccessibilityEnabled;
    }

    [RelayCommand]
    public void ResetActionViewLocation()
    {
        _screenService.ResetActionViewLocation();
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

    partial void OnShowTestViewChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            _screenService.Show(AndroidWindowView.TestView);
        }
        else
        {
            _screenService.Close(AndroidWindowView.TestView);
        }
    }

    [RelayCommand]
    private async Task OpenDiscordLink()
    {
        await Launcher.OpenAsync("https://discord.gg/abUPg3RU6J");
    }

    [RelayCommand]
    private async Task OpenGithubLink()
    {
        await Launcher.OpenAsync("https://github.com/kappagacha/yeetmacro2");
    }

    [RelayCommand]
    private async Task OpenDonateLink()
    {
        await Launcher.OpenAsync("https://www.paypal.com/donate?business=Z2GDPP65YMA7G&no_recurring=0&currency_code=USD");
    }

    [RelayCommand]
    private async Task OpenLatestVersionLink()
    {
        await Launcher.OpenAsync("https://github.com/kappagacha/yeetmacro2/releases/tag/latest");
    }

    [RelayCommand]
    private void UpdateDisplayInfo()
    {
        var e = new DisplayInfoChangedEventArgs(DeviceDisplay.MainDisplayInfo);
        DisplayHelper.DisplayRotation = e.DisplayInfo.Rotation;
        DisplayHelper.DisplayInfo = e.DisplayInfo;
        WeakReferenceMessenger.Default.Send(e);
        _toastService.Show($"Display Updated: {DisplayHelper.DisplayInfo}");
    }

    [RelayCommand]
    public void Appear()
    {
        IsAppearing = true;
        IsAccessibilityEnabled = _accessibilityService.HasAccessibilityPermissions;
        IsMacroReady = IsProjectionServiceEnabled && IsAccessibilityEnabled;
        IsAppearing = false;
    }
}