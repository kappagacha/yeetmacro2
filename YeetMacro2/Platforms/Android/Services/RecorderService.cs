
using Android.Content;
using Android.Hardware.Display;
using Android.Media;
using Android.Media.Projection;
using Android.Views;
using Android.Widget;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Services;

// https://github.com/chinmoyp/screenrecorder/blob/master/app/src/main/java/com/confusedbox/screenrecorder/MainActivity.java
// https://github.com/android/media-samples/blob/main/ScreenCapture/Application/src/main/java/com/example/android/screencapture/ScreenCaptureFragment.java
// https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionRecording.kt
// https://github.com/Fate-Grand-Automata/FGA/blob/6d6b5f190817574f2d07f04f124b677c39b09634/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
public class RecorderService : IRecorderService
{
    MainActivity _context;
    MediaProjectionManager _mediaProjectionManager;
    MediaProjection _mediaProjection;
    MediaRecorder _mediaRecorder;
    VirtualDisplay _virtualDisplay;
    public const int REQUEST_SCREEN_RECORD = 2;
    Intent _resultData;
    int _resultCode;
    bool _isRecording;

    public RecorderService()
    {
        _context = (MainActivity)Platform.CurrentActivity;
        _mediaProjectionManager = (MediaProjectionManager)_context.GetSystemService(Context.MediaProjectionService);
    }

    public void Start()
    {
        if (_isRecording) return;

        _context.StartActivityForResult(_mediaProjectionManager.CreateScreenCaptureIntent(), YeetMacro2.Platforms.Android.Services.RecorderService.REQUEST_SCREEN_RECORD);
    }

    public void Start(global::Android.App.Result resultCode, Intent resultData)
    {
        if (resultCode != global::Android.App.Result.Ok)
        {
            Toast.MakeText(_context, "Screen record canceled...", ToastLength.Short).Show();
            return;
        }

        _resultCode = (int)resultCode;
        _resultData = resultData;
        _mediaProjection = _mediaProjectionManager.GetMediaProjection(_resultCode, _resultData);

        InitMediaRecorder();
        _isRecording = true;
        Toast.MakeText(_context, "Screen record started...", ToastLength.Short).Show();
    }

    private void InitMediaRecorder()
    {
        if (_mediaRecorder != null)
        {
            _mediaRecorder.Release();
            _mediaRecorder.Dispose();
        }

        if (_virtualDisplay != null)
        {
            _virtualDisplay.Release();
            _virtualDisplay.Dispose();
        }

        var displayInfo = DeviceDisplay.MainDisplayInfo;
        var width = (int)displayInfo.Width;
        var height = (int)displayInfo.Height;
        var density = (int)displayInfo.Density;

        _mediaRecorder = new MediaRecorder();

        var profile = CamcorderProfile.Get(CamcorderQuality.High);
        _mediaRecorder.SetVideoSource(VideoSource.Surface);
        _mediaRecorder.SetOutputFormat(profile.FileFormat);
        _mediaRecorder.SetVideoEncoder(profile.VideoCodec);
        _mediaRecorder.SetVideoEncodingBitRate(profile.VideoBitRate);
        _mediaRecorder.SetVideoFrameRate(profile.VideoFrameRate);
        _mediaRecorder.SetVideoSize(width, height);

        var folder = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
        var file = System.IO.Path.Combine(folder, $"{DateTime.Now.ToString("recording_yyyyMMdd_HHmmss")}.mp4");
        _mediaRecorder.SetOutputFile(file);
        _mediaRecorder.Prepare();
        _mediaRecorder.Start();
        _virtualDisplay = _mediaProjection.CreateVirtualDisplay("ScreenRecord", width, height, density, (DisplayFlags)VirtualDisplayFlags.AutoMirror, _mediaRecorder.Surface, null, null);
    }

    public void Stop()
    {
        if (_mediaRecorder == null || _isRecording == false) return;

        _mediaRecorder.Stop();
        _mediaRecorder.Reset();
        _virtualDisplay?.Release();
        Toast.MakeText(_context, "Screen record stopped...", ToastLength.Short).Show();
        _isRecording = false;
    }
}
