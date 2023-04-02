//using OpenCV.Core;
//using static OpenCV.Core.Core;
//using Rect = OpenCV.Core.Rect;
using Org.Opencv.Core;
using static Org.Opencv.Core.Core;
using Rect = Org.Opencv.Core.Rect;
using Point = Microsoft.Maui.Graphics.Point;
using YeetMacro2.Data.Models;
using SkiaSharp;
using Scalar = Org.Opencv.Core.Scalar;
using Mat = Org.Opencv.Core.Mat;

namespace YeetMacro2.Platforms.Android.Services.OpenCv;
public static class OpenCvHelper
{
    public static byte[] CalcColorThreshold(byte[] imageData, ColorThresholdProperties colorThreshold)
    {
        // https://stackoverflow.com/questions/21113190/how-to-get-the-mat-object-from-the-byte-in-opencv-android
        var matOfByte = new MatOfByte(imageData);
        var mat = Org.Opencv.Imgcodecs.Imgcodecs.Imdecode(matOfByte, Org.Opencv.Imgcodecs.Imgcodecs.CvLoadImageColor);

        // https://ckyrkou.medium.com/color-thresholding-in-opencv-91049607b06d
        // blue, red, green
        var skColorTarget = SKColor.Parse(colorThreshold.Color);
        var variance = 255 * colorThreshold.VariancePct / 100.0;
        var lowerBounds = new Scalar(skColorTarget.Blue - variance, skColorTarget.Green - variance, skColorTarget.Red - variance);
        var upperBounds = new Scalar(skColorTarget.Blue + variance, skColorTarget.Green + variance, skColorTarget.Red + variance);
        var mask = new Mat();
        Core.InRange(mat, lowerBounds, upperBounds, mask);
        var maskInverted = new Mat();
        Core.Bitwise_not(mask, maskInverted);
        var maskInvertedMatOfByte = new MatOfByte();
        Org.Opencv.Imgcodecs.Imgcodecs.Imencode(".jpeg", maskInverted, maskInvertedMatOfByte);
        var result = maskInvertedMatOfByte.ToArray();

        mat.Dispose();
        matOfByte.Dispose();
        lowerBounds.Dispose();
        upperBounds.Dispose();
        mask.Dispose();
        maskInverted.Dispose();
        maskInvertedMatOfByte.Dispose();

        return result;
    }

    public static List<Point> GetPointsWithMatchTemplate(byte[] haystackImageData, byte[] needleImageData, int limit = 1, double threshold = 0.8)
    {
        try
        {
            var haystackMatOfByte = new MatOfByte(haystackImageData);
            var needleMatOfByte = new MatOfByte(needleImageData);
            var haystackMat = Org.Opencv.Imgcodecs.Imgcodecs.Imdecode(haystackMatOfByte, Org.Opencv.Imgcodecs.Imgcodecs.CvLoadImageColor);
            var needleMat = Org.Opencv.Imgcodecs.Imgcodecs.Imdecode(needleMatOfByte, Org.Opencv.Imgcodecs.Imgcodecs.CvLoadImageColor);
            haystackMatOfByte.Dispose();
            needleMatOfByte.Dispose();
            return GetPointsWithMatchTemplate(haystackMat, needleMat, limit, threshold);
        }
        catch (Exception ex)
        {
            return new List<Point>();
        }
    }

    public static List<Point> GetPointsWithMatchTemplate(global::Android.Graphics.Bitmap haystackBitmap, global::Android.Graphics.Bitmap needleBitmap, int limit = 1, double threshold = 0.8)
    {
        try
        {
            //https://answers.opencv.org/question/52722/what-is-the-correct-way-to-convert-a-mat-to-a-bitmap/
            var haystackMat = new Mat();
            var needleMat = new Mat();

            Org.Opencv.Android.Utils.BitmapToMat(haystackBitmap, haystackMat);
            Org.Opencv.Android.Utils.BitmapToMat(needleBitmap, needleMat);
            //OpenCV.Android.Utils.BitmapToMat(haystackBitmap, haystackMat);
            //OpenCV.Android.Utils.BitmapToMat(needleBitmap, needleMat);
            return GetPointsWithMatchTemplate(haystackMat, needleMat, limit, threshold);

            //var haystackMatGray = new Mat();
            //var needleMatGray = new Mat();
            //OpenCV.ImgProc.Imgproc.CvtColor(haystackMat, haystackMatGray, OpenCV.ImgProc.Imgproc.ColorBgr2gray);
            //OpenCV.ImgProc.Imgproc.CvtColor(needleMat, needleMatGray, OpenCV.ImgProc.Imgproc.ColorBgr2gray);
            //haystackMat.Dispose();
            //needleMat.Dispose();
            //return GetPointsWithMatchTemplate(haystackMatGray, needleMatGray, limit, threshold);
        }
        catch (Exception ex)
        {
            return new List<Point>();
        }
    }

