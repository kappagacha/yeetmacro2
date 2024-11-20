namespace YeetMacro2.Services;

public interface IOcrService
{
    string GetText(byte[] imageData, string whiteList = null);
    Task<string> GetTextAsync(byte[] imageData, string whiteList = null);
}
