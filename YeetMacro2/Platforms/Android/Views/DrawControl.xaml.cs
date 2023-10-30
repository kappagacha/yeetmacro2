using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.Concurrent;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Views;

public partial class DrawControl : ContentView
{
    bool _userRectangle;
    double _expirationMs = 2000.0;
    ConcurrentQueue<(SKRect rect, SKPaint paint, DateTime expiration)> _rectangles = new();
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
    public Rect Rect { get; set; }
    public bool CloseAfterDraw { get; set; } = false;
    public DrawControl()
	{
		InitializeComponent();
        _windowManagerService = ServiceHelper.GetService<AndroidWindowManagerService>();
    }

    public void AddRectangle(Rect rect)
    {
        var topLeft = _windowManagerService.GetTopLeft();
        var location = new SKPoint((float)(rect.X - topLeft.X), (float)(rect.Y - topLeft.Y));
        var size = new SKSize((float)rect.Width, (float)rect.Height);
        var skRect = SKRect.Create(location, size);
        _rectangles.Enqueue((skRect, _bluePaint.Clone(), DateTime.Now.AddMilliseconds(_expirationMs)));
        canvasView.InvalidateSurface();
    }

    public void ClearRectangles()
    {
        _canvasBegin = _canvasEnd = SKPoint.Empty;
        _rectangles.Clear();
        canvasView.InvalidateSurface();
    }

    public void AddCircle(Point point)
    {
        var topLeft = _windowManagerService.GetTopLeft();
        _circles.Enqueue((new SKPoint((float)(point.X - topLeft.X), (float)(point.Y - topLeft.Y)), _greenPaint.Clone(), DateTime.Now.AddMilliseconds(_expirationMs)));
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
        while (_rectangles.TryPeek(out (SKRect rect, SKPaint paint, DateTime expiration) r) && r.expiration <= now)
        {
            _rectangles.TryDequeue(out (SKRect rect, SKPaint paint, DateTime expiration) r0);
        }
        while (_circles.TryPeek(out (SKPoint center, SKPaint paint, DateTime expiration) c) && c.expiration <= now)
        {
            _circles.TryDequeue(out (SKPoint center, SKPaint paint, DateTime expiration) c0);
        }
        foreach (var r in _rectangles)
        {
            canvas.DrawRect(r.rect, r.paint);
        }
        foreach (var c in _circles)
        {
            canvas.DrawCircle(c.center, 10, c.paint);
        }

        // TODO: maybe make this a toggle?
        // troubleshoot with a grid
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
                var size = new SKSize(_canvasEnd.X - _canvasBegin.X, _canvasEnd.Y - _canvasBegin.Y);
                var skRect = SKRect.Create(_canvasBegin, size);
                _rectangles.Enqueue((skRect, _bluePaint.Clone(), DateTime.Now.AddDays(1)));
                canvasView.InvalidateSurface();
                if (CloseAfterDraw)
                {
                    var topLeft = _windowManagerService.GetUserDrawViewTopLeft();
                    var padding = 1.5;
                    var calcBeginX = Math.Min(_canvasBegin.X, _canvasEnd.X);
                    var calcBeginY = Math.Min(_canvasBegin.Y, _canvasEnd.Y);
                    var calcEndX = Math.Max(_canvasBegin.X, _canvasEnd.X);
                    var calcEndY = Math.Max(_canvasBegin.Y, _canvasEnd.Y);
                    var calcWidth = calcEndX - calcBeginX - _userStroke.StrokeWidth - padding;
                    var calcHeight = calcEndY - calcBeginY - _userStroke.StrokeWidth - padding;

                    Rect = new Rect(new Point(calcBeginX + topLeft.X + _userStroke.StrokeWidth - padding / 2, calcBeginY + topLeft.Y + _userStroke.StrokeWidth - padding / 2),
                                     new Size(calcWidth <= 0 ? 1: calcWidth, calcHeight <= 0 ? 1 : calcHeight));
                    _windowManagerService.Close(AndroidWindowView.UserDrawView);
                }
                break;
        }

        e.Handled = true;
    }
}