    private static List<Point> GetPointsWithMatchTemplate(Mat haystackMat, Mat needleMat, int limit, double threshold)
    {
        var matches = new List<Point>();

        MatchTemplate(haystackMat, needleMat, limit, threshold, (result) =>
        {
            //matches.Add(new Android.Graphics.PointF((float)(result.MaxLoc.X + template.Width() / 2), (float)(result.MaxLoc.Y + template.Height() / 2)));
            //matches.Add(((float)(result.MaxLoc.X + template.Width() / 2), (float)(result.MaxLoc.Y + template.Height() / 2)));
            matches.Add(new Point((int)(result.MaxLoc.X + needleMat.Width() / 2), (int)(result.MaxLoc.Y + needleMat.Height() / 2)));
        });

        haystackMat.Dispose();
        needleMat.Dispose();

        return matches;
    }

    public static global::Android.Graphics.Bitmap GetBitmapWithMatchTemplate(global::Android.Graphics.Bitmap haystackBitmap, global::Android.Graphics.Bitmap needleBitmap, int limit = 1, double threshold = 0.8)
    {
        //https://answers.opencv.org/question/52722/what-is-the-correct-way-to-convert-a-mat-to-a-bitmap/
        var haystackMat = new Mat();
        var needleMat = new Mat();
        Org.Opencv.Android.Utils.BitmapToMat(haystackBitmap, haystackMat);
        Org.Opencv.Android.Utils.BitmapToMat(needleBitmap, needleMat);

        //OpenCV.Android.Utils.BitmapToMat(haystackBitmap, haystackMat);
        //OpenCV.Android.Utils.BitmapToMat(needleBitmap, needleMat);

        return GetBitmapWithMatchTemplate(haystackMat, needleMat, limit, threshold);
    }

    private static global::Android.Graphics.Bitmap GetBitmapWithMatchTemplate(Mat haystackMat, Mat needleMat, int limit, double threshold)
    {
        MatchTemplate(haystackMat, needleMat, limit, threshold, (result) =>
        {
            //Draw rectangle
            //Rect r = new Rect(new Point(result.MaxLoc.X, result.MaxLoc.Y), new Size(needleMat.Width(), needleMat.Height()));
            Rect r = new Rect((int)result.MaxLoc.X, (int)result.MaxLoc.Y, needleMat.Width(), needleMat.Height());
            Org.Opencv.Imgproc.Imgproc.Rectangle(haystackMat, new Org.Opencv.Core.Point(result.MaxLoc.X, result.MaxLoc.Y), new Org.Opencv.Core.Point(result.MaxLoc.X + needleMat.Width(), result.MaxLoc.Y + needleMat.Height()), new Scalar(0, 0, 0));
            //OpenCV.ImgProc.Imgproc.Rectangle(haystackMat, r, new Scalar(0, 0, 0), 2);
        });

        var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(haystackMat.Width(), haystackMat.Height(), global::Android.Graphics.Bitmap.Config.Argb8888);
        Org.Opencv.Android.Utils.MatToBitmap(haystackMat, bitmap);
        //OpenCV.Android.Utils.MatToBitmap(haystackMat, bitmap);

        return bitmap;
    }

