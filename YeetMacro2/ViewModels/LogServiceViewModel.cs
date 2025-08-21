using AutoMapper;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

// https://github.com/serilog/serilog/wiki/Developing-a-sink
public partial class LogServiceViewModel(IRepository<Log> _logRepository, IMapper _mapper, IToastService _toastService) : ObservableObject
{   
    [ObservableProperty]
    string _info, _debug;
    [ObservableProperty]
    SortedObservableCollection<Log> _logs = new SortedObservableCollection<Log>((a, b) => (int)(b.Timestamp - a.Timestamp));
    [ObservableProperty]
    Log _selectedLog;
    [ObservableProperty]
    ImageSource _logImage;

    public void LogInfo(string message)
    {
        Log(new Log()
        {
            Message = message,
            Type = LogType.Info,
            Timestamp = DateTime.Now.Ticks
        });
    }

    public void LogDebug(string message)
    {
        Log(new Log()
        {
            Message = message,
            Type = LogType.Info,
            Timestamp = DateTime.Now.Ticks
        });
    }

    public void LogException(Exception ex)
    {
        var subLogs = new SortedObservableCollection<Log>((a, b) => (int)(b.Timestamp - a.Timestamp));
        var exceptionLog = new ExceptionLog() { 
            Message = ex.Message,
            Type = LogType.Error,
            Timestamp = DateTime.Now.Ticks,
            Logs = subLogs
        };

        if (ex.StackTrace is not null)
        {
            var stackLog = new Log()
            {
                Message = ex.StackTrace
            };
            subLogs.Add(stackLog);
        }

        StackTrace st = new StackTrace(1, true);
        StackFrame[] stFrames = st.GetFrames();
        var stackFrameLog = new Log()
        {
            Message = string.Join("", stFrames.Select(sf => sf.ToString()))
        };
        subLogs.Add(stackFrameLog);

        ex = ex.InnerException;
        while (ex is not null)
        {
            var innerExLog = new ExceptionLog()
            {
                Timestamp = DateTime.Now.Ticks,
                Type = LogType.Error,
                Message = ex.Message
            };

            subLogs.Add(innerExLog);
            ex = ex.InnerException;
        }

        Log(exceptionLog);
    }

    public ScreenCaptureLog GenerateScreenCaptureLog(string message)
    {
        var screenshotLog = new ScreenCaptureLog()
        {
            Message = message,
            ScreenCapture = ServiceHelper.GetService<IScreenService>().GetCurrentImageData(),
            Timestamp = DateTime.Now.Ticks
        };
        return screenshotLog;
    }

    public void LogScreenCapture(string message)
    {
        Log(GenerateScreenCaptureLog(message));
    }

    public void Log(Log log)
    {
        _logRepository.Insert(log);
        _logRepository.Save();
    }

    [RelayCommand]
    public void Test()
    {
        var ex = new Exception("Test exception", new Exception("Inner exception"));
        LogException(ex);
        //LogScreenCapture("Test Screenshot Log");
    }

    [RelayCommand]
    public void LoadLogs()
    {
        var logs = _logRepository.Get(l => l.ParentId == null && !l.IsArchived, noTracking: true);
        Logs.Clear();
        foreach (var log in logs)
        {
            Logs.Add(ResolveLog(log));
        }
    }

    [RelayCommand]
    public void LoadArchivedLogs()
    {
        var logs = _logRepository.Get(l => l.ParentId == null && l.IsArchived, noTracking: true);
        Logs.Clear();
        foreach (var log in logs)
        {
            Logs.Add(ResolveLog(log));
        }
    }

    private Log ResolveLog(Log log)
    {
        if (log is ExceptionLog exLog)
        {
            var subLogs = _logRepository.Get(l => l.ParentId == exLog.LogId, noTracking: true);
            exLog.Logs = new SortedObservableCollection<Log>((a, b) => (int)(b.Timestamp - a.Timestamp));
            foreach (var subLog in subLogs)
            {
                exLog.Logs.Add(ResolveLog(subLog));
            }
            return _mapper.Map<ExceptionLogViewModel>(exLog);
        }
        else if (log is ScriptLog sLog)
        {
            var subLogs = _logRepository.Get(l => l.ParentId == sLog.LogId, noTracking: false);
            sLog.Logs = new SortedObservableCollection<Log>((a, b) => (int)(b.Timestamp - a.Timestamp));
            foreach (var subLog in subLogs)
            {
                sLog.Logs.Add(ResolveLog(subLog));
            }
            return _mapper.Map<ScriptLogViewModel>(sLog);
        }
        else if (log is ScreenCaptureLog screenCaptureLog)
        {
            return _mapper.Map<ScreenCaptureLogViewModel>(screenCaptureLog);
        }
        else
        {
            return _mapper.Map<LogViewModel>(log);
        }
    }

