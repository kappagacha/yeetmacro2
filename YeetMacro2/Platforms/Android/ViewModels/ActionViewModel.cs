using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.ViewModels;
using System.ComponentModel;

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
    [ObservableProperty]
    bool _isBusy;

    AndroidWindowManagerService _windowManagerService;
    IScriptService _scriptService;
    MacroManagerViewModel _macroManagerViewModel;
    public ActionViewModel(AndroidWindowManagerService windowManagerService, IScriptService scriptService, MacroManagerViewModel macroManagerViewModel)
    {
        _windowManagerService = windowManagerService;
        _scriptService = scriptService;
        _macroManagerViewModel = macroManagerViewModel;

        _macroManagerViewModel.PropertyChanged += _macroManagerViewModel_PropertyChanged;
        _macroManagerViewModel.OnScriptExecuted = _macroManagerViewModel.OnScriptExecuted ?? new Command(() =>
        {
            _windowManagerService.Close(AndroidWindowView.ScriptsNodeView);
            State = ActionState.Running;
        });
        _macroManagerViewModel.OnScriptFinished = _macroManagerViewModel.OnScriptFinished ?? new Command<string>((result) =>
        {
            State = ActionState.Stopped;
            if (_macroManagerViewModel.ShowScriptLog)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _windowManagerService.Show(AndroidWindowView.LogView);
                });
            }

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
    public async Task ScreenCapture()
    {
        await _windowManagerService.ScreenCapture();
    }
}
