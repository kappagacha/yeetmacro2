using YeetMacro2.Data.Models;

namespace YeetMacro2.Services;
public interface IScreenService
{
    Task<MemoryStream> GetCurrentImageStream();
    Task<MemoryStream> GetCurrentImageStream(int x, int y, int width, int height);
    void DrawClear();
    Bounds TransformBounds(Bounds originalBounds, Resolution originalResolution);
    void DrawRectangle(int x, int y, int width, int height);
    void DrawCircle(int x, int y);
    Task<List<Point>> GetMatches(PatternBase template, FindOptions opts);
    void DoClick(float x, float y);
}
