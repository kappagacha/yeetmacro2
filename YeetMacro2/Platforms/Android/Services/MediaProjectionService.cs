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
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace YeetMacro2.Platforms.Android.Services;

//https://github.com/xamarin/monodroid-samples/blob/main/android5.0/ScreenCapture/ScreenCapture/ScreenCaptureFragment.cs
//https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
//https://medium.com/jamesob-com/recording-your-android-screen-7e0e75aae260
public class MediaProjectionService : IRecorderService
{
    ILogger _logger;
    MainActivity _context;
    MediaProjectionManager _mediaProjectionManager;
    MediaProjection _mediaProjection;
    ImageReader _imageReader;
    VirtualDisplay _virtualDisplay, _screenVirtualDisplay;
    MediaRecorder _mediaRecorder;
    Intent _resultData;
    int _resultCode;
    public const int REQUEST_MEDIA_PROJECTION = 1;
    TaskCompletionSource<bool> _startCompleted;
    bool _isRecording, _isInitialized;

    public MediaProjectionService()
    {
        _startCompleted = new TaskCompletionSource<bool>();
        _logger = ServiceHelper.GetService<ILogger<MediaProjectionService>>();

        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, string>(this, nameof(ForegroundService), (r, propertyChangedMessage) =>
        {
            if (!propertyChangedMessage.NewValue)
            {
                Stop();
                StopRecording();
            }
        });
    }

    public void Start()
    {
        if (!_isInitialized) return;

        try
        {
            _logger.LogTrace("MediaProjectionService Start");

            var screenService = ServiceHelper.GetService<AndroidScreenService>();
            var currentResolution = screenService.CalcResolution;
            var width = (int)screenService.CalcResolution.Width;
            var height = (int)screenService.CalcResolution.Height;
            var density = (int)DeviceDisplay.MainDisplayInfo.Density;

            // https://github.com/Fate-Grand-Automata/FGA/blob/2a62ab7a456a9913cf0355db81b5a15f13906f27/app/src/main/java/io/github/fate_grand_automata/runner/ScreenshotServiceHolder.kt#L53
            _mediaProjection = _mediaProjectionManager.GetMediaProjection(_resultCode, (Intent)_resultData.Clone());
            _imageReader = ImageReader.NewInstance(width, height, (ImageFormatType)global::Android.Graphics.Format.Rgba8888, 2);
            _virtualDisplay = _mediaProjection.CreateVirtualDisplay("ScreenCapture", width, height, density, (DisplayFlags)VirtualDisplayFlags.AutoMirror, _imageReader.Surface, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MediaProjectionService Init Exception");
        }
    }

    public void Init(global::Android.App.Result resultCode, Intent resultData)
    {
        if (resultCode != global::Android.App.Result.Ok)
        {
            if (_context != null)
            {
                Toast.MakeText(_context, "Media projection canceled...", ToastLength.Short).Show();
            }
            _startCompleted.SetResult(false);
            return;
        }

        _context = (MainActivity)Platform.CurrentActivity;
        _resultCode = (int)resultCode;
        _resultData = resultData;
        _mediaProjectionManager = (MediaProjectionManager)_context.GetSystemService(Context.MediaProjectionService);
        _startCompleted.SetResult(true);
        _isInitialized = true;
        // ForegroundService can start before Media projection approval so we send this message to invoke showing ActionView
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<bool>(this, nameof(ForegroundService.OnStartCommand), false, true), nameof(ForegroundService));
        Toast.MakeText(_context, "Media projection initialized...", ToastLength.Short).Show();
    }

    public async Task EnsureProjectionServiceStarted()
    {
        if (_resultCode == 0)
        {
            await _startCompleted.Task;
            await Task.Delay(500);      //give projection service time to start
        }
    }

    private Bitmap GetBitmap()
    {
        if (_imageReader is null)
        {
            Start();
            Thread.Sleep(1_000);
        }

        _logger.LogTrace("GetBitmap");
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

    public void Stop()
    {
        if (_imageReader != null)
        {
            _imageReader.Close();
            _imageReader.Dispose();
            _imageReader = null;
        }
        if (_virtualDisplay != null)
        {
            _virtualDisplay.Release();
            _virtualDisplay.Dispose();
            _virtualDisplay = null;
        }

        if (_mediaProjection != null)
        {
            _mediaProjection.Stop();
            _mediaProjection.Dispose();
            _mediaProjection = null;
        }
    }

    public byte[] GetCurrentImageData()
    {
        var bitmap = GetBitmap();
        if (bitmap == null) return null;

        MemoryStream ms = new MemoryStream();
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
        if (rect.X < 0) rect.X = 0;
        if (rect.Y < 0) rect.Y = 0;

        var bitmap = GetBitmap();
        if (bitmap == null) return null;

        var newBitmap = Bitmap.CreateBitmap(bitmap, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        bitmap.Dispose();
        MemoryStream ms = new MemoryStream();
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
        var file = System.IO.Path.Combine(folder, $"{DateTime.Now.ToString("screencapture_yyyyMMdd_HHmmss")}.jpeg");
        using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
        {
            fs.Write(imageData, 0, imageData.Length);
        }
    }

    // https://github.com/chinmoyp/screenrecorder/blob/master/app/src/main/java/com/confusedbox/screenrecorder/MainActivity.java
    // https://github.com/android/media-samples/blob/main/ScreenCapture/Application/src/main/java/com/example/android/screencapture/ScreenCaptureFragment.java
    // https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionRecording.kt
    // https://github.com/Fate-Grand-Automata/FGA/blob/6d6b5f190817574f2d07f04f124b677c39b09634/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
    public void StartRecording()
    {
        if (_isRecording) return;

        Start();
        var screenService = ServiceHelper.GetService<AndroidScreenService>();
        var width = (int)screenService.CalcResolution.Width;
        var height = (int)screenService.CalcResolution.Height;
        var density = (int)DeviceDisplay.MainDisplayInfo.Density;
        var profile = CamcorderProfile.Get(CamcorderQuality.High);

        _mediaRecorder = new MediaRecorder();
        _mediaRecorder.SetVideoSource(VideoSource.Surface);
        _mediaRecorder.SetOutputFormat(profile.FileFormat);
        _mediaRecorder.SetVideoEncoder(profile.VideoCodec);
        _mediaRecorder.SetVideoEncodingBitRate(profile.VideoBitRate);
        _mediaRecorder.SetVideoFrameRate(profile.VideoFrameRate);
        _mediaRecorder.SetVideoSize(width, height);     // weird resolutions will fail on prepare. ex: 1080x2350
        
        var folder = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
        var file = System.IO.Path.Combine(folder, $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.mp4");
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
}