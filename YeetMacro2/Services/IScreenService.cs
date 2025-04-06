using YeetMacro2.Data.Models;

namespace YeetMacro2.Services;

public class FindPatternResult
{
    public bool IsSuccess { get; set; }
    public string Path { get; set; }
    public string PredicatePath { get; set; }
    public string InversePredicatePath { get; set; }
    public Point Point { get; set; }
    public Point[] Points { get; set; }
}

public class FindOptions
{
    public int Limit { get; set; } = 1;
    public double VariancePct { get; set; }
    public double Scale { get; set; }
    public Point Offset { get; set; } = Point.Zero;
    //public Rect OverrideRect { get; set; }
}

public class TextFindOptions
{
    public Point Offset { get; set; } = Point.Zero;

    public String Whitelist { get; set; }
}

public interface IScreenService
{
    void DrawClear();
    void DrawRectangle(Rect rect);
    void DrawCircle(Point point);
    void DebugRectangle(Rect rect);
    void DebugCircle(Point point);
    void DebugClear();
    List<Point> GetMatches(Pattern template, FindOptions opts);
    void DoClick(Point point, long holdDurationMs = 100);
    void DoSwipe(Point start, Point end);
    Byte[] CalcColorThreshold(Pattern pattern, ColorThresholdProperties colorThreshold);
    byte[] GetCurrentImageData();
    byte[] GetCurrentImageData(Rect rect);
    string GetText(Pattern pattern, TextFindOptions opts);
    Task<string> GetTextAsync(Pattern pattern, TextFindOptions opts);
    string GetText(byte[] currentImage);
    FindPatternResult ClickPattern(Pattern pattern, FindOptions opts);
    FindPatternResult FindPattern(Pattern pattern, FindOptions opts);
    void ShowMessage(string message);
    byte[] ScaleImageData(byte[] data, double scale);
}