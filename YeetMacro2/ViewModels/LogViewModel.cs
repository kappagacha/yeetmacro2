
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog.Core;
using Serilog.Events;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;

namespace YeetMacro2.ViewModels;

// https://github.com/serilog/serilog/wiki/Developing-a-sink
public partial class LogViewModel : ObservableObject, ILogEventSink
{
    const string PERSIST_LOGS_KEY = "global_persist_logs";
    [ObservableProperty]
    string _debug, _info, _currentMacroSet, _currentScript;
    [ObservableProperty]
    bool _persistLogs;
    LogGroup _currentLogGroup;
    Lazy<IRepository<LogGroup>> _logGroupRepository;
    SortedObservableCollection<LogGroup> _logGroups;
    [ObservableProperty]
    LogGroup _selectedLogGroup;
    [ObservableProperty]
    Log _selectedLog;

    public SortedObservableCollection<LogGroup> LogGroups
    {
        get 
        { 
            if (_logGroups == null)
            {
                _logGroups = new SortedObservableCollection<LogGroup>((a, b) => (int)(b.Timestamp - a.Timestamp));
                var logGroups = _logGroupRepository.Value.Get();
                foreach (var logGroup in logGroups)
                {
                    _logGroups.Add(logGroup);
                }
            }
            return _logGroups; 
        }
    }

    // Using Lazy to resolve later because of circular dependency
    public LogViewModel(Lazy<IRepository<LogGroup>> logGroupRepository)
    {
        _logGroupRepository = logGroupRepository;
        CurrentMacroSet = "???";
        CurrentScript = "???";
        PersistLogs = Preferences.Default.Get(PERSIST_LOGS_KEY, false);
    }

    [RelayCommand]
    public async Task SelectLogGroup(Object logGroup)
    {
        SelectedLogGroup = logGroup as LogGroup;
        await Shell.Current.GoToAsync("LogGroupItem");
    }

    [RelayCommand]
    public async Task SelectLog(Object log)
    {
        SelectedLog = log as Log;
        await Shell.Current.GoToAsync("Log");
    }

    [RelayCommand]
    public void ClearAll()
    {
        foreach (var logGroup in LogGroups)
        {
            _logGroupRepository.Value.Delete(logGroup);
        }
        LogGroups.Clear();
        _logGroupRepository.Value.Save();
        _currentLogGroup = null;
    }

    partial void OnPersistLogsChanged(bool value)
    {
        Preferences.Default.Set(PERSIST_LOGS_KEY, value);
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

        _logGroupRepository.Value.Update(_currentLogGroup);
        _logGroupRepository.Value.Save();
    }

    private void Log(LogType logType, String message)
    {
        ResolveCurrentLogGroup();
        var log = new Log()
        {
            Timestamp = DateTime.Now.Ticks,
            Type = logType,
            Message = message
        };

        _currentLogGroup.Logs.Add(log);
        _logGroupRepository.Value.Update(_currentLogGroup);
        _logGroupRepository.Value.Save();
    }

    private void ResolveCurrentLogGroup()
    {
        if (_currentLogGroup is not null && _currentLogGroup.MacroSet == CurrentMacroSet && _currentLogGroup.Script == CurrentScript) return;

        _currentLogGroup = new LogGroup() { Timestamp = DateTime.Now.Ticks, MacroSet = CurrentMacroSet, Script = CurrentScript };
        _currentLogGroup.Logs = new SortedObservableCollection<Log>((a, b) => (int)(b.Timestamp - a.Timestamp));
        _logGroupRepository.Value.Insert(_currentLogGroup);
        _logGroupRepository.Value.Save();
        LogGroups.Add(_currentLogGroup);
    }

    public void Emit(LogEvent logEvent)
    {
        switch (logEvent.Level)
        {
            case LogEventLevel.Debug:
                Debug = logEvent.MessageTemplate.Text;
                if (PersistLogs) Log(LogType.Debug, $"[{Info}] {Debug}");
                break;
            case LogEventLevel.Information:
                Info = logEvent.MessageTemplate.Text;
                if (PersistLogs) Log(LogType.Info, $"[{Info}]");
                break;
        }

        //Debug.WriteLine("{0} {1}", logEvent.Level, logEvent.MessageTemplate.Text);
    }
}
