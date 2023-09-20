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
    [ObservableProperty]
    string _debug, _info;
    string _currentMacroSet = "???", _currentScript = "???";
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

    // Always persists on exception
    public void LogException(Exception ex, string message = null)
    {
        if (_currentLogGroup is null) InitLogGroup(ex);

        if (message is not null)
        {
            var log = new Log()
            {
                Timestamp = DateTime.Now.Ticks,
                Type = LogType.Error,
                Message = message
            };
            _currentLogGroup.Logs.Add(log);
        }

        while (ex is not null)
        {
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

    public void Emit(LogEvent logEvent)
    {
        return;

        if (logEvent.Properties.Count > 1 && logEvent.Properties.ContainsKey("macroSet"))
        {
            _currentMacroSet = logEvent.Properties["macroSet"].ToString();
            _currentScript = logEvent.Properties["script"].ToString();
            InitLogGroup();
            return;
        }

        if (logEvent.Properties.Count > 1 && logEvent.Properties.ContainsKey("persistLogs") &&
            logEvent.Properties["persistLogs"] is ScalarValue sv)
        {
            _persistLogs = (bool)sv.Value;
            return;
        }

        switch (logEvent.Level)
        {
            case LogEventLevel.Error:
                if (logEvent.Exception is not null) LogException(logEvent.Exception, logEvent.MessageTemplate.Text);
                break;
            case LogEventLevel.Debug:
                Debug = logEvent.MessageTemplate.Text;
                if (_persistLogs) Log(LogType.Debug, $"[{Info}] {Debug}");
                break;
            case LogEventLevel.Information:
                Info = logEvent.MessageTemplate.Text;
                if (_persistLogs) Log(LogType.Info, $"[{Info}]");
                break;
            case LogEventLevel.Verbose:
                if (_persistLogs) Log(LogType.Verbose, logEvent.MessageTemplate.Text);
                break;
        }

        if (logEvent.Level == LogEventLevel.Verbose)
        {
            Console.WriteLine($"[{logEvent.Level}] {logEvent.MessageTemplate.Text}");
        }
    }

    private void InitLogGroup(Exception ex = null)
    {
        _currentLogGroup = new LogGroup() { Timestamp = DateTime.Now.Ticks, MacroSet = _currentMacroSet, Script = ex?.Message ?? _currentScript, Stack = (ex is not null ? ex.StackTrace ?? Environment.StackTrace : null) };
        _currentLogGroup.Logs = new SortedObservableCollection<Log>((a, b) => (int)(b.Timestamp - a.Timestamp));
        LogGroups.Add(_currentLogGroup);
        _logGroupRepository.Value.Insert(_currentLogGroup);
        _logGroupRepository.Value.Save();
    }
}
