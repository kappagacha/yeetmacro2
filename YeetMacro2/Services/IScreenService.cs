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
    void DrawRectangle(Point start, Point end);
    void DrawCircle(Point point);
    void DebugRectangle(Point start, Point end);
    void DebugCircle(Point point);
    void DebugClear();
    Task<List<Point>> GetMatches(Pattern template, FindOptions opts);
    void DoClick(Point point);
    void DoSwipe(Point start, Point end);
    Byte[] CalcColorThreshold(Pattern pattern, ColorThresholdProperties colorThreshold);
    Task<byte[]> GetCurrentImageData(Point start, Point end);
    Task<string> GetText(Pattern pattern);
    Task<FindPatternResult> ClickPattern(Pattern pattern);
    Task<FindPatternResult> FindPattern(Pattern pattern, FindOptions opts);
}
