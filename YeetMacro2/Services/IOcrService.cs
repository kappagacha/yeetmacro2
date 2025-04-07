namespace YeetMacro2.Services;

public interface IOcrService
{
    string FindText(byte[] imageData, string whiteList = null);
    Task<string> FindTextAsync(byte[] imageData, string whiteList = null);
}
