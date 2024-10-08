﻿using CommunityToolkit.Mvvm.ComponentModel;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class StatusPanelViewModel(IRecorderService recorderService, IToastService toastService, LogServiceViewModel LogServiceViewModel) : ObservableObject
{
    readonly IRecorderService _recorderService = recorderService;
    readonly IToastService _toastService = toastService;
    [ObservableProperty]
    LogServiceViewModel _logServiceViewModel = LogServiceViewModel;
    [ObservableProperty]
    bool _isRecording;

    async partial void OnIsRecordingChanged(bool value)
    {
        if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted) return;

#if ANDROID
        // https://stackoverflow.com/questions/75880663/maui-on-android-listing-folder-contents-of-an-sd-card-and-writing-in-it
        if (OperatingSystem.IsAndroidVersionAtLeast(30) && !Android.OS.Environment.IsExternalStorageManager)
        {
            var intent = new Android.Content.Intent();
            intent.SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", Platform.CurrentActivity.PackageName, null);
            intent.SetData(uri);
            Platform.CurrentActivity.StartActivity(intent);
            IsRecording = false;
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