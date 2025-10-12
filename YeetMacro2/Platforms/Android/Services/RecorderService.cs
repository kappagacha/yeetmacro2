using Android.Content;
using Android.Hardware.Display;
using Android.Media.Projection;
using Android.Media;
using Android.Views;
using YeetMacro2.Services;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Platforms.Android.Services;

// https://github.com/chinmoyp/screenrecorder/blob/master/app/src/main/java/com/confusedbox/screenrecorder/MainActivity.java
// https://github.com/android/media-samples/blob/main/ScreenCapture/Application/src/main/java/com/example/android/screencapture/ScreenCaptureFragment.java
// https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionRecording.kt
// https://github.com/Fate-Grand-Automata/FGA/blob/6d6b5f190817574f2d07f04f124b677c39b09634/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
public class RecorderService : IRecorderService, IDisposable
{
    MediaProjection _mediaProjection;
    VirtualDisplay _virtualDisplay;
    MediaRecorder _mediaRecorder;
    Intent _resultData;
    int _resultCode;
    bool _isRecording;
    bool _recorderStarted; // Track if MediaRecorder.Start() was successfully called
    bool _isStoppingFromCallback; // Track if we're stopping due to MediaProjection callback
    public const int REQUEST_MEDIA_PROJECTION = 3;
    public bool IsInitialized => _resultCode == (int)global::Android.App.Result.Ok;
    MediaProjectionCallback _mediaProjectionCallback;
    private readonly object _disposeLock = new object();
    private bool _disposed = false;

    public RecorderService()
    {
        _mediaProjectionCallback = new MediaProjectionCallback(this);
    }

    public void Init(global::Android.App.Result resultCode, Intent resultData)
    {
        _resultCode = (int)resultCode;
        _resultData = resultData;

        if (resultCode != global::Android.App.Result.Ok)
        {
            ServiceHelper.LogService?.LogDebug("RecorderService Init: Result not OK");
            return;
        }

        ServiceHelper.LogService?.LogDebug("RecorderService initialized");
        _ = StartRecording();
    }

    [System.Runtime.Versioning.SupportedOSPlatform("android31.0")]
    private void CreateMediaRecorderForApi31()
    {
        var activity = Platform.CurrentActivity;
        if (activity == null)
        {
            ServiceHelper.LogService?.LogDebug("CreateMediaRecorderForApi31 failed: CurrentActivity is null");
            throw new InvalidOperationException("CurrentActivity is null");
        }
        _mediaRecorder = new MediaRecorder(activity);
        _mediaRecorder.SetVideoSource(VideoSource.Surface);
        _mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
        _mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
        _mediaRecorder.SetVideoEncodingBitRate(10000000); // 10 Mbps
        _mediaRecorder.SetVideoFrameRate(30);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("android26.0")]
    [System.Runtime.Versioning.UnsupportedOSPlatform("android31.0")]
    private void CreateMediaRecorderLegacy()
    {
        _mediaRecorder = new MediaRecorder();
        _mediaRecorder.SetVideoSource(VideoSource.Surface);
        var profile = CamcorderProfile.Get(CamcorderQuality.High);
        _mediaRecorder.SetOutputFormat(profile.FileFormat);
        _mediaRecorder.SetVideoEncoder(profile.VideoCodec);
        _mediaRecorder.SetVideoEncodingBitRate(profile.VideoBitRate);
        _mediaRecorder.SetVideoFrameRate(profile.VideoFrameRate);
    }

