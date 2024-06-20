using OpenCvSharp;
using SkiaSharp;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Platforms.Windows.Services;
public static class OpenCvHelper
{
    public static byte[] CalcColorThreshold(byte[] imageData, ColorThresholdProperties colorThreshold)
    {
        // https://github.com/shimat/opencvsharp/issues/173
        var mat = Cv2.ImDecode(imageData, ImreadModes.Color);

        // https://ckyrkou.medium.com/color-thresholding-in-opencv-91049607b06d
        // blue, red, green
        var skColorTarget = SKColor.Parse(colorThreshold.Color);
        var variance = 255 * colorThreshold.VariancePct / 100.0;
        var lowerBounds = new Scalar(skColorTarget.Blue - variance, skColorTarget.Green - variance, skColorTarget.Red - variance);
        var upperBounds = new Scalar(skColorTarget.Blue + variance, skColorTarget.Green + variance, skColorTarget.Red + variance);
        var mask = mat.InRange(lowerBounds, upperBounds);
        // https://forum.opencv.org/t/do-we-have-a-function-to-invert-gray-image-values/5902
        var maskInverted = new Scalar(255) - mask;
        
        //var maskRgb = mask.CvtColor(ColorConversionCodes.GRAY2BGR);
        //var result = mat & maskRgb;
        //return result.ToMat().ToBytes(".jpeg");

        return maskInverted.ToMat().ToBytes(".jpeg");
    }
}
