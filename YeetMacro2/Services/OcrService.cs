using TesseractOcrMaui;

namespace YeetMacro2.Services;

public interface IOcrService
{
    string GetText(byte[] imageData, string whiteList = null);
    Task<string> GetTextAsync(byte[] imageData, string whiteList = null);
}

public class OcrService : IOcrService
{
    readonly TessEngine _tessEngine;

    public OcrService()
    {
        var traineddataPath = Path.Combine(FileSystem.Current.CacheDirectory, "eng.traineddata");
        if (!File.Exists(traineddataPath))
        {
            var traineddata = ServiceHelper.GetAssetStream("eng.traineddata");
            FileStream fileStream = File.Create(traineddataPath);
            traineddata.CopyTo(fileStream);
        }

        _tessEngine = new TessEngine("eng", FileSystem.Current.CacheDirectory)
        {
            DefaultSegmentationMode = TesseractOcrMaui.Enums.PageSegmentationMode.SingleLine
        };
    }

    public string GetText(byte[] imageData, string whiteList = null)
    {
        try
        {
            //var pix = Pix.LoadFromMemory(imageData);
            // Work around since physical device fails with Pix.LoadFromMemory 
            // see https://github.com/henrivain/TesseractOcrMaui/issues/17
            var targetFile = Path.Combine(FileSystem.AppDataDirectory, "temp.jpeg");
            File.WriteAllBytes(targetFile, imageData);

            //#if ANDROID
            //        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
            //        var targetDirectory = DeviceInfo.Current.Platform == DevicePlatform.Android ?
            //            Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath :
            //            FileSystem.Current.AppDataDirectory;
            //        File.WriteAllBytes(Path.Combine(targetDirectory, "temp.jpeg"), imageData);
            //#endif
            var pix = Pix.LoadFromFile(targetFile);
            var page = _tessEngine.ProcessImage(pix);
            if (!String.IsNullOrWhiteSpace(whiteList)) _tessEngine.SetVariable("tessedit_char_whitelist", whiteList);
            var text = page.GetText();
            if (!String.IsNullOrWhiteSpace(whiteList)) _tessEngine.SetVariable("tessedit_char_whitelist", "");
            page.Dispose();

            return text.TrimEnd('\n');
        }
        catch (Exception)
        {
            return String.Empty;
        }
    }

    public Task<string> GetTextAsync(byte[] imageData, string whiteList = null)
    {
        return Task.FromResult(GetText(imageData, whiteList));
    }
}
