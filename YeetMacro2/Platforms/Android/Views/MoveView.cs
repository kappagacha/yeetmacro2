using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Platform;
using YeetMacro2.Data.Models;
using Java.Lang;
using Exception = System.Exception;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Views;

public interface IMovable
{
    bool IsMoving { get; set; }
    Microsoft.Maui.Graphics.Point Location { get; set; }
}

public class MoveView : LinearLayout, IShowable, IDisposable
{
    private readonly IWindowManager _windowManager;
    //private MainActivity _context;
    private readonly WindowManagerLayoutParams _layoutParams;
    TaskCompletionSource<bool> _closeCompleted;
    enum FormState { SHOWING, CLOSED };
    int _x, _y;
    private readonly VisualElement _visualElement;
    public VisualElement VisualElement => _visualElement;
    bool _isMoving = false;
    private volatile FormState _state;
    public bool IsShowing { get => _state == FormState.SHOWING; }
    private readonly object _stateLock = new object();
    private bool _disposed = false;
    private global::Android.Views.View _androidView;

    public MoveView(Context context, IWindowManager windowManager, VisualElement visualElement) : base(context)
    {
        _state = FormState.CLOSED;
        //_context = (MainActivity)context;
        _layoutParams = new WindowManagerLayoutParams
        {
            //_layoutParams.Type = WindowManagerTypes.ApplicationOverlay;
            Type = OperatingSystem.IsAndroidVersionAtLeast(26) ? WindowManagerTypes.ApplicationOverlay : WindowManagerTypes.Phone,
            Format = Format.Translucent
        };
        _layoutParams.Flags |= WindowManagerFlags.NotFocusable;
        _layoutParams.Flags |= WindowManagerFlags.TranslucentNavigation;
        //_layoutParams.Flags |= WindowManagerFlags.LayoutNoLimits;
        _layoutParams.Width = WindowManagerLayoutParams.WrapContent;
        _layoutParams.Height = WindowManagerLayoutParams.WrapContent;
        _layoutParams.Gravity = GravityFlags.Top | GravityFlags.Left;

        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            _layoutParams.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
        }

        //SetBackgroundColor(Color.Argb(70, 40, 40, 40));
        _windowManager = windowManager;
        _visualElement = visualElement;

        _androidView = visualElement.ToPlatform(IPlatformApplication.Current.Application.Handler.MauiContext);
        _androidView.SetPadding(0, 0, 0, 0);
        var density = DisplayHelper.DisplayInfo.Density;
        _layoutParams.Width = (int)(_visualElement.WidthRequest * density);
        _layoutParams.Height = (int)(_visualElement.HeightRequest * density);

        AddView(_androidView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
    }

    public void Show()
    {
        lock (_stateLock)
        {
            if (_state == FormState.SHOWING || _disposed) return;

            try
            {
                var movable = _visualElement?.BindingContext as IMovable;
                if (movable != null)
                {
                    _layoutParams.X = (int)movable.Location.X;
                    _layoutParams.Y = (int)movable.Location.Y;
                }
                _windowManager?.AddView(this, _layoutParams);
                _state = FormState.SHOWING;
                _closeCompleted = new TaskCompletionSource<bool>();
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

    public void SyncLocation()
    {
        lock (_stateLock)
        {
            if (_disposed) return;
            
            try
            {
                var movable = _visualElement?.BindingContext as IMovable;
                if (movable != null)
                {
                    _layoutParams.X = (int)movable.Location.X;
                    _layoutParams.Y = (int)movable.Location.Y;
                    if (_state == FormState.SHOWING)
                    {
                        _windowManager?.UpdateViewLayout(this, _layoutParams);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in SyncLocation: {ex.Message}");
            }
        }
    }

    public override bool OnInterceptTouchEvent(MotionEvent e)
    {
        if (_disposed) return false;
        
        try
        {
            var movable = _visualElement?.BindingContext as IMovable;
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _x = (int)e.RawX;
                    _y = (int)e.RawY;
                    break;
                case MotionEventActions.Move:
                    int nowX = (int)e.RawX;
                    int nowY = (int)e.RawY;
                    int movedX = nowX - _x;
                    int movedY = nowY - _y;
                    _x = nowX;
                    _y = nowY;
                    _layoutParams.X += movedX;
                    _layoutParams.Y += movedY;
                    if (movable != null && (movedX != 0 || movedY != 0))
                    {
                        _isMoving = true;
                        movable.IsMoving = _isMoving;
                        lock (_stateLock)
                        {
                            if (_state == FormState.SHOWING && !_disposed)
                            {
                                _windowManager?.UpdateViewLayout(this, _layoutParams);
                            }
                        }
                    }
                    break;
                case MotionEventActions.Up:
                    if (_isMoving)
                    {
                        _isMoving = false;
                        if (movable != null) 
                        { 
                            movable.IsMoving = _isMoving;
                            movable.Location = new Microsoft.Maui.Graphics.Point(_layoutParams.X, _layoutParams.Y);
                        }
                        return true;
                    }
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in OnInterceptTouchEvent: {ex.Message}");
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
