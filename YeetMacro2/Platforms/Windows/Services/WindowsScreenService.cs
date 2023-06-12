using System.Diagnostics;
using System.Runtime.InteropServices;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;
using System.Drawing;
using System.Drawing.Imaging;
using Size = System.Drawing.Size;
using Point = Microsoft.Maui.Graphics.Point;

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

    public void DebugCircle(Point point)
    {
        throw new NotImplementedException();
    }

    public void DebugClear()
    {
        throw new NotImplementedException();
    }

    public void DebugRectangle(Rect rect)
    {
        throw new NotImplementedException();
    }

    public void DoClick(Point point)
    {
        throw new NotImplementedException();
    }

    public void DoSwipe(Point start, Point end)
    {
        throw new NotImplementedException();
    }

    public void DrawCircle(Point point)
    {
        throw new NotImplementedException();
    }

    public void DrawClear()
    {
        throw new NotImplementedException();
    }

    public void DrawRectangle(Rect rect)
    {
        throw new NotImplementedException();
    }

    public Task<FindPatternResult> FindPattern(Pattern pattern, FindOptions opts)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> GetCurrentImageData(Rect rect)
    {
        // https://nishanc.medium.com/c-screenshot-utility-to-capture-a-portion-of-the-screen-489ddceeee49
        //Rectangle rect = new Rectangle((int)start.X, (int)start.Y, (int)width, (int)height);
        var bmp = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format32bppArgb);
        Graphics g = Graphics.FromImage(bmp);
        var size = new System.Drawing.Size((int)rect.Width, (int)rect.Height);
        g.CopyFromScreen((int)rect.Left, (int)rect.Top, 0, 0, size, CopyPixelOperation.SourceCopy);

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

    public Task<string> GetText(Pattern pattern, String whitelist = null)
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
    public System.Drawing.Point Location
    {
        get { return new System.Drawing.Point(Left, Top); }
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