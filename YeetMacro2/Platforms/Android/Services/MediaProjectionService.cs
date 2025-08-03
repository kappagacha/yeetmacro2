using Android.Content;
using Android.Graphics;
using Android.Hardware.Display;
using Android.Media.Projection;
using Android.Media;
using Android.Views;
using Android.Widget;
using static Android.Graphics.Bitmap;
using YeetMacro2.Services;
using Rect = Microsoft.Maui.Graphics.Rect;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.ViewModels;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Platforms.Android.Services;

//https://github.com/xamarin/monodroid-samples/blob/main/android5.0/ScreenCapture/ScreenCapture/ScreenCaptureFragment.cs
//https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
//https://medium.com/jamesob-com/recording-your-android-screen-7e0e75aae260
public class MediaProjectionService : IRecorderService
{
    MediaProjection _mediaProjection;
    ImageReader _imageReader;
    VirtualDisplay _virtualDisplay, _screenVirtualDisplay;
    MediaRecorder _mediaRecorder;
    Intent _resultData;
    int _resultCode;
    public const int REQUEST_MEDIA_PROJECTION = 1;
    bool _isRecording;
    public bool IsInitialized => _resultCode == (int)global::Android.App.Result.Ok;
    MediaProjectionCallback _mediaProjectionCallback;

    public MediaProjectionService()
    {
        _mediaProjectionCallback = new MediaProjectionCallback(this);
    }

    public void Start()
    {
        if (_virtualDisplay is not null)
        {
            return;
        }

        try
        {
            // TODO: support vertical somehow. Currently hardcoding to horizontal
            DisplayHelper.DisplayRotation = DisplayRotation.Rotation90;

            var physicalResolution = DisplayHelper.PhysicalResolution;
            var width = (int)physicalResolution.Width;
            var height = (int)physicalResolution.Height;
            var density = (int)DisplayHelper.DisplayInfo.Density;

            // https://github.com/Fate-Grand-Automata/FGA/blob/2a62ab7a456a9913cf0355db81b5a15f13906f27/app/src/main/java/io/github/fate_grand_automata/runner/ScreenshotServiceHolder.kt#L53
            var mediaProjectionManager = (MediaProjectionManager)Platform.CurrentActivity.GetSystemService(Context.MediaProjectionService);
            _mediaProjection = mediaProjectionManager.GetMediaProjection(_resultCode, (Intent)_resultData.Clone());
            _mediaProjection.RegisterCallback(_mediaProjectionCallback, null);
            _imageReader = ImageReader.NewInstance(width, height, (ImageFormatType)global::Android.Graphics.Format.Rgba8888, 2);
            _virtualDisplay = _mediaProjection.CreateVirtualDisplay("ScreenCapture", width, height, density, (DisplayFlags)VirtualDisplayFlags.AutoMirror, _imageReader.Surface, null, null);

            // Android 14 does not allow reusing media projection tokens
            if (OperatingSystem.IsAndroidVersionAtLeast(34))
            {
                _resultCode = 0;
            }
        }
        catch (Exception ex)
        {
            ServiceHelper.GetService<LogServiceViewModel>().LogDebug($"MediaProjectionService Start Exception: {ex.Message}");
            ServiceHelper.GetService<LogServiceViewModel>().LogException(ex);

            Stop();
            WeakReferenceMessenger.Default.Send(this);
        }
    }

    public void Init(global::Android.App.Result resultCode, Intent resultData)
    {
        _resultCode = (int)resultCode;
        _resultData = resultData;

        if (resultCode != global::Android.App.Result.Ok)
        {
            if (Platform.CurrentActivity != null)
            {
                Toast.MakeText(Platform.CurrentActivity, "Media projection canceled...", ToastLength.Short).Show();
            }
            return;
        }

        Toast.MakeText(Platform.CurrentActivity, "Media projection initialized...", ToastLength.Short).Show();
        Platform.AppContext.StartForegroundServiceCompat<ForegroundService>();
    }

    private Bitmap GetBitmap()
    {
        //https://www.tabnine.com/code/java/classes/android.media.Image?snippet=5ce69622e594670004ac3235
        var image = _imageReader.AcquireLatestImage();
        if (image == null) return null;
        var plane = image.GetPlanes()[0];
        var buffer = plane.Buffer;
        var pixelStride = plane.PixelStride;
        var rowStride = plane.RowStride;
        var rowPadding = rowStride - pixelStride * image.Width;
        var bitmap = Bitmap.CreateBitmap(image.Width + rowPadding / pixelStride, image.Height, Bitmap.Config.Argb8888); //Bitmap.Config.ARGB_8888
        bitmap.CopyPixelsFromBuffer(buffer);

        buffer.Dispose();
        plane.Dispose();
        image.Close();
        image.Dispose();

        return bitmap;
    }

