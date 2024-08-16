using Android.Content;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.ViewModels;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

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
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsCurrentPackageValid))]
    string _currentPackage;
    
    public bool IsCurrentPackageValid => CurrentPackage == _macroManagerViewModel.SelectedMacroSet?.Package;
    public Size CurrentResolution => _screenService.CalcResolution;
    public string WidthStatus
    {
        get
        {
            if (_macroManagerViewModel.SelectedMacroSet is null) return "Invalid";
            if (_macroManagerViewModel.SelectedMacroSet.SupportsGreaterWidth && _screenService.CalcResolution.Width > _macroManagerViewModel.SelectedMacroSet.Resolution.Width) return "Acceptable";
            return _screenService.CalcResolution.Width == _macroManagerViewModel.SelectedMacroSet.Resolution.Width ? "Valid" : "Invalid";
        }
    }
    public string HeightStatus
    {
        get
        {
            if (_macroManagerViewModel.SelectedMacroSet is null) return "Invalid";
            if (_macroManagerViewModel.SelectedMacroSet.SupportsGreaterHeight && _screenService.CalcResolution.Height > _macroManagerViewModel.SelectedMacroSet.Resolution.Height) return "Acceptable";
            return _screenService.CalcResolution.Height == _macroManagerViewModel.SelectedMacroSet.Resolution.Height ? "Valid" : "Invalid";
        }
    }
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
        MacroManagerViewModel macroManagerViewModel)
    {
        _context = (MainActivity)Platform.CurrentActivity;
        _screenService = screenService;
        _accessibilityService = accessibilityService;
        _macroManagerViewModel = macroManagerViewModel;

        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, string>(this, nameof(ForegroundService), (r, propertyChangedMessage) =>
        {
            if (IsAppearing) return;

            if (propertyChangedMessage.NewValue)
            {
                _screenService.Show(AndroidWindowView.ActionView);
                if (_macroManagerViewModel.InDebugMode) _screenService.Show(AndroidWindowView.DebugDrawView);
                if (ShowStatusPanel) _screenService.Show(AndroidWindowView.StatusPanelView);
            }
            else
            {
                IsProjectionServiceEnabled = IsMacroReady = false;
            }
        });

        WeakReferenceMessenger.Default.Register<MediaProjectionService>(this, (r, mediaProjectionService) =>
        {
            if (mediaProjectionService.IsInitialized)
            {
                IsProjectionServiceEnabled = true;
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

        WeakReferenceMessenger.Default.Register<string, string>(this, nameof(YeetAccessibilityService), async (r, currentPackage) =>
        {
            if (_macroManagerViewModel.SelectedMacroSet?.Package != currentPackage)
            {
                var matchingMacroSet = _macroManagerViewModel.MacroSets.FirstOrDefault(ms => ms.Package == currentPackage);
                if (matchingMacroSet != null)
                {
                    await Task.Delay(5000);
                    _macroManagerViewModel.SelectedMacroSet = matchingMacroSet;
                }
            }

            CurrentPackage = currentPackage;
        });

        WeakReferenceMessenger.Default.Register<MacroSet>(this, (r, macroSet) =>
        {
            _screenService.RefreshActionViewLocation();
        });


        ShowStatusPanel = Preferences.Default.Get(nameof(ShowStatusPanel), false);
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
                var allow = await Application.Current.MainPage.DisplayAlert("Accessibility Permission",
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
    public void Appear()
    {
        IsAppearing = true;
        IsAccessibilityEnabled = _accessibilityService.HasAccessibilityPermissions;
        IsMacroReady = IsProjectionServiceEnabled && IsAccessibilityEnabled;
        IsAppearing = false;
    }
}