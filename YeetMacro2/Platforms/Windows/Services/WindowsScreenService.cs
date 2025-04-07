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
    public Microsoft.Maui.Graphics.Size CalcResolution => throw new NotImplementedException();

    public Microsoft.Maui.Graphics.Size Resolution => throw new NotImplementedException();

    public double Density => throw new NotImplementedException();

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

    public FindPatternResult ClickPattern(Pattern pattern, FindOptions opts)
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

    public void DoClick(Point point, long holdDurationMs = 100)
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

    public FindPatternResult FindPattern(Pattern pattern, FindOptions opts)
    {
        throw new NotImplementedException();
    }

    public byte[] GetCurrentImageData()
    {
        var mdi = DeviceDisplay.Current.MainDisplayInfo;
        // https://nishanc.medium.com/c-screenshot-utility-to-capture-a-portion-of-the-screen-489ddceeee49
        Rectangle rect = new Rectangle(0, 0, (int)mdi.Width, (int)mdi.Height);
        var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
        Graphics g = Graphics.FromImage(bmp);
        var size = new System.Drawing.Size((int)rect.Width, (int)rect.Height);
        g.CopyFromScreen(0, 0, 0, 0, size, CopyPixelOperation.SourceCopy);
        
        var stream = new MemoryStream();
        bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        return stream.ToArray();
    }

    public byte[] GetCurrentImageData(Rect rect)
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
        return stream.ToArray();
    }

    public List<Point> GetMatches(Pattern template, FindOptions opts)
    {
        throw new NotImplementedException();
    }

    public string FindText(Pattern pattern, TextFindOptions opts)
    {
        throw new NotImplementedException();
    }

    public string FindText(byte[] currentImage)
    {
        throw new NotImplementedException();
    }

    public Task<string> FindTextAsync(Pattern pattern, TextFindOptions opts)
    {
        throw new NotImplementedException();
    }

    public Point GetTopLeft()
    {
        return Point.Zero;
    }

    public byte[] ScaleImageData(byte[] data, double scale)
    {
        throw new NotImplementedException();
    }

    public void ShowMessage(string message)
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

    public string FindText(Rect bounds, TextFindOptions opts)
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
public struct RECT(int Left, int Top, int Right, int Bottom)
{
    private int _Left = Left;
    private int _Top = Top;
    private int _Right = Right;
    private int _Bottom = Bottom;

    public RECT(RECT Rectangle) : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
    {
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
        if (Object is RECT rect)
        {
            return Equals(rect);
        }
        else if (Object is Rectangle rectangle)
        {
            return Equals(new RECT(rectangle));
        }

        return false;
    }
}