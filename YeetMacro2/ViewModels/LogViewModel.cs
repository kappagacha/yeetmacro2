using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class LogViewModel : ObservableObject
{
    IRecorderService _recorderService;
    [ObservableProperty]
    string _debug;
    [ObservableProperty]
    string _info;
    [ObservableProperty]
    bool _isRecording;

    public LogViewModel(IRecorderService recorderService)
    {
        _recorderService = recorderService;
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
