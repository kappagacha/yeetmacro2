using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Windows.Services;
public class WindowsProjectionService : IMediaProjectionService
{
    public bool Enabled => throw new NotImplementedException();

    public Task<TBitmap> GetCurrentImageBitmap<TBitmap>(int x, int y, int width, int height)
    {
        throw new NotImplementedException();
    }

    public Task<TBitmap> GetCurrentImageBitmap<TBitmap>()
    {
        throw new NotImplementedException();
    }

    public Task<MemoryStream> GetCurrentImageStream()
    {
        throw new NotImplementedException();
    }

    public Task<MemoryStream> GetCurrentImageStream(int x, int y, int width, int height)
    {
        throw new NotImplementedException();
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}
