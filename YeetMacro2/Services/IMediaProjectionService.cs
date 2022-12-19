namespace YeetMacro2.Services;
public interface IMediaProjectionService
{
    bool Enabled { get; }

    //Task<Bitmap> GetCurrentImageBitmap(int x, int y, int width, int height);
    //Task<Bitmap> GetCurrentImageBitmap();
    Task<MemoryStream> GetCurrentImageStream();
    Task<MemoryStream> GetCurrentImageStream(int x, int y, int width, int height);
    void Start();
    void Stop();
}
