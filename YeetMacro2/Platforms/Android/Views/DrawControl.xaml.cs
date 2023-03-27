using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections;
using System.Collections.Concurrent;
using YeetMacro2.Platforms.Android.Services;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Views;

public partial class DrawControl : ContentView
{
    bool _userRectangle;
    double _expirationMs = 250.0;
    ConcurrentQueue<(SKPoint begin, SKPoint end, SKPaint paint, DateTime expiration)> _rectangles = new();
    ConcurrentQueue<(float x, float y, SKPaint paint, DateTime expiration)> _circles = new();
    //FixedSizedQueue<(SKPoint begin, SKPoint end, SKPaint paint)> _rectangles = new FixedSizedQueue<(SKPoint begin, SKPoint end, SKPaint paint)>();
    //FixedSizedQueue<(float x, float y, SKPaint paint)> _circles = new FixedSizedQueue<(float x, float y, SKPaint paint)>();
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
    public int RectX { get; set; }
    public int RectY { get; set; }
    public int RectWidth { get; set; }
    public int RectHeight { get; set; }
    public bool CloseAfterDraw { get; set; } = false;
    public DrawControl()
	{
		InitializeComponent();

        //_rectangles.Limit = 10;
        //_circles.Limit = 10;
        _windowManagerService = ServiceHelper.GetService<AndroidWindowManagerService>();
    }

    public void SetMaxRectangles(int maxRectangles)
    {
        //_rectangles.Limit = maxRectangles;
    }

    public void AddRectangle(float x, float y, float width, float height)
    {
        //var begin = new SKPoint(x, y);
        //var end = new SKPoint(x + width, y + height);

        var topLeft = _windowManagerService.GetTopLeftByPackage();
        //var topLeft = (x: 0.0f, y: 0.0f);
        var begin = new SKPoint(x - topLeft.x, y - topLeft.y);
        var end = new SKPoint(x - topLeft.x + width, y - topLeft.y + height);

        _rectangles.Enqueue((begin, end, _bluePaint.Clone(), DateTime.Now.AddMilliseconds(_expirationMs)));
        //_rectangles.Enqueue((begin, end, _bluePaint.Clone()));
        canvasView.InvalidateSurface();
    }

    public void ClearRectangles()
    {
        _rectangles.Clear();
        canvasView.InvalidateSurface();
    }

    public void AddCircle(float x, float y)
    {
        var topLeft = _windowManagerService.GetTopLeftByPackage();
        //var topLeft = (x: 0.0f, y: 0.0f);
        _circles.Enqueue((x - topLeft.x, y - topLeft.y, _greenPaint.Clone(), DateTime.Now.AddMilliseconds(_expirationMs)));
        //_circles.Enqueue((x - topLeft.x, y - topLeft.y, _greenPaint.Clone()));
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
        while (_circles.TryPeek(out (float x, float y, SKPaint paint, DateTime expiration) c) && c.expiration <= now)
        {
            _circles.TryDequeue(out (float x, float y, SKPaint paint, DateTime expiration) c0);
        }
        foreach (var rect in _rectangles)
        {
            canvas.DrawRect(rect.begin.X, rect.begin.Y, rect.end.X - rect.begin.X, rect.end.Y - rect.begin.Y, rect.paint);
        }
        foreach (var c in _circles)
        {
            canvas.DrawCircle(c.x, c.y, 10, c.paint);
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

                    RectX = (int)((_canvasBegin.X + topLeft.x) + _userStroke.StrokeWidth - 1);
                    RectY = (int)((_canvasBegin.Y + topLeft.y) + _userStroke.StrokeWidth - 1);
                    RectWidth = (int)(_canvasEnd.X - _canvasBegin.X - _userStroke.StrokeWidth + 1);
                    RectHeight = (int)(_canvasEnd.Y - _canvasBegin.Y - _userStroke.StrokeWidth - 1);

                    _windowManagerService.Close(WindowView.UserDrawView);
                }
                break;
        }

        e.Handled = true;
    }

    //https://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enques
    //public class FixedSizedQueue<T> : IEnumerable<T>
    //{
    //    ConcurrentQueue<T> q = new ConcurrentQueue<T>();
    //    private object lockObject = new object();

    //    public int Limit { get; set; }

    //    public void Clear()
    //    {
    //        lock (lockObject)
    //        {
    //            q.Clear();
    //        }
    //    }

    //    public void Enqueue(T obj)
    //    {
    //        q.Enqueue(obj);
    //        lock (lockObject)
    //        {
    //            T overflow;
    //            while (q.Count > Limit && q.TryDequeue(out overflow)) ;
    //        }
    //    }

    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        return ((IEnumerable<T>)q).GetEnumerator();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return ((IEnumerable)q).GetEnumerator();
    //    }
    //}
}