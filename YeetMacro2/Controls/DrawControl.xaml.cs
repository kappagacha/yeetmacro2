using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using YeetMacro2.Services;

namespace YeetMacro2.Controls;

public partial class DrawControl : ContentView
{
    bool _userRectangle;
    FixedSizedQueue<(SKPoint begin, SKPoint end, SKPaint paint)> _rectangles = new FixedSizedQueue<(SKPoint begin, SKPoint end, SKPaint paint)>();
    FixedSizedQueue<(float x, float y, SKPaint paint)> _circles = new FixedSizedQueue<(float x, float y, SKPaint paint)>();
    SKPoint _canvasBegin = SKPoint.Empty, _canvasEnd = SKPoint.Empty;

    SKPaint _greenPaint = new SKPaint
    {
        Color = SKColors.Green,
        StrokeWidth = 3,
        StrokeCap = SKStrokeCap.Round,
        TextSize = 60,
        Style = SKPaintStyle.Stroke
    };
    SKPaint _bluePaint = new SKPaint
    {
        Color = SKColors.Blue,
        StrokeWidth = 3,
        StrokeCap = SKStrokeCap.Round,
        TextSize = 60,
        Style = SKPaintStyle.Stroke
    };
    IWindowManagerService _windowManagerService;
    public int RectX { get; set; }
    public int RectY { get; set; }
    public int RectWidth { get; set; }
    public int RectHeight { get; set; }
    public bool CloseAfterDraw { get; set; } = false;
    public DrawControl()
	{
		InitializeComponent();

        _rectangles.Limit = 1;
        _circles.Limit = 1;
        _windowManagerService = ServiceHelper.GetService<IWindowManagerService>();
    }

    public void SetMaxRectangles(int maxRectangles)
    {
        _rectangles.Limit = maxRectangles;
    }

    public void AddRectangle(float x, float y, float width, float height)
    {
        //var begin = new SKPoint(x, y);
        //var end = new SKPoint(x + width, y + height);

        var topLeft = _windowManagerService.GetTopLeft();
        //var topLeft = (x: 0.0f, y: 0.0f);
        var begin = new SKPoint(x - topLeft.x, y - topLeft.y);
        var end = new SKPoint(x - topLeft.x + width, y - topLeft.y + height);

        _rectangles.Enqueue((begin, end, _bluePaint.Clone()));
        canvasView.InvalidateSurface();
    }

    public void ClearRectangles()
    {
        _rectangles.Clear();
        canvasView.InvalidateSurface();
    }

    public void AddCircle(float x, float y)
    {
        var topLeft = _windowManagerService.GetTopLeft();
        //var topLeft = (x: 0.0f, y: 0.0f);
        _circles.Enqueue((x - topLeft.x, y - topLeft.y, _greenPaint.Clone()));
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
            canvas.DrawRect(_canvasBegin.X, _canvasBegin.Y - heightOffset, _canvasEnd.X - _canvasBegin.X, _canvasEnd.Y - _canvasBegin.Y - heightOffset, _bluePaint);
        }

        foreach (var rect in _rectangles)
        {
            canvas.DrawRect(rect.begin.X, rect.begin.Y, rect.end.X - rect.begin.X, rect.end.Y - rect.begin.Y, rect.paint);
        }

        foreach (var c in _circles)
        {
            canvas.DrawCircle(c.x, c.y, 5, c.paint);
        }
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
                _rectangles.Enqueue((new SKPoint(_canvasBegin.X, _canvasBegin.Y), new SKPoint(_canvasEnd.X, _canvasEnd.Y), _greenPaint.Clone()));
                canvasView.InvalidateSurface();
                if (CloseAfterDraw)
                {
                    var topLeft = _windowManagerService.GetTopLeft();
                    //var topLeft = (x: 0, y: 0);

                    RectX = (int)((_canvasBegin.X + topLeft.x));
                    RectY = (int)((_canvasBegin.Y + topLeft.y));
                    RectWidth = (int)((_canvasEnd.X - _canvasBegin.X));
                    RectHeight = (int)((_canvasEnd.Y - _canvasBegin.Y));

                    _windowManagerService.Close(WindowView.UserDrawView);
                }
                break;
        }

        e.Handled = true;
    }

    //https://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enques
    public class FixedSizedQueue<T> : IEnumerable<T>
    {
        ConcurrentQueue<T> q = new ConcurrentQueue<T>();
        private object lockObject = new object();

        public int Limit { get; set; }

        public void Clear()
        {
            lock (lockObject)
            {
                q.Clear();
            }
        }

        public void Enqueue(T obj)
        {
            q.Enqueue(obj);
            lock (lockObject)
            {
                T overflow;
                while (q.Count > Limit && q.TryDequeue(out overflow)) ;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)q).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)q).GetEnumerator();
        }
    }
}