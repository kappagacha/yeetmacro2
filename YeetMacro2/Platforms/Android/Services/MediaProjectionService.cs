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
using SecurityException = Java.Lang.SecurityException;

namespace YeetMacro2.Platforms.Android.Services;

//https://github.com/xamarin/monodroid-samples/blob/main/android5.0/ScreenCapture/ScreenCapture/ScreenCaptureFragment.cs
//https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
//https://medium.com/jamesob-com/recording-your-android-screen-7e0e75aae260
public class MediaProjectionService : IDisposable
{
    MediaProjection _mediaProjection;
    ImageReader _imageReader;
    VirtualDisplay _virtualDisplay;
    Intent _resultData;
    int _resultCode;
    public const int REQUEST_MEDIA_PROJECTION = 1;
    public bool IsInitialized => _resultCode == (int)global::Android.App.Result.Ok;
    MediaProjectionCallback _mediaProjectionCallback;
    private readonly object _disposeLock = new object();
    private bool _disposed = false;

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
            // Check if we have a valid projection token
            if (!IsInitialized)
            {
                ServiceHelper.LogService?.LogDebug("MediaProjectionService Start failed: No valid projection token");
                _resultCode = 0;  // Clear any invalid token
                WeakReferenceMessenger.Default.Send("MediaProjectionTokenExpired", "MediaProjectionToken");
                return;
            }

            // TODO: support vertical somehow. Currently hardcoding to horizontal
            DisplayHelper.DisplayRotation = DisplayRotation.Rotation90;

            var physicalResolution = DisplayHelper.PhysicalResolution;
            var width = (int)physicalResolution.Width;
            var height = (int)physicalResolution.Height;
            var density = (int)DisplayHelper.DisplayInfo.Density;

            // https://github.com/Fate-Grand-Automata/FGA/blob/2a62ab7a456a9913cf0355db81b5a15f13906f27/app/src/main/java/io/github/fate_grand_automata/runner/ScreenshotServiceHolder.kt#L53
            var activity = Platform.CurrentActivity;
            if (activity == null)
            {
                ServiceHelper.LogService?.LogDebug("MediaProjectionService Start failed: CurrentActivity is null");
                Stop();
                return;
            }

            var mediaProjectionManager = (MediaProjectionManager)activity.GetSystemService(Context.MediaProjectionService);
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
        catch (SecurityException secEx)
        {
            // Token is likely expired or invalid
            ServiceHelper.LogService?.LogDebug($"MediaProjectionService Start SecurityException: Token expired or invalid - {secEx.Message}");
            _resultCode = 0;  // Clear the invalid token
            Stop();
            WeakReferenceMessenger.Default.Send("MediaProjectionTokenExpired", "MediaProjectionToken");
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

        if (Platform.CurrentActivity != null)
        {
            Toast.MakeText(Platform.CurrentActivity, "Media projection initialized...", ToastLength.Short).Show();
        }

        // Start foreground service - it will detect we have media projection and use the correct type
        // On Android 35+, the service will only start if MediaProjection is initialized
        Platform.AppContext.StartForegroundServiceCompat<ForegroundService>();
    }

    private Bitmap GetBitmap()
    {
        //https://www.tabnine.com/code/java/classes/android.media.Image?snippet=5ce69622e594670004ac3235
        lock (_disposeLock)
        {
            if (_disposed || _imageReader == null) return null;
            
            try
            {
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
            catch (Exception ex)
            {
                ServiceHelper.LogService?.LogException(ex);
                return null;
            }
        }
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

        MemoryStream ms = null;
        try
        {
            ms = new MemoryStream();
            bitmap.Compress(CompressFormat.Jpeg, 100, ms);
            ms.Position = 0;
            return ms.ToArray();
        }
        finally
        {
            bitmap?.Dispose();
            ms?.Close();
            ms?.Dispose();
        }
    }

    public byte[] GetCurrentImageData(Rect rect)
    {
        var bitmap = GetBitmap();
        if (bitmap == null) return null;
        
        // Ensure rect is within bitmap bounds
        if (rect.X < 0) rect.X = 0;
        if (rect.Y < 0) rect.Y = 0;
        if (rect.Right > bitmap.Width) rect.Right = bitmap.Width;
        if (rect.Bottom > bitmap.Height) rect.Bottom = bitmap.Height;

        Bitmap newBitmap = null;
        MemoryStream ms = null;
        try
        {
            newBitmap = Bitmap.CreateBitmap(bitmap, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            ms = new MemoryStream();
            newBitmap.Compress(CompressFormat.Jpeg, 100, ms);
            ms.Position = 0;
            return ms.ToArray();
        }
        finally
        {
            bitmap?.Dispose();
            newBitmap?.Dispose();
            ms?.Close();
            ms?.Dispose();
        }
    }

    public void TakeScreenCapture()
    {
        try
        {
            var imageData = GetCurrentImageData();
            if (imageData == null || imageData.Length == 0)
            {
                ServiceHelper.LogService?.LogDebug("TakeScreenCapture: No image data available");
                return;
            }
            
            var folder = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
            var file = System.IO.Path.Combine(folder, $"{DateTime.Now:screencapture_yyyyMMdd_HHmmss}.jpeg");
            using FileStream fs = new(file, FileMode.OpenOrCreate);
            fs.Write(imageData, 0, imageData.Length);
        }
        catch (Exception ex)
        {
            ServiceHelper.LogService?.LogException(ex);
        }
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
            _mediaProjectionService?.CallbackStop();
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

                        // Clean up image reader
                        try
                        {
                            _imageReader?.Close();
                            _imageReader?.Dispose();
                        }
                        catch { }
                        finally
                        {
                            _imageReader = null;
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

    ~MediaProjectionService()
    {
        Dispose(false);
    }

    #endregion
}