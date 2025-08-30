using Android.Widget;
using static Android.Views.View;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables.Shapes;
using Color = Android.Graphics.Color;
using Microsoft.Maui.Platform;
using YeetMacro2.Data.Models;
using CommunityToolkit.Mvvm.Messaging;
using Java.Lang;
using Exception = System.Exception;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Views;

public class ResizeView : RelativeLayout, IOnTouchListener, IShowable, IDisposable
{
    enum FormState { SHOWING, CLOSED };
    private readonly IWindowManager _windowManager;
    private readonly MainActivity _context;
    private WindowManagerLayoutParams _layoutParams, _layoutParamsPortrait, _layoutParamsLandscape;
    private readonly ImageView _topLeft, _bottomRight, _topRight;
    int _x, _y, _displayWidth, _displayHeight;
    double _density;
    private readonly VisualElement _visualElement;
    private const int MIN_WIDTH = 50, MIN_HEIGHT = 50;
    private volatile FormState _state;
    public VisualElement VisualElement => _visualElement;
    TaskCompletionSource<bool> _closeCompleted;
    public Action OnShow { get; set; }
    public Action OnClose { get; set; }
    public bool IsShowing { get => _state == FormState.SHOWING; }
    private readonly object _stateLock = new object();
    private bool _disposed = false;
    private global::Android.Views.View _androidView;

    //https://www.linkedin.com/pulse/6-floating-windows-android-keyboard-input-v%C3%A1clav-hodek/

    public ResizeView(Context context, IWindowManager windowManager, VisualElement visualElement) : base(context)
    {
        // Safe cast with null check
        _context = context as MainActivity;
        _windowManager = windowManager;
        _layoutParamsPortrait = GenerateLayoutParams();
        _layoutParamsLandscape = GenerateLayoutParams();

        var density = DisplayHelper.DisplayInfo.Density;
        var cornerShapeSize = (int)(30 * density);
        var shape = new global::Android.Graphics.Drawables.ShapeDrawable(new OvalShape());
        shape.SetIntrinsicHeight(cornerShapeSize);
        shape.SetIntrinsicWidth(cornerShapeSize);
        shape.Paint.Color = Color.Blue;

        //https://stackoverflow.com/questions/4638832/how-to-programmatically-set-the-layout-align-parent-right-attribute-of-a-button
        _topLeft = new ImageView(_context);
        _topLeft.SetImageDrawable(shape);
        _topLeft.SetX(-cornerShapeSize / 2.0f);
        _topLeft.SetY(-cornerShapeSize / 2.0f);

        RelativeLayout.LayoutParams topLeftParams = new(cornerShapeSize, cornerShapeSize);
        topLeftParams.AddRule(LayoutRules.AlignParentTop);
        topLeftParams.AddRule(LayoutRules.AlignParentLeft);
        _topLeft.Clickable = true;
        _topLeft.SetOnTouchListener(this);

        _bottomRight = new ImageView(_context);
        _bottomRight.SetImageDrawable(shape);
        _bottomRight.SetX(cornerShapeSize / 2.0f);
        _bottomRight.SetY(cornerShapeSize / 2.0f);

        RelativeLayout.LayoutParams bottomRightParams = new(cornerShapeSize, cornerShapeSize);
        bottomRightParams.AddRule(LayoutRules.AlignParentBottom);
        bottomRightParams.AddRule(LayoutRules.AlignParentRight);
        _bottomRight.Clickable = true;
        _bottomRight.SetOnTouchListener(this);

        _topRight = new ImageView(_context);
        _topRight.SetImageResource(global::Android.Resource.Drawable.IcMenuCloseClearCancel);
        _topRight.SetColorFilter(Color.Red);    //https://stackoverflow.com/questions/1309629/how-to-change-colors-of-a-drawable-in-android

        RelativeLayout.LayoutParams topRightParams = new(cornerShapeSize, cornerShapeSize);
        topRightParams.AddRule(LayoutRules.AlignParentTop);
        topRightParams.AddRule(LayoutRules.AlignParentRight);
        _topRight.Clickable = true;
        _topRight.SetOnTouchListener(this);

        SetBackgroundColor(global::Android.Graphics.Color.Argb(70, 0, 0, 0));
        _state = FormState.CLOSED;

        // https://www.andreasnesheim.no/embedding-net-maui-pages-into-your-net-android-ios-application/
        _visualElement = visualElement;
        _androidView = visualElement.ToPlatform(IPlatformApplication.Current.Application.Handler.MauiContext);

        AddView(_androidView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        AddView(_topLeft, topLeftParams);
        AddView(_bottomRight, bottomRightParams);
        AddView(_topRight, topRightParams);

        _androidView.Clickable = true;
        Clickable = true;
        InitDisplay();

        WeakReferenceMessenger.Default.Register<DisplayInfoChangedEventArgs>(this, (r, e) =>
        {
            lock (_stateLock)
            {
                if (_disposed) return;
                
                var isPortrait = e.DisplayInfo.Orientation == DisplayOrientation.Portrait;
                _layoutParams = isPortrait ? _layoutParamsPortrait : _layoutParamsLandscape;

                if (_state == FormState.SHOWING)
                {
                    try
                    {
                        _windowManager?.UpdateViewLayout(this, _layoutParams);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Exception updating layout on orientation change: {ex.Message}");
                    }
                }
            }
        });
    }

    private WindowManagerLayoutParams GenerateLayoutParams()
    {
        var layoutParams = new WindowManagerLayoutParams
        {
            Type = OperatingSystem.IsAndroidVersionAtLeast(26) ? WindowManagerTypes.ApplicationOverlay : WindowManagerTypes.Phone,
            Format = Format.Translucent,
            //Flags = WindowManagerFlags.NotFocusable | WindowManagerFlags.TranslucentNavigation | WindowManagerFlags.LayoutNoLimits,
            Flags = WindowManagerFlags.NotFocusable | WindowManagerFlags.TranslucentNavigation,
            Gravity = GravityFlags.Top | GravityFlags.Left
        };

        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            layoutParams.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
        }

        return layoutParams;
    }

