
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;

namespace YeetMacro2.ViewModels;

public partial class LogViewModel : ObservableObject
{
    const string PERSIST_LOGS_KEY = "global_persist_logs";
    [ObservableProperty]
    string _debug, _info, _currentMacroSet, _currentScript;
    [ObservableProperty]
    bool _persistLogs;
    LogGroup _currentLogGroup;
    IRepository<LogGroup> _logGroupRepository;
    [ObservableProperty]
    SortedObservableCollection<LogGroup> _logGroups = new SortedObservableCollection<LogGroup>((a, b) => (int)(b.Timestamp - a.Timestamp));
    [ObservableProperty]
    LogGroup _selectedLogGroup;
    [ObservableProperty]
    Log _selectedLog;
    public LogViewModel(IRepository<LogGroup> logGroupRepository)
    {
        _logGroupRepository = logGroupRepository;
        CurrentMacroSet = "???";
        CurrentScript = "???";
        PersistLogs = Preferences.Default.Get(PERSIST_LOGS_KEY, false);
        var logGroups = _logGroupRepository.Get();
        foreach (var logGroup in logGroups)
        {
            LogGroups.Add(logGroup);
        }
    }

    [RelayCommand]
    public async void SelectLogGroup(Object logGroup)
    {
        SelectedLogGroup = logGroup as LogGroup;
        await Shell.Current.GoToAsync("LogGroupItem");
    }

    [RelayCommand]
    public async void SelectLog(Object log)
    {
        SelectedLog = log as Log;
        await Shell.Current.GoToAsync("Log");
    }

    [RelayCommand]
    public void ClearAll()
    {
        foreach (var logGroup in LogGroups)
        {
            _logGroupRepository.Delete(logGroup);
        }
        LogGroups.Clear();
        _logGroupRepository.Save();
    }

    partial void OnPersistLogsChanged(bool value)
    {
        Preferences.Default.Set(PERSIST_LOGS_KEY, value);
    }

    partial void OnDebugChanged(string value)
    {
        if (!PersistLogs) return;
        ResolveCurrentLogGroup();
        Log(LogType.Debug, $"[{Info}] {value}");
    }

    partial void OnInfoChanged(string value)
    {
        if (!PersistLogs) return;
        ResolveCurrentLogGroup();
        Log(LogType.Info, $"[{value}]");
    }

    // Always persists on exception
    public void LogException(Exception ex)
    {
        while (ex is not null)
        {
            ResolveCurrentLogGroup();
            var log = new Log()
            {
                Timestamp = DateTime.Now.Ticks,
                Type = LogType.Error,
                Message = ex.Message,
                Stack = ex.StackTrace
            };

            _currentLogGroup.Logs.Add(log);
            ex = ex.InnerException;
        }

        _logGroupRepository.Update(_currentLogGroup);
        _logGroupRepository.Save();
    }

    private void Log(LogType logType, String message)
    {
        var log = new Log()
        {
            Timestamp = DateTime.Now.Ticks,
            Type = logType,
            Message = message
        };

        _currentLogGroup.Logs.Add(log);
        _logGroupRepository.Update(_currentLogGroup);
        _logGroupRepository.Save();
    }

    private void ResolveCurrentLogGroup()
    {
        if (_currentLogGroup is not null && _currentLogGroup.MacroSet == CurrentMacroSet && _currentLogGroup.Script == CurrentScript) return;

        _currentLogGroup = new LogGroup() { Timestamp = DateTime.Now.Ticks, MacroSet = CurrentMacroSet, Script = CurrentScript };
        _currentLogGroup.Logs = new SortedObservableCollection<Log>((a, b) => (int)(b.Timestamp - a.Timestamp));
        _logGroupRepository.Insert(_currentLogGroup);
        _logGroupRepository.Save();
        LogGroups.Add(_currentLogGroup);
    }
}
