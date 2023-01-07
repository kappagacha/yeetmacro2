using OpenCV.Core;
using static OpenCV.Core.Core;
using Rect = OpenCV.Core.Rect;
using Point = Microsoft.Maui.Graphics.Point;

namespace YeetMacro2.Platforms.Android.Services.OpenCv;
public static class OpenCvHelper
{
    public static List<Point> GetPointsWithMatchTemplate(global::Android.Graphics.Bitmap haystackBitmap, global::Android.Graphics.Bitmap needleBitmap, int limit = 1, double threshold = 0.8)
    {
        try
        {
            //https://answers.opencv.org/question/52722/what-is-the-correct-way-to-convert-a-mat-to-a-bitmap/
            var haystackMat = new Mat();
            var needleMat = new Mat();

            OpenCV.Android.Utils.BitmapToMat(haystackBitmap, haystackMat);
            OpenCV.Android.Utils.BitmapToMat(needleBitmap, needleMat);
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

    //var resultBitmap = XamarinApp.Droid.OpenCv.Utils.GetBitmapWithMatchTemplate(imageBitmap, templateBitmap);
    //MemoryStream ms = new MemoryStream();
    //resultBitmap.Compress(CompressFormat.Jpeg, 100, ms);
    //ms.Position = 0;
    //await _fileService.SavePicture("testImage.jpeg", ms);
    public static global::Android.Graphics.Bitmap GetBitmapWithMatchTemplate(global::Android.Graphics.Bitmap haystackBitmap, global::Android.Graphics.Bitmap needleBitmap, int limit = 1, double threshold = 0.8)
    {
        //https://answers.opencv.org/question/52722/what-is-the-correct-way-to-convert-a-mat-to-a-bitmap/
        var haystackMat = new Mat();
        var needleMat = new Mat();
        OpenCV.Android.Utils.BitmapToMat(haystackBitmap, haystackMat);
        OpenCV.Android.Utils.BitmapToMat(needleBitmap, needleMat);

        return GetBitmapWithMatchTemplate(haystackMat, needleMat, limit, threshold);
    }

    private static global::Android.Graphics.Bitmap GetBitmapWithMatchTemplate(Mat haystackMat, Mat needleMat, int limit, double threshold)
    {
        MatchTemplate(haystackMat, needleMat, limit, threshold, (result) =>
        {
            //Draw rectangle
            //Rect r = new Rect(new Point(result.MaxLoc.X, result.MaxLoc.Y), new Size(needleMat.Width(), needleMat.Height()));
            Rect r = new Rect((int)result.MaxLoc.X, (int)result.MaxLoc.Y, needleMat.Width(), needleMat.Height());
            OpenCV.ImgProc.Imgproc.Rectangle(haystackMat, r, new Scalar(0, 0, 0), 2);
        });

        var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(haystackMat.Width(), haystackMat.Height(), global::Android.Graphics.Bitmap.Config.Argb8888);
        OpenCV.Android.Utils.MatToBitmap(haystackMat, bitmap);

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

            OpenCV.ImgProc.Imgproc.MatchTemplate(image, template, result, (int)TemplateMatchModes.CCoeffNormed);
            OpenCV.ImgProc.Imgproc.Threshold(result, result, 0.8, 1.0, (int)ThresholdTypes.Tozero);

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
                OpenCV.ImgProc.Imgproc.FloodFill(result, mask, location.MaxLoc, new Scalar(0, 0, 0), outRect, new Scalar(floodFillDiff), new Scalar(floodFillDiff));
                outRect.Dispose();
            }
        }

        mask.Dispose();

        watch.Stop();
        Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
    }
}