    private void InitDisplay()
    {
        _density = DisplayHelper.DisplayInfo.Density;
        _displayWidth = (int)DisplayHelper.DisplayInfo.Width;
        _displayHeight = (int)DisplayHelper.DisplayInfo.Height;

        var isPortrait = DisplayHelper.DisplayInfo.Orientation == DisplayOrientation.Portrait;

        _layoutParamsPortrait.Width = (int)((isPortrait ? _displayWidth : _displayHeight) * 0.75);
        _layoutParamsPortrait.Height = (int)((isPortrait ? _displayHeight : _displayWidth) * 0.75);
        _layoutParamsPortrait.X = (int)((isPortrait ? _displayWidth : _displayHeight) * 0.20);
        _layoutParamsPortrait.Y = (int)((isPortrait ? _displayHeight : _displayWidth) * 0.25);

        _layoutParamsLandscape.Width = (int)((isPortrait ? _displayHeight : _displayWidth) * 0.75);
        _layoutParamsLandscape.Height = (int)((isPortrait ? _displayWidth : _displayHeight) * 0.75);
        _layoutParamsLandscape.X = (int)((isPortrait ? _displayHeight : _displayWidth) * 0.20);
        _layoutParamsLandscape.Y = (int)((isPortrait ? _displayWidth : _displayHeight) * 0.25);

        _layoutParams = isPortrait ? _layoutParamsPortrait : _layoutParamsLandscape;

        lock (_stateLock)
        {
            if (_state == FormState.SHOWING && !_disposed)
            {
                try
                {
                    _windowManager?.UpdateViewLayout(this, _layoutParams);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception in InitDisplay: {ex.Message}");
                }
            }
        }
    }

    //https://developer.android.com/training/gestures/viewgroup
    public override bool OnInterceptTouchEvent(MotionEvent ev)
    {
        if (ev.Action == MotionEventActions.Down && ClickIsInside(this, ev))
        {
            EnableKeyboard();
        }
        else if (ev.Action == MotionEventActions.Down)
        {
            DisableKeyboard();
        }

        return base.OnInterceptTouchEvent(ev);
    }

    public void Show()
    {
        lock (_stateLock)
        {
            if (_state == FormState.SHOWING || _disposed) return;

            try
            {
                var isPortrait = DisplayHelper.DisplayInfo.Orientation == DisplayOrientation.Portrait;
                _layoutParams = isPortrait ? _layoutParamsPortrait : _layoutParamsLandscape;
                _windowManager?.AddView(this, _layoutParams);
                _closeCompleted = new TaskCompletionSource<bool>();
                OnShow?.Invoke();
                _state = FormState.SHOWING;
            }
            catch (WindowManagerBadTokenException ex)
            {
                _state = FormState.CLOSED;
                System.Diagnostics.Debug.WriteLine($"WindowManager BadTokenException in Show: {ex.Message}");
            }
            catch (Exception ex)
            {
                _state = FormState.CLOSED;
                System.Diagnostics.Debug.WriteLine($"Exception in Show: {ex.Message}");
            }
        }
    }

    public void Close()
    {
        lock (_stateLock)
        {
            if (_state == FormState.CLOSED || _disposed) return;

            try
            {
                _windowManager?.RemoveView(this);
                _state = FormState.CLOSED;
                _closeCompleted?.TrySetResult(true);
                OnClose?.Invoke();
            }
            catch (IllegalArgumentException)
            {
                _state = FormState.CLOSED;
            }
            catch (Exception ex)
            {
                _state = FormState.CLOSED;
                System.Diagnostics.Debug.WriteLine($"Exception in Close: {ex.Message}");
            }
        }
    }

