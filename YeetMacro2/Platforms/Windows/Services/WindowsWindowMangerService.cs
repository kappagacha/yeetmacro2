using System.Diagnostics;
using System.Text.Json;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Windows.Services;
public class WindowsWindowMangerService : IWindowManagerService
{
    public bool ProjectionServiceEnabled => throw new NotImplementedException();

    public void Cancel(WindowView view)
    {
        throw new NotImplementedException();
    }

    public void Close(WindowView view)
    {
        throw new NotImplementedException();
    }

    public void CloseOverlayWindow()
    {
        throw new NotImplementedException();
    }

    public void DrawCircle(int x, int y)
    {
        throw new NotImplementedException();
    }

    public void DrawClear()
    {
        throw new NotImplementedException();
    }

    public void DrawRectangle(int x, int y, int width, int height)
    {
        throw new NotImplementedException();
    }

    public async Task<Bounds> DrawUserRectangle()
    {
        // https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "Platforms/Windows/Ahk/userRectangle/userRectangle.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        proc.Start();        
        await proc.WaitForExitAsync();
        var output = await proc.StandardOutput.ReadToEndAsync();
        var result = JsonSerializer.Deserialize<Bounds>(output);
        return result;
    }

    public Task<List<Point>> GetMatches(PatternBase template, int limit = 1)
    {
        throw new NotImplementedException();
    }

    public (int x, int y) GetTopLeft()
    {
        throw new NotImplementedException();
    }

    public void LaunchYeetMacro()
    {
        throw new NotImplementedException();
    }

    public Task<string> PromptInput(string message)
    {
        return Application.Current.MainPage.DisplayPromptAsync("", message);
    }

    public void RequestAccessibilityPermissions()
    {
        throw new NotImplementedException();
    }

    public void RevokeAccessibilityPermissions()
    {
        throw new NotImplementedException();
    }

    public void Show(WindowView view)
    {
        throw new NotImplementedException();
    }

    public void ShowOverlayWindow()
    {
        throw new NotImplementedException();
    }

    public void StartProjectionService()
    {
        throw new NotImplementedException();
    }

    public void StopProjectionService()
    {
        throw new NotImplementedException();
    }

    public Bounds TransformBounds(Bounds originalBounds, Resolution originalResolution)
    {
        throw new NotImplementedException();
    }
}