    public async Task StartRecording()
    {
        if (_isRecording) return;

        try
        {
            // Check and request file access permission
            if (!OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                    if (status != PermissionStatus.Granted)
                    {
                        ServiceHelper.LogService?.LogDebug("RecorderService StartRecording failed: Storage write permission not granted");
                        return;
                    }
                }
            }

            // Get activity once at the beginning
            var activity = Platform.CurrentActivity;
            if (activity == null)
            {
                ServiceHelper.LogService?.LogDebug("RecorderService StartRecording failed: CurrentActivity is null");
                return;
            }

            // Get media projection manager
            var mediaProjectionManager = (MediaProjectionManager)activity.GetSystemService(Context.MediaProjectionService);

            // Request media projection permission if not initialized
            if (!IsInitialized)
            {
                try
                {
                    activity.StartActivityForResult(mediaProjectionManager.CreateScreenCaptureIntent(), REQUEST_MEDIA_PROJECTION);
                    ServiceHelper.LogService?.LogDebug("RecorderService requesting media projection permission");
                    return; // Exit and wait for Init to be called via OnActivityResult
                }
                catch (Exception ex)
                {
                    ServiceHelper.LogService?.LogDebug($"RecorderService StartRecording failed to request media projection: {ex.Message}");
                    ServiceHelper.LogService?.LogException(ex);
                    return;
                }
            }

            // TODO: support vertical somehow. Currently hardcoding to horizontal
            DisplayHelper.DisplayRotation = DisplayRotation.Rotation90;

            var physicalResolution = DisplayHelper.PhysicalResolution;
            var width = (int)physicalResolution.Width;
            var height = (int)physicalResolution.Height;
            var density = (int)DisplayHelper.DisplayInfo.Density;

            // Start foreground service before getting media projection
            // This is required on newer Android versions to keep MediaProjection alive
            Platform.AppContext.StartForegroundServiceCompat<RecorderForegroundService>();
            ServiceHelper.LogService?.LogDebug("RecorderService: Started RecorderForegroundService");

            // Get media projection
            _mediaProjection = mediaProjectionManager.GetMediaProjection(_resultCode, (Intent)_resultData.Clone());
            _mediaProjection.RegisterCallback(_mediaProjectionCallback, null);

            // Use appropriate API based on Android version
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                CreateMediaRecorderForApi31();
            }
            else
            {
                CreateMediaRecorderLegacy();
            }

            _mediaRecorder.SetVideoSize(width, height);     // weird resolutions will fail on prepare. ex: 1080x2350
            ServiceHelper.LogService?.LogDebug($"RecorderService: MediaRecorder video size set to {width}x{height}");

