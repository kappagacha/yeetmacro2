using CommunityToolkit.Mvvm.ComponentModel;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class StatusPanelViewModel : ObservableObject
{
    IRecorderService _recorderService;
    IToastService _toastService;
    [ObservableProperty]
    LogViewModel _logViewModel;
    [ObservableProperty]
    bool _isRecording;
    public StatusPanelViewModel(IRecorderService recorderService, IToastService toastService, LogViewModel logViewModel)
    {
        _recorderService = recorderService;
        _toastService = toastService;
        _logViewModel = logViewModel;
    }

    async partial void OnIsRecordingChanged(bool value)
    {
        if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted) return;

#if ANDROID
        // https://stackoverflow.com/questions/75880663/maui-on-android-listing-folder-contents-of-an-sd-card-and-writing-in-it
        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.R && !Android.OS.Environment.IsExternalStorageManager)
        {
            var intent = new Android.Content.Intent();
            intent.SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", Platform.CurrentActivity.PackageName, null);
            intent.SetData(uri);
            Platform.CurrentActivity.StartActivity(intent);
            _isRecording = false;
            OnPropertyChanged(nameof(IsRecording));
            return;
        }
#endif

        try
        {
            if (value)
            {
                _recorderService.StartRecording();
                _toastService.Show("Started Recording...");
            }
            else
            {
                _recorderService.StopRecording();
                _toastService.Show("Stopped Recording...");
            }
        }
        catch (Exception ex)
        {
            _toastService.Show($"Toggle IsRecording Failed: ${ex.Message}");
        }
    }
}