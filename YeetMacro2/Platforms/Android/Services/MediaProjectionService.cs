﻿using Android.Content;
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
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace YeetMacro2.Platforms.Android.Services;

//https://github.com/xamarin/monodroid-samples/blob/main/android5.0/ScreenCapture/ScreenCapture/ScreenCaptureFragment.cs
//https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
//https://medium.com/jamesob-com/recording-your-android-screen-7e0e75aae260
public class MediaProjectionService : IRecorderService
{
    readonly ILogger _logger;
    MainActivity _context;
    MediaProjectionManager _mediaProjectionManager;
    MediaProjection _mediaProjection;
    ImageReader _imageReader;
    VirtualDisplay _virtualDisplay, _screenVirtualDisplay;
    MediaRecorder _mediaRecorder;
    Intent _resultData;
    int _resultCode;
    public const int REQUEST_MEDIA_PROJECTION = 1;
    bool _isRecording;
    public bool IsInitialized => _resultCode == (int)global::Android.App.Result.Ok;

    public MediaProjectionService()
    {
        _context = (MainActivity)Platform.CurrentActivity;
        _mediaProjectionManager = (MediaProjectionManager)_context.GetSystemService(Context.MediaProjectionService);
        _logger = ServiceHelper.GetService<ILogger<MediaProjectionService>>();

        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<bool>, string>(this, nameof(ForegroundService), (r, propertyChangedMessage) =>
        {
            if (propertyChangedMessage.NewValue)
            {
                _context.StartActivityForResult(_mediaProjectionManager.CreateScreenCaptureIntent(), Services.MediaProjectionService.REQUEST_MEDIA_PROJECTION);
            }
            else
            {
                if (_isRecording) StopRecording();
                else Stop();
            }
        });
    }

    public void Start()
    {
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
        _resultCode = (int)resultCode;
        _resultData = resultData;

        if (resultCode != global::Android.App.Result.Ok)
        {
            if (_context != null)
            {
                Toast.MakeText(_context, "Media projection canceled...", ToastLength.Short).Show();
            }
            WeakReferenceMessenger.Default.Send(this);
            return;
        }

        Toast.MakeText(_context, "Media projection initialized...", ToastLength.Short).Show();
        WeakReferenceMessenger.Default.Send(this);
    }

    // https://stackoverflow.com/questions/18760252/timeout-an-async-method-implemented-with-taskcompletionsource
    public async Task<TResult> TimeoutAfter<TResult>(Task<TResult> task, TimeSpan timeout)
    {
        using (var timeoutCancellationTokenSource = new CancellationTokenSource())
        {
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task;  // Very important in order to propagate exceptions
            }
            else
            {
                throw new TimeoutException($"{nameof(TimeoutAfter)}: The operation has timed out after {timeout:mm\\:ss}");
            }
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
        _resultCode = 0;
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
        WeakReferenceMessenger.Default.Send(this);
        if (_context != null)
        {
            Toast.MakeText(_context, "Media projection stopped...", ToastLength.Short).Show();
        }
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
        if (rect.X < 0) rect.X = 0;
        if (rect.Y < 0) rect.Y = 0;

        var bitmap = GetBitmap();
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
        if (_imageReader is null)
        {
            Start();
            Thread.Sleep(1_000);
        }

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
}