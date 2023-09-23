using YeetMacro2.Services;
using Tesseract.Droid;

namespace YeetMacro2.Platforms.Android.Services;
public class AndroidOcrService : IOcrService
{
    TesseractApi _tesseractApi;
    public AndroidOcrService()
    {
        Task.Run(async () =>
        {
            _tesseractApi = new TesseractApi(Platform.CurrentActivity, AssetsDeployment.OncePerVersion);
            await _tesseractApi.Init("eng");
        });
    }

    public string GetText(byte[] imageData, string whiteList = null)
    {
        if (_tesseractApi is null) return string.Empty;

        if (!String.IsNullOrWhiteSpace(whiteList)) _tesseractApi.SetWhitelist(whiteList);
        _ = _tesseractApi.SetImage(imageData).Result;
        if (!String.IsNullOrWhiteSpace(whiteList)) _tesseractApi.SetWhitelist("");

        return _tesseractApi.Text;
    }
}