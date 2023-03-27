using YeetMacro2.Data.Models;

namespace YeetMacro2.Services;
public interface IScreenService
{
    void DrawClear();
    void DrawRectangle(int x, int y, int width, int height);
    void DrawCircle(int x, int y);
    void DebugRectangle(int x, int y, int width, int height);
    void DebugCircle(int x, int y);
    void DebugClear();
    Task<List<Point>> GetMatches(Pattern template, FindOptions opts);
    void DoClick(float x, float y);
    Byte[] CalcColorThreshold(Pattern pattern, ColorThresholdProperties colorThreshold);
    Task<byte[]> GetCurrentImageData(int x, int y, int w, int h);
    Task<string> GetText(Pattern pattern);
}