    //https://stackoverflow.com/questions/32737420/multiple-results-in-opencvsharp3-matchtemplate
    //https://github.com/Fate-Grand-Automata/FGA/blob/master/app/src/main/java/com/mathewsachin/fategrandautomata/imaging/DroidCvPattern.kt
    private static void MatchTemplate(Mat image, Mat template, int limit, double threshold, Action<MinMaxLocResult> resultAction)
    {
        var watch = new System.Diagnostics.Stopwatch();
        var mask = new Mat();

        using (var result = new Mat(image.Rows() - template.Rows() + 1, image.Cols() - template.Cols() + 1, MatType.CV_32FC1))
        {
            watch.Start();

            Org.Opencv.Imgproc.Imgproc.MatchTemplate(image, template, result, Org.Opencv.Imgproc.Imgproc.TmCcoeffNormed);
            Org.Opencv.Imgproc.Imgproc.Threshold(result, result, 0.8, 1.0, Org.Opencv.Imgproc.Imgproc.ThreshTozero);
            //OpenCV.ImgProc.Imgproc.MatchTemplate(image, template, result, (int)TemplateMatchModes.CCoeffNormed);
            //OpenCV.ImgProc.Imgproc.Threshold(result, result, 0.8, 1.0, (int)ThresholdTypes.Tozero);

            var count = 0;
            while (count < limit)
            {
                var location = MinMaxLoc(result);

                if (location.MaxVal < threshold)
                    break;

                count++;
                resultAction(location);

                var floodFillDiff = 0.3;        //flood fill avoid searching the same area
                Rect outRect = new Rect();
                
                Org.Opencv.Imgproc.Imgproc.FloodFill(result, mask, location.MaxLoc, new Scalar(0, 0, 0), outRect, new Scalar(floodFillDiff), new Scalar(floodFillDiff), Org.Opencv.Imgproc.Imgproc.FloodfillFixedRange);
                //OpenCV.ImgProc.Imgproc.FloodFill(result, mask, location.MaxLoc, new Scalar(0, 0, 0), outRect, new Scalar(floodFillDiff), new Scalar(floodFillDiff));
                outRect.Dispose();
            }
        }

        mask.Dispose();

        watch.Stop();
        Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
    }
}

