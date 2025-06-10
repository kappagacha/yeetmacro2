using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;

public partial class TestViewModel : ObservableObject
{
    [ObservableProperty]
    bool _isBusy, _isImageViewTestRunning, _isImageViewWinking, _isOpenCVMatchTemplateTestRunning, _isOpenCVCalcColorThresholdTestRunning,
        _isMediaProjectionGetCurrentImageTestRunning, _isAccessibilityServiceDoClickTestRunning, _isFindTextTestRunning;
    [ObservableProperty]
    int _imageViewTestCount, _openCVMatchTemplateTestCount, _OpenCVCalcColorThresholdTestCount,
        _mediaProjectionGetCurrentImageTestCount, _accessibilityServiceDoClickTestCount, _findTextTestCount;
    [ObservableProperty]
    string _deviceDisplayResolution, _currentWindowMetrics, _resolution, _customResolution, _topLeft, _testMessage;
    readonly byte[] _haystackImage = Convert.FromBase64String("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"), 
           _needleImage = Convert.FromBase64String("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"),
           _textImage = Convert.FromBase64String("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");
    readonly ILogger _logger;
    readonly MediaProjectionService _mediaProjectionService;
    readonly YeetAccessibilityService _accessibilityService;
    readonly OpenCvService _openCvService;
    readonly AndroidScreenService _screenService;

    readonly Java.Lang.Reflect.Method dumpGREFTableMethod = Java.Lang.Class.ForName("dalvik.system.VMDebug").GetDeclaredMethod("dumpReferenceTables");
    readonly Java.Lang.Object[] args = [];

