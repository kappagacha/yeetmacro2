using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Windows.Services;
internal class WindowsInputService : IInputService
{
    public Task<string> PromptInput(string message, string placeholderInput = "")
    {
        return Application.Current.Windows[0].Page.DisplayPromptAsync("", message, placeholder: placeholderInput);
    }

    public Task<string> SelectOption(string message, params string[] options)
    {
        return Application.Current.Windows[0].Page.DisplayActionSheet(message, "cancel", "ok", options);
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
                CreateNoWindow = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            }
        };

        proc.Start();
        await proc.WaitForExitAsync();

        var output = await proc.StandardOutput.ReadToEndAsync();
        var json = JsonSerializer.Deserialize<JsonObject>(output);
        var x = (double)json["X"];
        var y = (double)json["Y"];
        var width = (double)json["Width"];
        var height = (double)json["Height"];
        var location = new Point(x, y);
        var size = new Size(width, height);
        var result = new Rect(location, size);

        return result;
    }

    public void GoBack()
    {
        // Stub implementation - Windows doesn't have a back button concept
        // This method is Android-specific functionality
    }
}