    public void CloseCancel()
    {
        lock (_stateLock)
        {
            if (_state == FormState.CLOSED || _disposed) return;

            try
            {
                _windowManager?.RemoveView(this);
                _state = FormState.CLOSED;
                _closeCompleted?.TrySetResult(false);
                OnClose?.Invoke();
            }
            catch (IllegalArgumentException)
            {
                _state = FormState.CLOSED;
            }
            catch (Exception ex)
            {
                _state = FormState.CLOSED;
                System.Diagnostics.Debug.WriteLine($"Exception in CloseCancel: {ex.Message}");
            }
        }
    }

    private void EnableKeyboard()
    {
        lock (_stateLock)
        {
            if (Focusable && !_disposed && _state == FormState.SHOWING)
            {
                try
                {
                    _layoutParams.Flags &= ~WindowManagerFlags.NotFocusable;
                    _windowManager?.UpdateViewLayout(this, _layoutParams);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception in EnableKeyboard: {ex.Message}");
                }
            }
        }
    }

    private void DisableKeyboard()
    {
        lock (_stateLock)
        {
            if (!_disposed && _state == FormState.SHOWING)
            {
                try
                {
                    _layoutParams.Flags |= WindowManagerFlags.NotFocusable;
                    _windowManager?.UpdateViewLayout(this, _layoutParams);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception in DisableKeyboard: {ex.Message}");
                }
            }
        }
    }

    //https://stackoverflow.com/questions/11172191/android-motionevent-find-out-if-motion-happened-outside-the-view
    private bool ClickIsInside(global::Android.Views.View v, MotionEvent e)
    {
        return !(e.GetX() < 0 || e.GetY() < 0
                || e.GetX() > v.MeasuredWidth
                || e.GetY() > v.MeasuredHeight);
    }

    public bool OnTouch(global::Android.Views.View v, MotionEvent e)
    {
        if (_disposed) return false;
        
        try
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _x = (int)e.RawX;
                    _y = (int)e.RawY;
                    if (v == _topRight)
                    {
                        DisableKeyboard();
                        Close();
                    }
                    break;
                case MotionEventActions.Move:
                    int nowX = (int)e.RawX;
                    int nowY = (int)e.RawY;
                    int movedX = nowX - _x;
                    int movedY = nowY - _y;

                    lock (_stateLock)
                    {
                        if (_state == FormState.SHOWING && !_disposed)
                        {
                            if (v == _topLeft)
                            {
                                _x = nowX;
                                _y = nowY;
                                _layoutParams.X += movedX;
                                _layoutParams.Y += movedY;
                                _windowManager?.UpdateViewLayout(this, _layoutParams);
                            }
                            else if (v == _bottomRight)
                            {
                                //var topLeft = _windowManagerService.GetTopLeft();
                                var topLeft = (x: 0.0, y: 0.0);
                                var targetWidth = nowX - _layoutParams.X - topLeft.x;
                                var targetHeight = nowY - _layoutParams.Y - topLeft.y;

                                if (targetWidth >= MIN_WIDTH && targetWidth <= _displayWidth)
                                {
                                    _layoutParams.Width = (int)targetWidth;
                                }

                                if (targetHeight >= MIN_HEIGHT && targetHeight <= _displayHeight)
                                {
                                    _layoutParams.Height = (int)targetHeight;
                                }

                                _windowManager?.UpdateViewLayout(this, _layoutParams);
                                _visualElement?.Layout(new Microsoft.Maui.Graphics.Rect(0, 0, _layoutParams.Width / _density, _layoutParams.Height / _density));
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in OnTouch: {ex.Message}");
        }
        return false;
    }

    public async Task<bool> WaitForClose()
    {
        if (_closeCompleted == null) return false;
        return await _closeCompleted.Task;
    }

    protected override void Dispose(bool disposing)
    {
        lock (_stateLock)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        // Ensure we're closed before disposing
                        if (_state == FormState.SHOWING)
                        {
                            try
                            {
                                _windowManager?.RemoveView(this);
                            }
                            catch { /* Ignore errors during disposal */ }
                        }

                        // Clean up event handlers
                        if (_topLeft != null) _topLeft.SetOnTouchListener(null);
                        if (_bottomRight != null) _bottomRight.SetOnTouchListener(null);
                        if (_topRight != null) _topRight.SetOnTouchListener(null);

                        // Unregister message handler
                        WeakReferenceMessenger.Default.Unregister<DisplayInfoChangedEventArgs>(this);

                        // Complete any pending tasks
                        _closeCompleted?.TrySetCanceled();
                        
                        // Clear references
                        _androidView = null;
                    }
                    catch (Exception ex)
                    {
                        ServiceHelper.LogService?.LogException(ex);
                    }
                }
                _disposed = true;
            }
        }
        base.Dispose(disposing);
    }
}