using System.Diagnostics;
using System.Text.Json;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Windows.Services;
internal class WindowsInputService : IInputService
{
    public Task<string> PromptInput(string message)
    {
        return Application.Current.MainPage.DisplayPromptAsync("", message);
    }

    public Task<string> SelectOption(string message, params string[] options)
    {
        return Application.Current.MainPage.DisplayActionSheet(message, "cancel", "ok", options);
    }

    public async Task<Rect> DrawUserRectangle()
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
        var result = JsonSerializer.Deserialize<Rect>(output);

        return result;
    }
}
