namespace YeetMacro2.Services;
public interface IMediaProjectionService
{
    bool Enabled { get; }

    Task<TBitmap> GetCurrentImageBitmap<TBitmap>(int x, int y, int width, int height);
    Task<TBitmap> GetCurrentImageBitmap<TBitmap>();
    Task<MemoryStream> GetCurrentImageStream();
    Task<MemoryStream> GetCurrentImageStream(int x, int y, int width, int height);
    void Start();
    void Stop();
}
