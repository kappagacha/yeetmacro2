using YeetMacro2.Data.Models;

namespace YeetMacro2.Services;

public class FindPatternResult
{
    public bool IsSuccess { get; set; }
    public string Path { get; set; }
    public Point Point { get; set; }
    public Point[] Points { get; set; }
}

public class FindOptions
{
    public int Limit { get; set; }
    public double VariancePct { get; set; }
}

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
    Task<FindPatternResult> ClickPattern(Pattern pattern);
    Task<FindPatternResult> FindPattern(Pattern pattern, FindOptions opts);
}