/// <summary>
/// https://github.com/shimat/opencvsharp/blob/master/src/OpenCvSharp/Modules/core/Struct/MatType.cs
/// Matrix data type (depth and number of channels)
/// </summary>
public readonly struct MatType : IEquatable<MatType>, IEquatable<int>
{
    /// <summary>
    /// Entity value
    /// </summary>
    private readonly int value;

    /// <summary>
    /// Entity value
    /// </summary>
    public int Value => value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public MatType(int value)
    {
        this.value = value;
    }

    /// <summary> 
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static implicit operator int(MatType self)
    {
        return self.value;
    }

    /// <summary> 
    /// </summary>
    /// <returns></returns>
    public int ToInt32()
    {
        return value;
    }

    /// <summary> 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator MatType(int value)
    {
        return new MatType(value);
    }

    /// <summary> 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static MatType FromInt32(int value)
    {
        return new MatType(value);
    }

    /// <summary>
    /// 
    /// </summary>
    public int Depth => value & CV_DEPTH_MAX - 1;

    /// <summary>
    /// 
    /// </summary>
    public bool IsInteger => Depth < CV_32F;

    /// <summary>
    /// 
    /// </summary>
    public int Channels => (Value >> CV_CN_SHIFT) + 1;

    public bool Equals(MatType other)
    {
        return value == other.value;
    }

    public bool Equals(int other)
    {
        return value == other;
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        if (obj.GetType() != typeof(MatType))
            return false;
        return obj is MatType mt && Equals(mt);
    }

    public static bool operator ==(MatType self, MatType other)
    {
        return self.Equals(other);
    }

    public static bool operator !=(MatType self, MatType other)
    {
        return !self.Equals(other);
    }

    public static bool operator ==(MatType self, int other)
    {
        return self.Equals(other);
    }

    public static bool operator !=(MatType self, int other)
    {
        return !self.Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        string s;
        switch (Depth)
        {
            case CV_8U:
                s = "CV_8U";
                break;
            case CV_8S:
                s = "CV_8S";
                break;
            case CV_16U:
                s = "CV_16U";
                break;
            case CV_16S:
                s = "CV_16S";
                break;
            case CV_32S:
                s = "CV_32S";
                break;
            case CV_32F:
                s = "CV_32F";
                break;
            case CV_64F:
                s = "CV_64F";
                break;
            case CV_USRTYPE1:
                s = "CV_USRTYPE1";
                break;
            default:
                return $"Unsupported type value ({Value})";
        }

        var ch = Channels;
        if (ch <= 4)
            return s + "C" + ch;
        else
            return s + "C(" + ch + ")";
    }

    private const int CV_CN_MAX = 512,
        CV_CN_SHIFT = 3,
        CV_DEPTH_MAX = 1 << CV_CN_SHIFT;

    /// <summary>
    /// type depth constants
    /// </summary>
    public const int
        CV_8U = 0,
        CV_8S = 1,
        CV_16U = 2,
        CV_16S = 3,
        CV_32S = 4,
        CV_32F = 5,
        CV_64F = 6,
        CV_USRTYPE1 = 7;

    /// <summary>
    /// predefined type constants
    /// </summary>
    public static readonly MatType
        CV_8UC1 = CV_8UC(1),
        CV_8UC2 = CV_8UC(2),
        CV_8UC3 = CV_8UC(3),
        CV_8UC4 = CV_8UC(4),
        CV_8SC1 = CV_8SC(1),
        CV_8SC2 = CV_8SC(2),
        CV_8SC3 = CV_8SC(3),
        CV_8SC4 = CV_8SC(4),
        CV_16UC1 = CV_16UC(1),
        CV_16UC2 = CV_16UC(2),
        CV_16UC3 = CV_16UC(3),
        CV_16UC4 = CV_16UC(4),
        CV_16SC1 = CV_16SC(1),
        CV_16SC2 = CV_16SC(2),
        CV_16SC3 = CV_16SC(3),
        CV_16SC4 = CV_16SC(4),
        CV_32SC1 = CV_32SC(1),
        CV_32SC2 = CV_32SC(2),
        CV_32SC3 = CV_32SC(3),
        CV_32SC4 = CV_32SC(4),
        CV_32FC1 = CV_32FC(1),
        CV_32FC2 = CV_32FC(2),
        CV_32FC3 = CV_32FC(3),
        CV_32FC4 = CV_32FC(4),
        CV_64FC1 = CV_64FC(1),
        CV_64FC2 = CV_64FC(2),
        CV_64FC3 = CV_64FC(3),
        CV_64FC4 = CV_64FC(4);
    /*
    public const int 
        CV_8UC1 = 0,
        CV_8SC1 = 1,
        CV_16UC1 = 2,
        CV_16SC1 = 3,
        CV_32SC1 = 4,
        CV_32FC1 = 5,
        CV_64FC1 = 6,
        CV_8UC2 = 8,
        CV_8SC2 = 9,
        CV_16UC2 = 10,
        CV_16SC2 = 11,
        CV_32SC2 = 12,
        CV_32FC2 = 13,
        CV_64FC2 = 14,
        CV_8UC3 = 16,
        CV_8SC3 = 17,
        CV_16UC3 = 18,
        CV_16SC3 = 19,
        CV_32SC3 = 20,
        CV_32FC3 = 21,
        CV_64FC3 = 22,
        CV_8UC4 = 24,
        CV_8SC4 = 25,
        CV_16UC4 = 26,
        CV_16SC4 = 27,
        CV_32SC4 = 28,
        CV_32FC4 = 29,
        CV_64FC4 = 30,
        CV_8UC5 = 32,
        CV_8SC5 = 33,
        CV_16UC5 = 34,
        CV_16SC5 = 35,
        CV_32SC5 = 36,
        CV_32FC5 = 37,
        CV_64FC5 = 38,
        CV_8UC6 = 40,
        CV_8SC6 = 41,
        CV_16UC6 = 42,
        CV_16SC6 = 43,
        CV_32SC6 = 44,
        CV_32FC6 = 45,
        CV_64FC6 = 46;
     */

    public static MatType CV_8UC(int ch)
    {
        return MakeType(CV_8U, ch);
    }

    public static MatType CV_8SC(int ch)
    {
        return MakeType(CV_8S, ch);
    }

    public static MatType CV_16UC(int ch)
    {
        return MakeType(CV_16U, ch);
    }

    public static MatType CV_16SC(int ch)
    {
        return MakeType(CV_16S, ch);
    }

    public static MatType CV_32SC(int ch)
    {
        return MakeType(CV_32S, ch);
    }

    public static MatType CV_32FC(int ch)
    {
        return MakeType(CV_32F, ch);
    }

    public static MatType CV_64FC(int ch)
    {
        return MakeType(CV_64F, ch);
    }

    public static MatType MakeType(int depth, int channels)
    {
        if (channels <= 0 || channels >= CV_CN_MAX)
            throw new Exception("Channels count should be 1.." + (CV_CN_MAX - 1));
        if (depth < 0 || depth >= CV_DEPTH_MAX)
            throw new Exception("Data type depth should be 0.." + (CV_DEPTH_MAX - 1));
        return (depth & CV_DEPTH_MAX - 1) + (channels - 1 << CV_CN_SHIFT);
    }
}