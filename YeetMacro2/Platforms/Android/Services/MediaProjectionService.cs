using Android.Content;
using Android.Graphics;
using Android.Hardware.Display;
using Android.Media.Projection;
using Android.Media;
using Android.Views;
using Android.Widget;
using static Android.Graphics.Bitmap;
using YeetMacro2.Services;
namespace YeetMacro2.Platforms.Android.Services;

//https://github.com/xamarin/monodroid-samples/blob/main/android5.0/ScreenCapture/ScreenCapture/ScreenCaptureFragment.cs
//https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
//https://medium.com/jamesob-com/recording-your-android-screen-7e0e75aae260
public class MediaProjectionService : IRecorderService
{
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
    bool _isRecording;
    public bool Enabled { get => _virtualDisplay != null; }

    public MediaProjectionService()
    {
        Console.WriteLine("[*****YeetMacro*****] MediaProjectionService Constructor Start");
        Init();
        Console.WriteLine("[*****YeetMacro*****] MediaProjectionService Constructor End");
    }

    private void Init()
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] MediaProjectionService Init");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] MediaProjectionService Init Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }
    }

    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        try
        {
            Console.WriteLine("[*****YeetMacro*****] MediaProjectionService DeviceDisplay_MainDisplayInfoChanged");
            InitVirtualDisplay();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[*****YeetMacro*****] MediaProjectionService DeviceDisplay_MainDisplayInfoChanged Exception");
            Console.WriteLine("[*****YeetMacro*****] " + ex.Message);
        }
    }

    public void Start()
    {
        _startCompleted = new TaskCompletionSource<bool>();
    }

    public void Start(global::Android.App.Result resultCode, Intent resultData)
    {
        if (resultCode != global::Android.App.Result.Ok)
        {
            Toast.MakeText(_context, "Media projection canceled...", ToastLength.Short).Show();
            _startCompleted.SetResult(false);
            return;
        }

        _context = (MainActivity)Platform.CurrentActivity;
        _mediaProjectionManager = (MediaProjectionManager)_context.GetSystemService(Context.MediaProjectionService);

        _resultCode = (int)resultCode;
        _resultData = resultData;
        InitVirtualDisplay();
        _startCompleted.SetResult(true);
        _mediaProjection = _mediaProjectionManager.GetMediaProjection(_resultCode, _resultData);

        var displayInfo = DeviceDisplay.MainDisplayInfo;
        var width = (int)displayInfo.Width;
        var height = (int)displayInfo.Height;
        var density = (int)displayInfo.Density;

        _imageReader = ImageReader.NewInstance(width, height, (ImageFormatType)global::Android.Graphics.Format.Rgba8888, 2);
        _virtualDisplay = _mediaProjection.CreateVirtualDisplay("ScreenCapture", width, height, density, (DisplayFlags)VirtualDisplayFlags.AutoMirror, _imageReader.Surface, null, null);

        Toast.MakeText(_context, "Media projection started...", ToastLength.Short).Show();
        //DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
    }

    private void InitVirtualDisplay()
    {
        Stop();
        //https://stackoverflow.com/questions/38891654/get-current-screen-width-in-xamarin-forms

    }

    //It may be faster if we don't convert to bitmap
    //public byte[] GetCurrentImageByteArray()
    //{
    //    var image = _imageReader.AcquireLatestImage();
    //    var plane = image.GetPlanes()[0];
    //    var buffer = plane.Buffer;
    //    byte[] bytes = new byte[buffer.Capacity()];
    //    buffer.Get(bytes);

    //    return bytes;
    //}

    private async Task EnsureProjectionServiceStarted()
    {
        if (_resultCode == 0)
        {
            Start();
            await _startCompleted.Task;
            await Task.Delay(500);      //give projection service time to start
        }
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

    public async Task<MemoryStream> GetCurrentImageStream()
    {
        await EnsureProjectionServiceStarted();

        var bitmap = GetBitmap();
        if (bitmap == null) return null;

        MemoryStream ms = new MemoryStream();
        bitmap.Compress(CompressFormat.Jpeg, 100, ms);
        bitmap.Dispose();
        ms.Position = 0;
        return ms;
    }

    public async Task<MemoryStream> GetCurrentImageStream(int x, int y, int width, int height)
    {
        if (x < 0) x = 0;
        if (y < 0) y = 0;
        await EnsureProjectionServiceStarted();

        var bitmap = GetBitmap();
        if (bitmap == null) return null;

        var newBitmap = Bitmap.CreateBitmap(bitmap, x, y, width, height);
        bitmap.Dispose();
        MemoryStream ms = new MemoryStream();
        newBitmap.Compress(CompressFormat.Jpeg, 100, ms);
        newBitmap.Dispose();
        ms.Position = 0;
        return ms;
    }

    public void Stop()
    {
        DeviceDisplay.MainDisplayInfoChanged -= DeviceDisplay_MainDisplayInfoChanged;

        if (_mediaProjection != null)
        {
            _mediaProjection.Stop();
            _mediaProjection.Dispose();
            _mediaProjection = null;
        }
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
        
        if (_context != null)
        {
            Toast.MakeText(_context, "Media projection stopped...", ToastLength.Short).Show();
        }
    }

    public async Task<TBitmap> GetCurrentImageBitmap<TBitmap>(int x, int y, int width, int height)
    {
        if (typeof(TBitmap) != typeof(Bitmap))
        {
            throw new Exception("Type not supported");
        }
        if (x < 0) x = 0;
        if (y < 0) y = 0;
        await EnsureProjectionServiceStarted();

        var bitmap = GetBitmap();
        if (bitmap == null) return default(TBitmap);

        var newBitmap = Bitmap.CreateBitmap(bitmap, x, y, width, height);
        bitmap.Dispose();
        MemoryStream ms = new MemoryStream();
        newBitmap.Compress(CompressFormat.Jpeg, 100, ms);
        newBitmap.Dispose();
        ms.Position = 0;
        var decoded = BitmapFactory.DecodeStream(ms);
        return (TBitmap)(Object)decoded;
    }

    public async Task<TBitmap> GetCurrentImageBitmap<TBitmap>()
    {
        if (typeof(TBitmap) != typeof(Bitmap))
        {
            throw new Exception("Type not supported");
        }

        await EnsureProjectionServiceStarted();

        var bitmap = GetBitmap();
        if (bitmap == null) return default(TBitmap);

        MemoryStream ms = new MemoryStream();
        bitmap.Compress(CompressFormat.Jpeg, 100, ms);
        bitmap.Dispose();
        ms.Position = 0;
        var decoded = BitmapFactory.DecodeStream(ms);
        return (TBitmap)(Object)decoded;
    }

    public async Task TakeScreenCapture()
    {
        var ms = await GetCurrentImageStream();
        if (ms == null) return;
        byte[] bArray = new byte[ms.Length];

        var folder = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
        var file = System.IO.Path.Combine(folder, $"{DateTime.Now.ToString("screencapture_yyyyMMdd_HHmmss")}.jpeg");
        using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
        {
            using (ms)
            {
                ms.Read(bArray, 0, (int)ms.Length);
            }
            int length = bArray.Length;
            fs.Write(bArray, 0, length);
        }
    }

    // https://github.com/chinmoyp/screenrecorder/blob/master/app/src/main/java/com/confusedbox/screenrecorder/MainActivity.java
    // https://github.com/android/media-samples/blob/main/ScreenCapture/Application/src/main/java/com/example/android/screencapture/ScreenCaptureFragment.java
    // https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionRecording.kt
    // https://github.com/Fate-Grand-Automata/FGA/blob/6d6b5f190817574f2d07f04f124b677c39b09634/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/MediaProjectionScreenshotService.kt
    public void StartRecording()
    {
        if (_isRecording) return;

        var displayInfo = DeviceDisplay.MainDisplayInfo;
        var width = (int)displayInfo.Width;
        var height = (int)displayInfo.Height;
        var density = (int)displayInfo.Density;
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

        _mediaRecorder.Stop();
        _mediaRecorder.Release();
        _mediaRecorder.Dispose();
        _screenVirtualDisplay.Release();
        _screenVirtualDisplay.Dispose();
        _isRecording = false;
    }
}