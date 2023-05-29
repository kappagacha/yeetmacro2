using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class StatusPanelViewModel : ObservableObject
{
    IRecorderService _recorderService;
    [ObservableProperty]
    string _debug;
    [ObservableProperty]
    string _info;
    [ObservableProperty]
    bool _isRecording, _isSavingLog;
    [ObservableProperty]
    SortedObservableCollection<LogRecord> _savedLogs = new SortedObservableCollection<LogRecord>((a, b) => (int)(b.TimeStamp - a.TimeStamp));

    public StatusPanelViewModel(IRecorderService recorderService)
    {
        _recorderService = recorderService;
    }

    partial void OnIsSavingLogChanged(bool value)
    {
        if (value)
        {
            SavedLogs.Clear();
        }
    }

    partial void OnDebugChanged(string value)
    {
        if (IsSavingLog)
        {
            SavedLogs.Add(new LogRecord(DateTime.Now.Ticks, $"[{Info}] {value}"));
        }
    }

    partial void OnInfoChanged(string value)
    {
        if (IsSavingLog)
        {
            SavedLogs.Add(new LogRecord(DateTime.Now.Ticks, $"[{value}]"));
        }
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

    public class LogRecord
    {
        public long TimeStamp { get; set; }
        public string Log { get; set; }
        public LogRecord(long timeStamp, string log)
        {
            TimeStamp = timeStamp;
            Log = log;
        }
    }
}
