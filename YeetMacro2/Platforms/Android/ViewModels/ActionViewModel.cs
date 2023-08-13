using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.ViewModels;
using System.ComponentModel;
using System.Text.Json;
using YeetMacro2.Data.Serialization;

namespace YeetMacro2.Platforms.Android.ViewModels;

public enum ActionState
{
    Stopped,
    Running
}

public partial class ActionViewModel : ObservableObject, IMovable
{
    [ObservableProperty]
    ActionState _state;
    public bool IsMoving { get; set; }
    public Point Location { get; set; }
    [ObservableProperty]
    bool _isBusy;

    AndroidWindowManagerService _windowManagerService;
    IScriptService _scriptService;
    MacroManagerViewModel _macroManagerViewModel;
    IToastService _toastService;
    JsonSerializerOptions _serializationOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = PointPropertiesResolver.Instance
    };

    public ActionViewModel(AndroidWindowManagerService windowManagerService, IScriptService scriptService, MacroManagerViewModel macroManagerViewModel, IToastService toastService)
    {
        _windowManagerService = windowManagerService;
        _scriptService = scriptService;
        _macroManagerViewModel = macroManagerViewModel;
        _toastService = toastService;

        _macroManagerViewModel.PropertyChanged += _macroManagerViewModel_PropertyChanged;
        _macroManagerViewModel.OnScriptExecuted = _macroManagerViewModel.OnScriptExecuted ?? new Command(() =>
        {
            _windowManagerService.Close(AndroidWindowView.ScriptsNodeView);
            State = ActionState.Running;
        });
        _macroManagerViewModel.OnScriptFinished = _macroManagerViewModel.OnScriptFinished ?? new Command<string>((result) =>
        {
            State = ActionState.Stopped;

            if (!String.IsNullOrWhiteSpace(result))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _windowManagerService.ShowMessage(result);
                });
            }
        });
    }

    private void _macroManagerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MacroManagerViewModel.InDebugMode))
        {
            if (_macroManagerViewModel.InDebugMode)
            {
                _windowManagerService.Show(AndroidWindowView.DebugDrawView);
            }
            else
            {
                _windowManagerService.Close(AndroidWindowView.DebugDrawView);
            }
        }
        else if (e.PropertyName == nameof(MacroManagerViewModel.ShowStatusPanel))
        {
            if (_macroManagerViewModel.ShowStatusPanel)
            {
                _windowManagerService.Show(AndroidWindowView.StatusPanelView);
            }
            else
            {
                _windowManagerService.Close(AndroidWindowView.StatusPanelView);
            }
        }
    }

    [RelayCommand]
    public async void Execute()
    {
        if (_macroManagerViewModel.SelectedMacroSet is null)
        {
            _toastService.Show("No MacroSet selected...");
            return;
        }

        var selectedMacroSetName = Preferences.Default.Get<string>(nameof(MacroManagerViewModel.SelectedMacroSet), null);
        if (selectedMacroSetName is not null)
        {
            var orientation = DeviceDisplay.Current.MainDisplayInfo.Orientation;
            var preferenceKey = $"{selectedMacroSetName}_location_{orientation}";
            Preferences.Default.Set(preferenceKey, JsonSerializer.Serialize(Location, _serializationOptions));
        }

        IsBusy = true;
        await _macroManagerViewModel.Scripts.WaitForInitialization();
        IsBusy = false;

        switch (State)
        {
            case ActionState.Stopped:
                _windowManagerService.Show(AndroidWindowView.ScriptsNodeView);
                break;
            case ActionState.Running:
                _scriptService.Stop();
                State = ActionState.Stopped;
                break;
        }
    }

    [RelayCommand]
    public void OpenMenu(object o)
    {
        if (!IsMoving)
        {
            _windowManagerService.Show(AndroidWindowView.ActionMenuView);
        }
    }

    [RelayCommand]
    public void ScreenCapture()
    {
        _windowManagerService.ScreenCapture();
    }
}
