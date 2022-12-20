namespace YeetMacro2.Services;
public interface IAccessibilityService
{
    bool HasAccessibilityPermissions { get; }
    string CurrentPackage { get; }

    void DoClick(float x, float y);
    void Start();
    void Stop();
}
