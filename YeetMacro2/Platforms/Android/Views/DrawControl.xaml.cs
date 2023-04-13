using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.Concurrent;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Views;

public partial class DrawControl : ContentView
{
    bool _userRectangle;
    double _expirationMs = 250.0;
    ConcurrentQueue<(SKPoint begin, SKPoint end, SKPaint paint, DateTime expiration)> _rectangles = new();
    ConcurrentQueue<(SKPoint center, SKPaint paint, DateTime expiration)> _circles = new();
    SKPoint _canvasBegin = SKPoint.Empty, _canvasEnd = SKPoint.Empty;
    SKPaint _greenPaint = new SKPaint
    {
        Color = SKColors.Green,
        StrokeWidth = 6,
        StrokeCap = SKStrokeCap.Round,
        TextSize = 60,
        Style = SKPaintStyle.Stroke
    };
    SKPaint _bluePaint = new SKPaint
    {
        Color = SKColors.Blue,
        StrokeWidth = 6,
        StrokeCap = SKStrokeCap.Round,
        TextSize = 60,
        Style = SKPaintStyle.Stroke
    };
    SKPaint _userStroke = new SKPaint
    {
        Color = SKColors.Red,
        StrokeWidth = 3,
        StrokeCap = SKStrokeCap.Round,
        TextSize = 60,
        Style = SKPaintStyle.Stroke
    };
    AndroidWindowManagerService _windowManagerService;
    public Point Start { get; set; }
    public Point End { get; set; }
    public bool CloseAfterDraw { get; set; } = false;
    public DrawControl()
	{
		InitializeComponent();
        _windowManagerService = ServiceHelper.GetService<AndroidWindowManagerService>();
    }

    public void AddRectangle(Point start, Point end)
    {
        var topLeft = _windowManagerService.GetTopLeftByPackage();
        //var topLeft = (x: 0.0f, y: 0.0f);
        var skStart = new SKPoint((float)(start.X - topLeft.x), (float)(start.Y - topLeft.y));
        var skEnd = new SKPoint((float)(end.X - topLeft.x), (float)(end.Y - topLeft.y));

        _rectangles.Enqueue((skStart, skEnd, _bluePaint.Clone(), DateTime.Now.AddMilliseconds(_expirationMs)));
        canvasView.InvalidateSurface();
    }

    public void ClearRectangles()
    {
        _rectangles.Clear();
        canvasView.InvalidateSurface();
    }

    public void AddCircle(Point point)
    {
        var topLeft = _windowManagerService.GetTopLeftByPackage();
        //var topLeft = (x: 0.0f, y: 0.0f);
        _circles.Enqueue((new SKPoint((float)point.X, (float)point.Y), _greenPaint.Clone(), DateTime.Now.AddMilliseconds(_expirationMs)));
        canvasView.InvalidateSurface();
    }

    public void ClearCircles()
    {
        _circles.Clear();
        canvasView.InvalidateSurface();
    }

    //https://www.c-sharpcorner.com/article/getting-started-with-skiasharp-with-xamarin-forms/
    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();
        if (_userRectangle)
        {
            var heightOffset = 0.0f;
            canvas.DrawRect(_canvasBegin.X, _canvasBegin.Y - heightOffset, _canvasEnd.X - _canvasBegin.X, _canvasEnd.Y - _canvasBegin.Y - heightOffset, _userStroke);
        }

        DateTime now = DateTime.Now;
        while(_rectangles.TryPeek(out (SKPoint begin, SKPoint end, SKPaint paint, DateTime expiration) r) && r.expiration <= now)
        {
            _rectangles.TryDequeue(out (SKPoint begin, SKPoint end, SKPaint paint, DateTime expiration) r0);
        }
        while (_circles.TryPeek(out (SKPoint center, SKPaint paint, DateTime expiration) c) && c.expiration <= now)
        {
            _circles.TryDequeue(out (SKPoint center, SKPaint paint, DateTime expiration) c0);
        }
        foreach (var rect in _rectangles)
        {
            canvas.DrawRect(rect.begin.X, rect.begin.Y, rect.end.X - rect.begin.X, rect.end.Y - rect.begin.Y, rect.paint);
        }
        foreach (var c in _circles)
        {
            canvas.DrawCircle(c.center.X, c.center.Y, 10, c.paint);
        }

        //troubleshoot with a grid
        //var width = DeviceDisplay.MainDisplayInfo.Width;
        //var height = DeviceDisplay.MainDisplayInfo.Height;

        //for (int i = 0; i < width; i += 100)
        //{
        //    canvas.DrawRect(i, 0, 2, (float)height, i % 200 == 0 ? _greenPaint : _bluePaint);
        //}

        //for (int i = 0; i < height; i += 100)
        //{
        //    canvas.DrawRect(0, i, (float)width, 2, i % 200 == 0 ? _greenPaint : _bluePaint);
        //}
    }

    private void canvasView_Touch(object sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _userRectangle = true;
                _canvasBegin.X = e.Location.X;
                _canvasBegin.Y = e.Location.Y;
                _canvasEnd.X = e.Location.X;
                _canvasEnd.Y = e.Location.Y;
                break;
            case SKTouchAction.Moved:
                _canvasEnd.X = e.Location.X;
                _canvasEnd.Y = e.Location.Y;
                canvasView.InvalidateSurface();
                break;
            case SKTouchAction.Released:
            case SKTouchAction.Cancelled:
                _rectangles.Enqueue((new SKPoint(_canvasBegin.X, _canvasBegin.Y), new SKPoint(_canvasEnd.X, _canvasEnd.Y), _greenPaint.Clone(), DateTime.Now.AddDays(1)));
                canvasView.InvalidateSurface();
                if (CloseAfterDraw)
                {
                    var topLeft = _windowManagerService.GetTopLeftByPackage();
                    Start = new Point(_canvasBegin.X + topLeft.x + _userStroke.StrokeWidth - 1, _canvasBegin.Y + topLeft.y + _userStroke.StrokeWidth - 1);
                    End = new Point(_canvasEnd.X - _userStroke.StrokeWidth + 1, _canvasEnd.Y - _userStroke.StrokeWidth - 1);
                    _windowManagerService.Close(AndroidWindowView.UserDrawView);
                }
                break;
        }

        e.Handled = true;
    }
}