            var folder = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
            var file = System.IO.Path.Combine(folder, $"{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
            _mediaRecorder.SetOutputFile(file);
            ServiceHelper.LogService?.LogDebug($"RecorderService: Output file set to {file}");

            _mediaRecorder.Prepare();
            ServiceHelper.LogService?.LogDebug("RecorderService: MediaRecorder prepared");

            _mediaRecorder.Start();
            _recorderStarted = true; // Mark that Start() was successfully called
            ServiceHelper.LogService?.LogDebug("RecorderService: MediaRecorder started successfully");

            _virtualDisplay = _mediaProjection.CreateVirtualDisplay("ScreenRecord", width, height, density, (DisplayFlags)VirtualDisplayFlags.AutoMirror, _mediaRecorder.Surface, null, null);
            ServiceHelper.LogService?.LogDebug("RecorderService: VirtualDisplay created");

            _isRecording = true;

            // Android 14 does not allow reusing media projection tokens
            if (OperatingSystem.IsAndroidVersionAtLeast(34))
            {
                _resultCode = 0;
            }

            ServiceHelper.LogService?.LogDebug($"RecorderService recording started - MediaProjection valid: {_mediaProjection != null}, VirtualDisplay valid: {_virtualDisplay != null}");
        }
        catch (Exception ex)
        {
            ServiceHelper.LogService?.LogDebug($"RecorderService StartRecording Exception: {ex.Message}");
            ServiceHelper.LogService?.LogException(ex);
            StopRecording();
        }
    }

    public void StopRecording()
    {
        ServiceHelper.LogService?.LogDebug($"RecorderService StopRecording called: _isRecording={_isRecording}, _recorderStarted={_recorderStarted}, _mediaRecorder={(_mediaRecorder != null ? "not null" : "null")}, _isStoppingFromCallback={_isStoppingFromCallback}");

        if (!_isRecording)
        {
            ServiceHelper.LogService?.LogDebug("RecorderService StopRecording: Already stopped, returning");
            return;
        }

        _isRecording = false;

        // Clean up virtual display
        try
        {
            if (_virtualDisplay != null)
            {
                ServiceHelper.LogService?.LogDebug("RecorderService: Releasing virtual display");
                _virtualDisplay.Release();
                _virtualDisplay.Dispose();
                ServiceHelper.LogService?.LogDebug("RecorderService: Virtual display released successfully");
            }
        }
        catch (Exception ex)
        {
            ServiceHelper.LogService?.LogDebug($"RecorderService StopRecording: Error releasing virtual display - {ex.Message}");
        }
        finally
        {
            _virtualDisplay = null;
        }

        // Clean up media recorder - only call Stop() if Start() was successfully called
        if (_recorderStarted && _mediaRecorder != null)
        {
            try
            {
                ServiceHelper.LogService?.LogDebug("RecorderService: Stopping media recorder");
                _mediaRecorder.Stop();
                ServiceHelper.LogService?.LogDebug("RecorderService: Media recorder stopped successfully");
            }
            catch (Exception ex)
            {
                ServiceHelper.LogService?.LogDebug($"RecorderService StopRecording: Error stopping media recorder - {ex.Message}");
                ServiceHelper.LogService?.LogException(ex);
                // If Stop() fails, try to reset the recorder to a valid state
                try
                {
                    ServiceHelper.LogService?.LogDebug("RecorderService: Attempting to reset media recorder");
                    _mediaRecorder.Reset();
                    ServiceHelper.LogService?.LogDebug("RecorderService: Media recorder reset successfully");
                }
                catch (Exception resetEx)
                {
                    ServiceHelper.LogService?.LogDebug($"RecorderService StopRecording: Error resetting media recorder - {resetEx.Message}");
                    ServiceHelper.LogService?.LogException(resetEx);
                }
            }
            finally
            {
                _recorderStarted = false;
            }
        }
        else
        {
            ServiceHelper.LogService?.LogDebug($"RecorderService: Skipping Stop() call - _recorderStarted={_recorderStarted}, _mediaRecorder={(_mediaRecorder != null ? "not null" : "null")}");
        }

        try
        {
            if (_mediaRecorder != null)
            {
                ServiceHelper.LogService?.LogDebug("RecorderService: Releasing media recorder");
                _mediaRecorder.Release();
                _mediaRecorder.Dispose();
                ServiceHelper.LogService?.LogDebug("RecorderService: Media recorder released successfully");
            }
        }
        catch (Exception ex)
        {
            ServiceHelper.LogService?.LogDebug($"RecorderService StopRecording: Error releasing media recorder - {ex.Message}");
            ServiceHelper.LogService?.LogException(ex);
        }
        finally
        {
            _mediaRecorder = null;
        }

        // Clean up media projection
        // Unregister callback first to prevent recursive calls
        try
        {
            if (_mediaProjection != null && _mediaProjectionCallback != null)
            {
                ServiceHelper.LogService?.LogDebug("RecorderService: Unregistering MediaProjection callback");
                _mediaProjection.UnregisterCallback(_mediaProjectionCallback);
                ServiceHelper.LogService?.LogDebug("RecorderService: MediaProjection callback unregistered successfully");
            }
        }
        catch (Exception ex)
        {
            ServiceHelper.LogService?.LogDebug($"RecorderService StopRecording: Error unregistering callback - {ex.Message}");
            ServiceHelper.LogService?.LogException(ex);
        }

        // Only stop media projection if we're not being called from the callback
        if (!_isStoppingFromCallback)
        {
            try
            {
                if (_mediaProjection != null)
                {
                    ServiceHelper.LogService?.LogDebug("RecorderService: Stopping media projection");
                    _mediaProjection.Stop();
                    ServiceHelper.LogService?.LogDebug("RecorderService: Media projection stopped successfully");
                }
            }
            catch (Exception ex)
            {
                ServiceHelper.LogService?.LogDebug($"RecorderService StopRecording: Error stopping media projection - {ex.Message}");
                ServiceHelper.LogService?.LogException(ex);
            }
        }
        else
        {
            ServiceHelper.LogService?.LogDebug("RecorderService: Skipping media projection Stop() call - already stopped by callback");
        }

        try
        {
            if (_mediaProjection != null)
            {
                ServiceHelper.LogService?.LogDebug("RecorderService: Disposing media projection");
                _mediaProjection.Dispose();
                ServiceHelper.LogService?.LogDebug("RecorderService: Media projection disposed successfully");
            }
        }
        catch (Exception ex)
        {
            ServiceHelper.LogService?.LogDebug($"RecorderService StopRecording: Error disposing media projection - {ex.Message}");
            ServiceHelper.LogService?.LogException(ex);
        }
        finally
        {
            _mediaProjection = null;
            _isStoppingFromCallback = false; // Reset flag
        }

        // Stop the recorder foreground service
        try
        {
            var exitIntent = new Intent(Platform.AppContext, typeof(RecorderForegroundService));
            exitIntent.SetAction(RecorderForegroundService.EXIT_ACTION);
            Platform.AppContext.StartService(exitIntent);
            ServiceHelper.LogService?.LogDebug("RecorderService: Stopped RecorderForegroundService");
        }
        catch (Exception ex)
        {
            ServiceHelper.LogService?.LogDebug($"RecorderService: Error stopping RecorderForegroundService - {ex.Message}");
        }

        ServiceHelper.LogService?.LogDebug("RecorderService: Recording stopped completely");
    }

    private void CallbackStop()
    {
        ServiceHelper.LogService?.LogDebug($"RecorderService: MediaProjection callback stopped - Current state: _isRecording={_isRecording}, _recorderStarted={_recorderStarted}");
        _resultCode = 0;
        _isStoppingFromCallback = true; // Set flag to prevent calling Stop() on MediaProjection
        StopRecording();
    }

    private class MediaProjectionCallback : MediaProjection.Callback
    {
        RecorderService _recorderService;
        public MediaProjectionCallback(RecorderService recorderService)
        {
            _recorderService = recorderService;
        }
        public override void OnStop()
        {
            _recorderService?.CallbackStop();
        }
    }

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        lock (_disposeLock)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        // Stop recording if in progress
                        if (_isRecording)
                        {
                            try
                            {
                                StopRecording();
                            }
                            catch { }
                        }

                        // Clean up virtual display
                        try
                        {
                            _virtualDisplay?.Release();
                            _virtualDisplay?.Dispose();
                        }
                        catch { }
                        finally
                        {
                            _virtualDisplay = null;
                        }

                        // Clean up media recorder
                        try
                        {
                            if (_mediaRecorder != null)
                            {
                                _mediaRecorder.Release();
                                _mediaRecorder.Dispose();
                            }
                        }
                        catch { }
                        finally
                        {
                            _mediaRecorder = null;
                        }

                        // Clean up media projection
                        try
                        {
                            if (_mediaProjection != null)
                            {
                                _mediaProjection.UnregisterCallback(_mediaProjectionCallback);
                                _mediaProjection.Stop();
                                _mediaProjection.Dispose();
                            }
                        }
                        catch { }
                        finally
                        {
                            _mediaProjection = null;
                        }

                        // Clear callback reference
                        _mediaProjectionCallback = null;

                        // Clear intent data
                        _resultData?.Dispose();
                        _resultData = null;
                    }
                    catch (Exception ex)
                    {
                        ServiceHelper.LogService?.LogException(ex);
                    }
                }
                _disposed = true;
            }
        }
    }

    ~RecorderService()
    {
        Dispose(false);
    }

    #endregion
}