    public TestViewModel(ILogger<TestViewModel> logger, MediaProjectionService mediaProjectionService, YeetAccessibilityService accessibilityService,
        OpenCvService openCvService, AndroidScreenService screenService)
    {
        _logger = logger;
        _mediaProjectionService = mediaProjectionService;
        _accessibilityService = accessibilityService;
        _openCvService = openCvService;
        _screenService = screenService;

        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
    }

    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        CurrentWindowMetrics = String.Empty;
        ToggleCurrentWindowMetrics();
    }

    // https://stackoverflow.com/questions/46232314/trying-to-track-down-gref-leak
    [RelayCommand]
    public void DumpGlobalRefTable()
    {
        if (Permissions.RequestAsync<Permissions.StorageWrite>().Result != PermissionStatus.Granted) return;
        var grefFile = Path.Combine("/data/data", AppInfo.PackageName, "files/.__override__", "grefs.txt");
        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
        //var targetDirectory = DeviceInfo.Current.Platform == DevicePlatform.Android ?
        //    global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).AbsolutePath :
        //    FileSystem.Current.AppDataDirectory;
        var targetDirectory = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
        var fileDestination = Path.Combine(targetDirectory, "mygrefs.txt");
        if (File.Exists(grefFile))
        {
            var data = File.ReadAllBytes(grefFile);
            using FileStream fs = new(fileDestination, FileMode.Create);
            fs.Write(data, 0, data.Length);
            //File.Copy(grefFile, fileDestination, true);
        }

        //#if ANDROID
        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
        //var targetDirectory = DeviceInfo.Current.Platform == DevicePlatform.Android ?
        //    global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).AbsolutePath :
        //    FileSystem.Current.AppDataDirectory;
        //File.WriteAllText(Path.Combine(targetDirectory, "temp.txt"), "Hello therer");
        //#endif

        dumpGREFTableMethod.Invoke(null, args);
    }

    public void ToggleTest()
    {

    }

    [RelayCommand]
    public void ToggleImageViewTest()
    {
        if (IsImageViewTestRunning)
        {
            IsImageViewTestRunning = IsBusy = false;
            return;
        }

        if (IsBusy) return;

        IsImageViewTestRunning = IsBusy = true;
        DeviceDisplay.Current.KeepScreenOn = true;
        Task.Run(() =>
        {
            ImageViewTestCount = 0;
            while (IsImageViewTestRunning)
            {
                _logger.LogDebug(ImageViewTestCount.ToString());
                ImageViewTestCount++;
                IsImageViewWinking = !IsImageViewWinking;
                //await Task.Delay(500);
                new System.Threading.ManualResetEvent(false).WaitOne(100);
            }
        });
    }

    [RelayCommand]
    public void ToggleOpenCVMatchTemplateTest()
    {
        if (IsOpenCVMatchTemplateTestRunning)
        {
            IsOpenCVMatchTemplateTestRunning = IsBusy = false;
            return;
        }

        if (IsBusy) return;

        IsOpenCVMatchTemplateTestRunning = IsBusy = true;
        Task.Run(async () =>
        {
            OpenCVMatchTemplateTestCount = 0;
            while (IsOpenCVMatchTemplateTestRunning)
            {
                OpenCVMatchTemplateTestCount++;
                _logger.LogDebug(OpenCVMatchTemplateTestCount.ToString());
                var points = _openCvService.GetPointsWithMatchTemplate(_haystackImage, _needleImage);
                await Task.Delay(100);
            }
        });
    }

    [RelayCommand]
    public void ToggleOpenCVCalcColorThresholdTest()
    {
        if (IsOpenCVCalcColorThresholdTestRunning)
        {
            IsOpenCVCalcColorThresholdTestRunning = IsBusy = false;
            return;
        }

        if (IsBusy) return;

        IsOpenCVCalcColorThresholdTestRunning = IsBusy = true;
        DeviceDisplay.Current.KeepScreenOn = true;
        Task.Run(async () =>
        {
            OpenCVCalcColorThresholdTestCount = 0;
            while (IsOpenCVCalcColorThresholdTestRunning)
            {
                OpenCVCalcColorThresholdTestCount++;
                _logger.LogDebug(OpenCVCalcColorThresholdTestCount.ToString());
                var output = _openCvService.CalcColorThreshold(_haystackImage, new ColorThresholdProperties()
                {
                    Color = "#fffaf0e5"
                });
                await Task.Delay(100);
            }
        });
    }

    [RelayCommand]
    public void ToggleMediaProjectionGetCurrentImageTest()
    {
        if (IsMediaProjectionGetCurrentImageTestRunning)
        {
            IsMediaProjectionGetCurrentImageTestRunning = IsBusy = false;
            return;
        }

        if (IsBusy) return;

        IsMediaProjectionGetCurrentImageTestRunning = IsBusy = true;
        DeviceDisplay.Current.KeepScreenOn = true;
        Task.Run(async () =>
        {
            MediaProjectionGetCurrentImageTestCount = 0;
            while (IsMediaProjectionGetCurrentImageTestRunning)
            {
                try
                {
                    MediaProjectionGetCurrentImageTestCount++;
                    _logger.LogDebug(MediaProjectionGetCurrentImageTestCount.ToString());
                    var currentImage = _mediaProjectionService.GetCurrentImageData();
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                await Task.Delay(100);
            }
        });
    }

    [RelayCommand]
    public void ToggleAccessibilityServiceDoClickTest()
    {
        if (IsAccessibilityServiceDoClickTestRunning)
        {
            IsAccessibilityServiceDoClickTestRunning = IsBusy = false;
            return;
        }

        if (IsBusy) return;

        IsAccessibilityServiceDoClickTestRunning = IsBusy = true;
        DeviceDisplay.Current.KeepScreenOn = true;
        Task.Run(async () =>
        {
            AccessibilityServiceDoClickTestCount = 0;
            while (IsAccessibilityServiceDoClickTestRunning)
            {
                try
                {
                    AccessibilityServiceDoClickTestCount++;
                    _logger.LogDebug(AccessibilityServiceDoClickTestCount.ToString());
                    _accessibilityService.DoClick(new Point() { X = 100, Y = 100 });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                await Task.Delay(250);
            }
        });
    }

    [RelayCommand]
    public void ToggleFindTextTest()
    {
        if (IsFindTextTestRunning)
        {
            IsFindTextTestRunning = IsBusy = false;
            return;
        }

        if (IsBusy) return;

        IsFindTextTestRunning = IsBusy = true;
        DeviceDisplay.Current.KeepScreenOn = true;
        Task.Run(async () =>
        {
            FindTextTestCount = 0;
            while (IsFindTextTestRunning)
            {
                FindTextTestCount++;
                _logger.LogDebug(FindTextTestCount.ToString());
                var text = _screenService.FindText(_textImage);
                await Task.Delay(100);
            }
        });
    }

    [RelayCommand]
    public void ToggleDeviceDisplayResolution()
    {
        if (!String.IsNullOrEmpty(DeviceDisplayResolution))
        {
            DeviceDisplayResolution = String.Empty;
            return;
        }

        DeviceDisplayResolution = DeviceDisplay.MainDisplayInfo.ToString();
    }

    [RelayCommand]
    public void ToggleCurrentWindowMetrics()
    {
        if (!String.IsNullOrEmpty(CurrentWindowMetrics))
        {
            CurrentWindowMetrics = String.Empty;
            return;
        }

        var decorView = Platform.CurrentActivity?.Window?.DecorView;
        var insets = decorView?.RootWindowInsets;
        var cutout = insets?.DisplayCutout;

        if (cutout != null)
        {
            var displayInfo = DeviceDisplay.MainDisplayInfo;
            var rotation = displayInfo.Rotation;
            var rotation2 = Platform.CurrentActivity?.WindowManager?.DefaultDisplay?.Rotation;
            var rotation3 = Platform.CurrentActivity?.Display?.Rotation;
            var rotation4 = Platform.CurrentActivity?.Window?.DecorView?.Display?.Rotation;

            int top = 0, left = 0;
            int width = (int)displayInfo.Width;
            int height = (int)displayInfo.Height;

            switch (rotation)
            {
                case DisplayRotation.Rotation0:
                    top = cutout.SafeInsetTop;
                    left = cutout.SafeInsetLeft;
                    width -= cutout.SafeInsetLeft;
                    height -= cutout.SafeInsetTop;
                    break;

                case DisplayRotation.Rotation90:
                    top = cutout.SafeInsetRight;
                    left = cutout.SafeInsetTop;
                    width -= cutout.SafeInsetTop;
                    height -= cutout.SafeInsetRight;
                    break;

                case DisplayRotation.Rotation180:
                    top = cutout.SafeInsetBottom;
                    left = cutout.SafeInsetRight;
                    width -= cutout.SafeInsetRight;
                    height -= cutout.SafeInsetBottom;
                    break;

                case DisplayRotation.Rotation270:
                    top = cutout.SafeInsetLeft;
                    left = cutout.SafeInsetBottom;
                    width -= cutout.SafeInsetBottom;
                    height -= cutout.SafeInsetLeft;
                    break;
            }

            CurrentWindowMetrics =
                $"SafeInsetLeft: {cutout.SafeInsetLeft}\n" +
                $"SafeInsetTop: {cutout.SafeInsetTop}\n" +
                $"SafeInsetRight: {cutout.SafeInsetRight}\n" +
                $"SafeInsetBottom: {cutout.SafeInsetBottom}\n" +
                $"Rotation: {rotation}\n" +
                $"Rotation2: {rotation2}\n" +
                $"Rotation3: {rotation3}\n" +
                $"Rotation4: {rotation4}\n" +
                $"Top: {top}\nLeft: {left}\nWidth: {width}\nHeight: {height}\n";
        }
        else
        {
            CurrentWindowMetrics = "Cutout not resolved";
        }

        //var decorView = Platform.CurrentActivity?.Window?.DecorView;
        //var insets = decorView.RootWindowInsets;
        //var cutout = insets?.DisplayCutout;
        //if (cutout != null)
        //{
        //    CurrentWindowMetrics = $"SafeInsetLeft: {cutout.SafeInsetLeft}\nSafeInsetTop: {cutout.SafeInsetTop}\nSafeInsetRight: {cutout.SafeInsetRight}\nSafeInsetBottom: {cutout.SafeInsetBottom}\n";
        //    CurrentWindowMetrics += $"Rotation{DeviceDisplay.MainDisplayInfo.Rotation}";

        //    if (DeviceDisplay.MainDisplayInfo.Rotation == DisplayRotation.Rotation0)
        //    {
        //        CurrentWindowMetrics +=  $"Top: {cutout.SafeInsetTop}\nLeft: {cutout.SafeInsetLeft}\nWidth: {DeviceDisplay.MainDisplayInfo.Width - cutout.SafeInsetLeft}\nHeight: {DeviceDisplay.MainDisplayInfo.Height - cutout.SafeInsetTop}\n";
        //    }
        //    else if (DeviceDisplay.MainDisplayInfo.Rotation == DisplayRotation.Rotation90)
        //    {
        //        CurrentWindowMetrics += $"Top: {cutout.SafeInsetLeft}\nLeft: {cutout.SafeInsetBottom}\nWidth: {DeviceDisplay.MainDisplayInfo.Width - cutout.SafeInsetBottom}\nHeight: {DeviceDisplay.MainDisplayInfo.Height - cutout.SafeInsetLeft}\n";
        //    }
        //    else if (DeviceDisplay.MainDisplayInfo.Rotation == DisplayRotation.Rotation180)
        //    {
        //        CurrentWindowMetrics += $"Top: {cutout.SafeInsetBottom}\nLeft: {cutout.SafeInsetRight}\nWidth: {DeviceDisplay.MainDisplayInfo.Width - cutout.SafeInsetRight}\nHeight: {DeviceDisplay.MainDisplayInfo.Height - cutout.SafeInsetBottom}\n";
        //    }
        //    else if (DeviceDisplay.MainDisplayInfo.Rotation == DisplayRotation.Rotation270)
        //    {
        //        CurrentWindowMetrics += $"Top: {cutout.SafeInsetRight}\nLeft: {cutout.SafeInsetTop}\nWidth: {DeviceDisplay.MainDisplayInfo.Width - cutout.SafeInsetTop}\nHeight: {DeviceDisplay.MainDisplayInfo.Height - cutout.SafeInsetRight}\n";
        //    }
        //}
        //else
        //{
        //    CurrentWindowMetrics = "Cutout not resolved";
        //}


        //var windowManager = Platform.CurrentActivity.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        //var wm = windowManager.CurrentWindowMetrics;
        //CurrentWindowMetrics = $"Width: {wm.Bounds.Width()}\nHeight: {wm.Bounds.Height()}\nWidth: {wm.Bounds.Top}\nHeight: {wm.Bounds.Left}";
    }

    [RelayCommand]
    public void ToggleResolution()
    {
        if (!String.IsNullOrEmpty(Resolution))
        {
            Resolution = String.Empty;
            return;
        }

        Resolution = PatternHelper.CurrentResolution.ToString();
    }

    [RelayCommand]
    public void ToggleCustomResolution()
    {
        if (!String.IsNullOrEmpty(CustomResolution))
        {
            CustomResolution = String.Empty;
            return;
        }

        //var windowManager = Platform.CurrentActivity.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        //var dm = new DisplayMetrics();
        //windowManager.DefaultDisplay.GetRealMetrics(dm);
        //CustomResolution = $"Density: {dm.Density}\nScaledDensity: {dm.ScaledDensity}\nWidthPixels: {dm.WidthPixels}\nHeightPixels: {dm.HeightPixels}\nXdpi: {dm.Xdpi}\nYdpi: {dm.Ydpi}";

        //CustomResolution = _screenService.CurrentResolution.ToString();

        CustomResolution = PatternHelper.ScreenResolution.ToString();
    }

    [RelayCommand]
    public void ToggleTopLeft()
    {
        if (!String.IsNullOrEmpty(TopLeft))
        {
            TopLeft = String.Empty;
            return;
        }

        //TopLeft = _screenService.GetTopLeft().ToString();
    }

    [RelayCommand]
    public void ToggleTestMessage()
    {
        if (!String.IsNullOrEmpty(TestMessage))
        {
            TestMessage = String.Empty;
            return;
        }

        TestMessage = _screenService.TestMessage;
    }
}