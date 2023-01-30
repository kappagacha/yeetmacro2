using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class LogViewModel : ObservableObject
{
    IRecorderService _recorderService;
    [ObservableProperty]
    string _message;
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
            _recorderService.Stop();
        }
        else
        {
            _recorderService.Start();
        }
    }
}
