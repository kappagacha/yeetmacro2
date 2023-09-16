using TesseractOcrMaui;

namespace YeetMacro2.Services;

public interface IOcrService
{
    string GetText(byte[] imageData, string whiteList = null);
}

public class OcrService : IOcrService
{
    TessEngine _tessEngine;

    public OcrService()
    {
        var traineddataPath = Path.Combine(FileSystem.Current.CacheDirectory, "eng.traineddata");
        if (!File.Exists(traineddataPath))
        {
            var traineddata = ServiceHelper.GetAssetStream("eng.traineddata");
            FileStream fileStream = File.Create(traineddataPath);
            traineddata.CopyTo(fileStream);
        }

        _tessEngine = new TessEngine("eng", FileSystem.Current.CacheDirectory);
    }

    public string GetText(byte[] imageData, string whiteList = null)
    {
        var pix = Pix.LoadFromMemory(imageData);
        var page = _tessEngine.ProcessImage(pix);
        if (!String.IsNullOrWhiteSpace(whiteList)) _tessEngine.SetVariable("tessedit_char_whitelist", whiteList);
        var text = page.GetText();
        if (!String.IsNullOrWhiteSpace(whiteList)) _tessEngine.SetVariable("tessedit_char_whitelist", "");
        page.Dispose();

        return text;
    }
}
