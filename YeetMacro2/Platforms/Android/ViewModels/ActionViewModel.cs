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
        else if (e.PropertyName == nameof(MacroManagerViewModel.ShowLogView))
        {
            if (_macroManagerViewModel.ShowLogView)
            {
                _windowManagerService.Show(AndroidWindowView.LogView);
            }
            else
            {
                _windowManagerService.Close(AndroidWindowView.LogView);
            }
        }
    }

    [RelayCommand]
    public async void Execute()
    {
        await _macroManagerViewModel.Scripts.WaitForInitialization();

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
}