    public void CallbackStop()
    {
        if (_virtualDisplay == null) return;

        _resultCode = 0;
        Stop();
    }

    public void Stop()
    {
        if (_virtualDisplay == null) return;

        _virtualDisplay.Release();
        _virtualDisplay = null;
        _imageReader.Close();
        _imageReader = null;
        _mediaProjection.Stop();
        _mediaProjection = null;
        //if (Platform.CurrentActivity != null)
        //{
        //    Toast.MakeText(Platform.CurrentActivity, "Media projection stopped...", ToastLength.Short).Show();
        //}
    }

    public byte[] GetCurrentImageData()
    {
        var bitmap = GetBitmap();
        if (bitmap == null) return null;

        MemoryStream ms = new();
        bitmap.Compress(CompressFormat.Jpeg, 100, ms);
        bitmap.Dispose();
        ms.Position = 0;
        var array = ms.ToArray();
        ms.Close();
        ms.Dispose();
        return array;
    }

    public byte[] GetCurrentImageData(Rect rect)
    {

        var bitmap = GetBitmap();
        if (rect.X < 0) rect.X = 0;
        if (rect.Y < 0) rect.Y = 0;
        if (rect.Right > bitmap.Width) rect.Right = bitmap.Width;
        if (rect.Bottom > bitmap.Height) rect.Bottom = bitmap.Height;
        if (bitmap == null) return null;

        var newBitmap = Bitmap.CreateBitmap(bitmap, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        bitmap.Dispose();
        MemoryStream ms = new();
        newBitmap.Compress(CompressFormat.Jpeg, 100, ms);
        newBitmap.Dispose();
        ms.Position = 0;
        var array = ms.ToArray();
        ms.Close();
        ms.Dispose();
        return array;
    }

    public void TakeScreenCapture()
    {
        var imageData = GetCurrentImageData();
        var folder = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
        var file = System.IO.Path.Combine(folder, $"{DateTime.Now:screencapture_yyyyMMdd_HHmmss}.jpeg");
        using FileStream fs = new(file, FileMode.OpenOrCreate);
        fs.Write(imageData, 0, imageData.Length);
    }

    // https://github.com/chinmoyp/screenrecorder/blob/master/app/src/main/java/com/confusedbox/screenrecorder/MainActivity.java
    // https://github.com/android/media-samples/blob/main/ScreenCapture/Application/src/main/java/com/example/android/screencapture/ScreenCaptureFragment.java
    // https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionRecording.kt
    // https://github.com/Fate-Grand-Automata/FGA/blob/6d6b5f190817574f2d07f04f124b677c39b09634/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
    public void StartRecording()
    {
        if (_isRecording) return;

        var screenResolution = DisplayHelper.PhysicalResolution;
        var width = (int)screenResolution.Width;
        var height = (int)screenResolution.Height;
        var density = (int)DisplayHelper.DisplayInfo.Density;
        var profile = CamcorderProfile.Get(CamcorderQuality.High);

        _mediaRecorder = new MediaRecorder();
        _mediaRecorder.SetVideoSource(VideoSource.Surface);
        _mediaRecorder.SetOutputFormat(profile.FileFormat);
        _mediaRecorder.SetVideoEncoder(profile.VideoCodec);
        _mediaRecorder.SetVideoEncodingBitRate(profile.VideoBitRate);
        _mediaRecorder.SetVideoFrameRate(profile.VideoFrameRate);
        _mediaRecorder.SetVideoSize(width, height);     // weird resolutions will fail on prepare. ex: 1080x2350
        
        var folder = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
        var file = System.IO.Path.Combine(folder, $"{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
        _mediaRecorder.SetOutputFile(file);
        _mediaRecorder.Prepare();
        _mediaRecorder.Start();
        _screenVirtualDisplay = _mediaProjection.CreateVirtualDisplay("ScreenRecord", width, height, density, (DisplayFlags)VirtualDisplayFlags.AutoMirror, _mediaRecorder.Surface, null, null);
        _isRecording = true;
    }
    public void StopRecording()
    {
        if (!_isRecording) return;

        _isRecording = false;
        _screenVirtualDisplay.Release();
        _screenVirtualDisplay.Dispose();
        _mediaRecorder.Stop();
        _mediaRecorder.Release();
        _mediaRecorder.Dispose();
        Stop();
    }

    private class MediaProjectionCallback : MediaProjection.Callback
    {
        MediaProjectionService _mediaProjectionService;
        public MediaProjectionCallback(MediaProjectionService mediaProjectionService)
        {
            _mediaProjectionService = mediaProjectionService;
        }
        public override void OnStop()
        {
            _mediaProjectionService.CallbackStop();
        }
    }
}