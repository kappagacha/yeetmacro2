namespace YeetMacro2.Services;
public enum WindowView
{
    PatternsTreeView,
    PatternsView,
    DrawView,
    UserDrawView,
    ActionView,
    ActionMenuView,
    PromptStringInputView,
    LogView
}

public interface IWindowManagerService
{
    //bool ProjectionServiceEnabled { get; }

    //void LaunchYeetMacro();
    void Show(WindowView view);
    void Close(WindowView view);
    //void Cancel(WindowView view);
    //Task<string> PromptInput(string message);
    //Task<Bounds> DrawUserRectangle();
    //Bounds TransformBounds(Bounds originalBounds, Resolution originalResolution);
    //void DrawRectangle(int x, int y, int width, int height);
    //(int x, int y) GetTopLeft();
    void StartProjectionService();
    void StopProjectionService();
    //void RequestAccessibilityPermissions();
    //void RevokeAccessibilityPermissions();
    void ShowOverlayWindow();
    void CloseOverlayWindow();
    //void DrawCircle(int x, int y);
    //Task<List<Point>> GetMatches(PatternBase template, int limit = 1);
    //void DrawClear();
}
