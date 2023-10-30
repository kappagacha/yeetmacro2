using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.ViewModels;
using System.Text.Json;
using YeetMacro2.Data.Serialization;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.Data.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

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

    AndroidScreenService _screenService;
    IScriptService _scriptService;
    MacroManagerViewModel _macroManagerViewModel;
    IToastService _toastService;
    JsonSerializerOptions _serializationOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = PointPropertiesResolver.Instance
    };

    public ActionViewModel(AndroidScreenService screenService, IScriptService scriptService, 
        MacroManagerViewModel macroManagerViewModel, IToastService toastService)
    {
        _screenService = screenService;
        _scriptService = scriptService;
        _macroManagerViewModel = macroManagerViewModel;
        _toastService = toastService;

        WeakReferenceMessenger.Default.Register<ScriptEventMessage>(this, (r, scriptEventMessage) =>
        {
            if (scriptEventMessage.Value.Type == ScriptEventType.Started)
            {
                _screenService.Close(AndroidWindowView.ScriptsNodeView);
                State = ActionState.Running;
            }
            else
            {
                State = ActionState.Stopped;
                if (!String.IsNullOrWhiteSpace(scriptEventMessage.Value.Result))
                {
                    MainThread.BeginInvokeOnMainThread(() => _screenService.ShowMessage(scriptEventMessage.Value.Result));
                }
            }
        });

        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, string>(this, nameof(MacroManagerViewModel), (r, propertyChangedMessage) => {
            AndroidWindowView windowView;
            switch (propertyChangedMessage.PropertyName)
            {
                case nameof(MacroManagerViewModel.InDebugMode):
                    windowView = AndroidWindowView.DebugDrawView;
                    break;
                case nameof(MacroManagerViewModel.ShowStatusPanel):
                    windowView = AndroidWindowView.StatusPanelView;
                    break;
                default:
                    return;
            }

            if (propertyChangedMessage.NewValue)
            {
                _screenService.Show(windowView);
            }
            else
            {
                _screenService.Close(windowView);
            }
        });
    }

    [RelayCommand]
    public async Task Execute()
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
                _screenService.Show(AndroidWindowView.ScriptsNodeView);
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
            _screenService.Show(AndroidWindowView.ActionMenuView);
        }
    }

    [RelayCommand]
    public void ScreenCapture()
    {
        _screenService.ScreenCapture();
    }
}
