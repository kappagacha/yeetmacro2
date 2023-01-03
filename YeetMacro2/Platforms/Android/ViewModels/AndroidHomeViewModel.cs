using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Platforms.Android.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;

public partial class AndriodHomeViewModel : ObservableObject
{
    [ObservableProperty]
    bool _isProjectionServiceEnabled;
    [ObservableProperty]
    bool _isAccessibilityEnabled;
    [ObservableProperty]
    bool _isAppearing;
    [ObservableProperty]
    bool _showLogView;
    private AndroidWindowManagerService _windowManagerService;
    private YeetAccessibilityService _accessibilityService;

    public AndriodHomeViewModel(AndroidWindowManagerService windowManagerService, YeetAccessibilityService accessibilityService)
    {
        _windowManagerService = windowManagerService;
        _accessibilityService = accessibilityService;
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
    public void ToggleProjectionService()
    {
        if (IsProjectionServiceEnabled)
        {
            _windowManagerService.StartProjectionService();
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

    [RelayCommand]
    public void OnAppear()
    {
        _isAppearing = true;
        IsProjectionServiceEnabled = _windowManagerService.ProjectionServiceEnabled;
        IsAccessibilityEnabled = _accessibilityService.HasAccessibilityPermissions;
        _isAppearing = false;

        //IsProjectionServiceEnabled = true;
        //_windowManagerService.Show(WindowView.PatternsTreeView);
    }
}

