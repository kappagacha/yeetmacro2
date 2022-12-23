using Android.Graphics;
using Color = Android.Graphics.Color;
using Point = Android.Graphics.Point;

namespace YeetMacro2.Platforms.Android.Services;
public static class BitmapHelper
{
    //https://stackoverflow.com/questions/11343772/find-subimage-in-larger-image-in-c-sharp#answer-11345850
    public static List<Point> SearchBitmap(Bitmap needleBitmap, Bitmap haystackBitmap, double tolerance)
    {
        var watch = new System.Diagnostics.Stopwatch();
        int haystackWidth = haystackBitmap.Width;
        int haystackHeight = haystackBitmap.Height;
        int needleWidth = needleBitmap.Width;
        int needleHeight = needleBitmap.Height;
        int movewidth = haystackWidth - needleWidth + 1;
        int moveheight = haystackHeight - needleHeight + 1;
        var points = new List<Point>();

        watch.Start();
        int[] haystackPixels = new int[haystackWidth * haystackHeight];
        haystackBitmap.GetPixels(haystackPixels, 0, haystackWidth, 0, 0, haystackWidth, haystackHeight);
        int[] needlePixels = new int[needleWidth * needleHeight];
        needleBitmap.GetPixels(needlePixels, 0, needleWidth, 0, 0, needleWidth, needleHeight);

        for (int startX = 0; startX < movewidth; startX++)
        {
            for (int startY = 0; startY < moveheight; startY++)
            {

                if (IsMatchingColor(haystackPixels[startY * haystackWidth + startY], needlePixels[0]))
                {
                    var matchFound = true;
                    for (int searchX = 0; searchX < needleWidth && matchFound; searchX++)
                    {
                        for (int searchY = 0; searchY < needleHeight && matchFound; searchY++)
                        {
                            var haytackColor = haystackPixels[(startY + searchY) * haystackWidth + (startX + searchX)];
                            var needleColor = needlePixels[searchY * haystackWidth + searchX];

                            if (!IsMatchingColor(haytackColor, needleColor))
                            {
                                matchFound = false;
                            }
                        }
                    }
                    if (matchFound)
                    {
                        points.Add(new Point((startX + needleWidth) / 2, (startY + needleHeight) / 2));
                    }
                }

                //GetPixel seems to be much slower than array usage
                //if (IsMatchingColor(haystackBitmap.GetPixel(startX, startY), needleBitmap.GetPixel(0, 0)))
                //{
                //    var matchFound = true;
                //    for (int searchX = 0; searchX < needleWidth && matchFound; searchX++)
                //    {
                //        for (int searchY = 0; searchY < needleHeight && matchFound; searchY++)
                //        {
                //            var haytackColor = haystackBitmap.GetPixel(searchX + startX, searchY + startY);
                //            var needleColor = needleBitmap.GetPixel(searchX, searchY);

                //            if (!IsMatchingColor(haytackColor, needleColor))
                //            {
                //                matchFound = false;
                //            }
                //        }
                //    }
                //    if (matchFound)
                //    {
                //        points.Add(new Point((startX + needleWidth) / 2, (startY + needleHeight) / 2));
                //    }
                //}
            }
        }

        watch.Stop();
        Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
        Console.WriteLine($"Count: {points.Count}");

        return points;
    }

    //https://stackoverflow.com/questions/9018016/how-to-compare-two-colors-for-similarity-difference#answer-58008400
    private static bool IsMatchingColor(int color1, int color2, int treshholdPct = 100)
    {
        var threadSold = 255 - (255 / 100f * treshholdPct);
        //var diffAlpha = Math.Abs(Color.GetAlphaComponent(color1) - Color.GetAlphaComponent(color2));
        var diffRed = Math.Abs(Color.GetRedComponent(color1) - Color.GetRedComponent(color2));
        var diffGreen = Math.Abs(Color.GetGreenComponent(color1) - Color.GetGreenComponent(color2));
        var diffBlue = Math.Abs(Color.GetBlueComponent(color1) - Color.GetBlueComponent(color2));

        //if (diffAlpha > threadSold || diffRed > threadSold || diffGreen > threadSold || diffBlue > threadSold)
        if (diffRed > threadSold || diffGreen > threadSold || diffBlue > threadSold)
        {
            return false;
        }
        return true;
    }
}
