using System.Diagnostics;
using System.Runtime.InteropServices;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;
using System.Drawing;
using System.Drawing.Imaging;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace YeetMacro2.Platforms.Windows.Services;
public class WindowsScreenService : IScreenService, IRecorderService
{
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    public byte[] CalcColorThreshold(Pattern pattern)
    {
        return OpenCvHelper.CalcColorThreshold(pattern.ImageData, pattern.ColorThreshold);
    }

    public byte[] CalcColorThreshold(Pattern pattern, ColorThresholdProperties colorThreshold)
    {
        throw new NotImplementedException();
    }

    public Task<FindPatternResult> ClickPattern(Pattern pattern)
    {
        throw new NotImplementedException();
    }

    public void DebugCircle(int x, int y)
    {
        throw new NotImplementedException();
    }

    public void DebugClear()
    {
        throw new NotImplementedException();
    }

    public void DebugRectangle(int x, int y, int width, int height)
    {
        throw new NotImplementedException();
    }

    public void DoClick(float x, float y)
    {
        throw new NotImplementedException();
    }

    public void DoSwipe(Microsoft.Maui.Graphics.Point start, Microsoft.Maui.Graphics.Point end)
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

    public Task<FindPatternResult> FindPattern(Pattern pattern, FindOptions opts)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> GetCurrentImageData(int x, int y, int w, int h)
    {
        // https://nishanc.medium.com/c-screenshot-utility-to-capture-a-portion-of-the-screen-489ddceeee49
        Rectangle rect = new Rectangle(x, y, w, h);
        var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
        Graphics g = Graphics.FromImage(bmp);
        var size = new System.Drawing.Size(w, h);
        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, size, CopyPixelOperation.SourceCopy);

        //var file = FileSystem.Current.AppDataDirectory + "/test.png";
        //bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);

        //var stream2 = new MemoryStream();
        //bmp.Save(stream2, System.Drawing.Imaging.ImageFormat.Png);
        //var image = ImageSource.FromStream(() => stream2);

        //using (FileStream fs = new FileStream(FileSystem.Current.AppDataDirectory + "/test.png", FileMode.Create, FileAccess.ReadWrite))
        //{
        //    byte[] bytes = stream2.ToArray();
        //    fs.Write(bytes, 0, bytes.Length);
        //}

        // capture specific window

        //var handle = WindowHelper.WinGetHandle("Nox", "DisgaeaRPG2");
        //var bmp = PrintWindow(handle);

        var stream = new MemoryStream();
        bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        return Task.FromResult(stream.ToArray());
    }

    public Task<List<Microsoft.Maui.Graphics.Point>> GetMatches(Pattern template, int limit = 1)
    {
        throw new NotImplementedException();
    }

    public Task<List<Microsoft.Maui.Graphics.Point>> GetMatches(Pattern template, FindOptions opts)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetText(Pattern pattern)
    {
        throw new NotImplementedException();
    }

    public void StartRecording()
    {
        throw new NotImplementedException();
    }

    public void StopRecording()
    {
        throw new NotImplementedException();
    }

    public Bounds TransformBounds(Bounds originalBounds, Resolution originalResolution)
    {
        throw new NotImplementedException();
    }

}

public static class WindowHelper
{
    public static IntPtr WinGetHandle(string processName, string windowName)
    {
        foreach (Process pList in Process.GetProcesses())
            if (pList.ProcessName.Contains(processName) && pList.MainWindowTitle.Contains(windowName))
                return pList.MainWindowHandle;

        return IntPtr.Zero;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    private int _Left;
    private int _Top;
    private int _Right;
    private int _Bottom;

    public RECT(RECT Rectangle) : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
    {
    }
    public RECT(int Left, int Top, int Right, int Bottom)
    {
        _Left = Left;
        _Top = Top;
        _Right = Right;
        _Bottom = Bottom;
    }

    public int X
    {
        get { return _Left; }
        set { _Left = value; }
    }
    public int Y
    {
        get { return _Top; }
        set { _Top = value; }
    }
    public int Left
    {
        get { return _Left; }
        set { _Left = value; }
    }
    public int Top
    {
        get { return _Top; }
        set { _Top = value; }
    }
    public int Right
    {
        get { return _Right; }
        set { _Right = value; }
    }
    public int Bottom
    {
        get { return _Bottom; }
        set { _Bottom = value; }
    }
    public int Height
    {
        get { return _Bottom - _Top; }
        set { _Bottom = value + _Top; }
    }
    public int Width
    {
        get { return _Right - _Left; }
        set { _Right = value + _Left; }
    }
    public Point Location
    {
        get { return new Point(Left, Top); }
        set
        {
            _Left = value.X;
            _Top = value.Y;
        }
    }
    public Size Size
    {
        get { return new Size(Width, Height); }
        set
        {
            _Right = value.Width + _Left;
            _Bottom = value.Height + _Top;
        }
    }

    public static implicit operator Rectangle(RECT Rectangle)
    {
        return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
    }
    public static implicit operator RECT(Rectangle Rectangle)
    {
        return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
    }
    public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
    {
        return Rectangle1.Equals(Rectangle2);
    }
    public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
    {
        return !Rectangle1.Equals(Rectangle2);
    }

    public override string ToString()
    {
        return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public bool Equals(RECT Rectangle)
    {
        return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
    }

    public override bool Equals(object Object)
    {
        if (Object is RECT)
        {
            return Equals((RECT)Object);
        }
        else if (Object is Rectangle)
        {
            return Equals(new RECT((Rectangle)Object));
        }

        return false;
    }
}