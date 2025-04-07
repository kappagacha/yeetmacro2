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

    public string FindText(byte[] imageData, string whiteList = null)
    {
        //var folder = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).Path;
        //var haystackFile = System.IO.Path.Combine(folder, $"ocr_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.jpeg");
        //using (FileStream fs = new FileStream(haystackFile, FileMode.OpenOrCreate))
        //{
        //    fs.Write(imageData, 0, imageData.Length);
        //}

        if (_tesseractApi is null) return string.Empty;

        if (!String.IsNullOrWhiteSpace(whiteList)) _tesseractApi.SetWhitelist(whiteList);
        _ = _tesseractApi.SetImage(imageData).Result;
        if (!String.IsNullOrWhiteSpace(whiteList)) _tesseractApi.SetWhitelist("");

        return _tesseractApi.Text;
    }

    public async Task<string> FindTextAsync(byte[] imageData, string whiteList = null)
    {
        if (_tesseractApi is null) return string.Empty;

        if (!String.IsNullOrWhiteSpace(whiteList)) _tesseractApi.SetWhitelist(whiteList);
        await _tesseractApi.SetImage(imageData);
        if (!String.IsNullOrWhiteSpace(whiteList)) _tesseractApi.SetWhitelist("");

        return _tesseractApi.Text;
    }
}

