﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.ViewModels;

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
    IScriptsService _scriptService;
    MacroManagerViewModel _macroManagerViewModel;
    IToastService _toastService;
    public ActionViewModel(AndroidWindowManagerService windowManagerService, IScriptsService scriptService, MacroManagerViewModel macroManagerViewModel,
        IToastService toastService)
    {
        _windowManagerService = windowManagerService;
        _scriptService = scriptService;
        _macroManagerViewModel = macroManagerViewModel;
        _toastService = toastService;
    }

    [RelayCommand]
    public async void Execute()
    {
        var scriptList = _macroManagerViewModel.Scripts.Scripts.Select(s => s.Name);
        if (!scriptList.Any())
        {
            _toastService.Show("No script found...");
            return;
        }

        switch (State)
        {
            case ActionState.Stopped:
                var script = await _windowManagerService.SelectOption("Run Script", scriptList.ToArray());
                if (script == null)
                {
                    _toastService.Show("Run script canceled...");
                    return;
                };
                State = ActionState.Running;
                _scriptService.RunScript(script);
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
            _windowManagerService.Show(WindowView.ActionMenuView);
        }
    }
}