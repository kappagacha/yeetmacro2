using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class StatusPanelViewModel : ObservableObject
{
    IRecorderService _recorderService;
    [ObservableProperty]
    bool _isRecording;
    [ObservableProperty]
    LogViewModel _logViewModel;
    public StatusPanelViewModel(IRecorderService recorderService, LogViewModel logViewModel)
    {
        _recorderService = recorderService;
        _logViewModel = logViewModel;
    }


    [RelayCommand]
    public void ToggleRecording()
    {
        if (!IsRecording)
        {
            _recorderService.StopRecording();
        }
        else
        {
            _recorderService.StartRecording();
        }
    }
}