    [RelayCommand]
    public void ClearLogs()
    {
        var logs = _logRepository.Get(l => l.ParentId == null && !l.IsArchived);
        foreach (var log in logs)
        {
            _logRepository.Delete(log);
        }
        Logs.Clear();
        _logRepository.Save();
    }


    [RelayCommand]
    public async Task ClearArchivedLogs()
    {
        if (!await Application.Current.Windows[0].Page.DisplayAlert("Delete All Archived Logs", "Are you sure?", "Ok", "Cancel")) return;

        var logs = _logRepository.Get(l => l.ParentId == null && l.IsArchived);

        foreach (var log in logs)
        {
            _logRepository.Delete(log);
        }
        Logs.Clear();
        _logRepository.Save();
    }

    [RelayCommand]
    public void SelectLog(Log log)
    {
        if (SelectedLog is ScreenCaptureLog)
        {
            LogImage = null;
        }

        if (log == SelectedLog)
        {
            log.IsSelected = !log.IsSelected;
        }
        else
        {
            if (SelectedLog is not null)
            {
                SelectedLog.IsSelected = false;
            }

            if (log is not null) log.IsSelected = true;
            SelectedLog = log;
        }

        if (SelectedLog is not null && !SelectedLog.IsSelected)
        {
            SelectedLog = null;
        }

        if (SelectedLog is ScreenCaptureLog screenCaptureLog)
        {
            LogImage = ImageSource.FromStream(() => new MemoryStream(screenCaptureLog.ScreenCapture));
        }
    }

    [RelayCommand]
    public void ToggleLogArchived(Log log)
    {
        log.IsArchived = !log.IsArchived;
        _logRepository.Update(log);
        _logRepository.Save();
    }

    [RelayCommand]
    public async Task DownloadScreenCapture(ScreenCaptureLog log)
    {
#if ANDROID
        // https://stackoverflow.com/questions/75880663/maui-on-android-listing-folder-contents-of-an-sd-card-and-writing-in-it
        if (OperatingSystem.IsAndroidVersionAtLeast(30) && !Android.OS.Environment.IsExternalStorageManager)
        {
            var intent = new Android.Content.Intent();
            intent.SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", Platform.CurrentActivity.PackageName, null);
            intent.SetData(uri);
            Platform.CurrentActivity.StartActivity(intent);
            return;
        }
        else if (!OperatingSystem.IsAndroidVersionAtLeast(30) && await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
        {
            return;
        }
#endif

        var defaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

#if ANDROID
        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
        defaultFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
#endif

        using var stream = new MemoryStream(log.ScreenCapture);
        
        var fileSaverResult = await FileSaver.Default.SaveAsync(defaultFolder, $"{log.Message}.jpeg", stream);
        if (fileSaverResult.IsSuccessful)
        {
            _toastService.Show($"Downloaded screen capture {log.Message}");
        }
        else
        {
            _toastService.Show($"Failed to download screen capture {log.Message}: {fileSaverResult.Exception.Message}");
        }
    }

    [RelayCommand]
    public async Task CopyMessgeToClipboard(Log log)
    {
        await Clipboard.Default.SetTextAsync(log.Message);
        _toastService.Show($"Copied message to clipboard: {log.Message}");
    }
}

[ObservableObject]
public partial class ScriptLogViewModel : ScriptLog
{
    public override bool IsSelected
    {
        get => base.IsSelected;
        set
        {
            base.IsSelected = value;
            OnPropertyChanged();
        }
    }

    public override bool IsArchived
    {
        get => base.IsArchived;
        set
        {
            base.IsArchived = value;
            OnPropertyChanged();
        }
    }
}

[ObservableObject]
public partial class ExceptionLogViewModel : ExceptionLog
{
    public override bool IsSelected
    {
        get => base.IsSelected;
        set
        {
            base.IsSelected = value;
            OnPropertyChanged();
        }
    }

    public override bool IsArchived
    {
        get => base.IsArchived;
        set
        {
            base.IsArchived = value;
            OnPropertyChanged();
        }
    }
}

[ObservableObject]
public partial class ScreenCaptureLogViewModel : ScreenCaptureLog
{
    public override bool IsSelected
    {
        get => base.IsSelected;
        set
        {
            base.IsSelected = value;
            OnPropertyChanged();
        }
    }

    public override bool IsArchived
    {
        get => base.IsArchived;
        set
        {
            base.IsArchived = value;
            OnPropertyChanged();
        }
    }
}

[ObservableObject]
public partial class LogViewModel: Log
{
    public override bool IsSelected
    {
        get => base.IsSelected;
        set
        {
            base.IsSelected = value;
            OnPropertyChanged();
        }
    }

    public override bool IsArchived
    {
        get => base.IsArchived;
        set
        {
            base.IsArchived = value;
            OnPropertyChanged();
        }
    }
}