using SkiaSharp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using YeetMacro2.Services;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace YeetMacro2.Platforms.Windows.Services;
public class WindowsProjectionService : IMediaProjectionService
{
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


    public bool Enabled => throw new NotImplementedException();

    public Task<TBitmap> GetCurrentImageBitmap<TBitmap>(int x, int y, int width, int height)
    {
        throw new NotImplementedException();
    }

    public Task<TBitmap> GetCurrentImageBitmap<TBitmap>()
    {
        throw new NotImplementedException();
    }

    public Task<MemoryStream> GetCurrentImageStream()
    {
        throw new NotImplementedException();
    }

    public Task<MemoryStream> GetCurrentImageStream(int x, int y, int width, int height)
    {
        // https://nishanc.medium.com/c-screenshot-utility-to-capture-a-portion-of-the-screen-489ddceeee49
        Rectangle rect = new Rectangle(x, y, width, height);
        var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
        Graphics g = Graphics.FromImage(bmp);
        var size = new System.Drawing.Size(width, height);
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
        bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        return Task.FromResult(stream);
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public IntPtr GetHandleWindow(string title)
    {
        return FindWindow(null, title);
    }

    // https://stackoverflow.com/questions/891345/get-a-screenshot-of-a-specific-application
    public static Bitmap PrintWindow(IntPtr hwnd)
    {
        RECT rc;
        GetWindowRect(hwnd, out rc);

        Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
        Graphics gfxBmp = Graphics.FromImage(bmp);
        IntPtr hdcBitmap = gfxBmp.GetHdc();

        PrintWindow(hwnd, hdcBitmap, 0);

        gfxBmp.ReleaseHdc(hdcBitmap);
        gfxBmp.Dispose();

        return bmp;
    }
}

public static class WindowHelper {
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