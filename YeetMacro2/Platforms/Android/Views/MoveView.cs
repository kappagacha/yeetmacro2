using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Platform;
using Color = Android.Graphics.Color;

namespace YeetMacro2.Platforms.Android.Views;

public interface IMovable
{
    bool IsMoving { get; set; }
}

public class MoveView : LinearLayout, IShowable
{
    private IWindowManager _windowManager;
    private MainActivity _context;
    private WindowManagerLayoutParams _layoutParams;
    TaskCompletionSource<bool> _closeCompleted;
    enum FormState { SHOWING, CLOSED };
    int _x, _y;
    private VisualElement _visualElement;
    public VisualElement VisualElement => _visualElement;
    bool _isMoving = false;
    FormState _state;
    public bool IsShowing { get => _state == FormState.SHOWING; }

    public MoveView(Context context, IWindowManager windowManager, VisualElement visualElement) : base(context)
    {
        _state = FormState.CLOSED;
        _context = (MainActivity)context;
        _layoutParams = new WindowManagerLayoutParams();
        _layoutParams.Type = WindowManagerTypes.ApplicationOverlay;
        _layoutParams.Format = Format.Translucent;
        _layoutParams.Flags |= WindowManagerFlags.NotFocusable;
        _layoutParams.Flags |= WindowManagerFlags.TranslucentNavigation;
        _layoutParams.Width = WindowManagerLayoutParams.WrapContent;
        _layoutParams.Height = WindowManagerLayoutParams.WrapContent;
        _layoutParams.Gravity = GravityFlags.Top;

        //SetBackgroundColor(Color.Argb(70, 40, 40, 40));
        _windowManager = windowManager;
        _visualElement = visualElement;

        var mauiContext = new MauiContext(MauiApplication.Current.Services, context);
        var androidView = visualElement.ToPlatform(mauiContext);
        androidView.SetPadding(0, 0, 0, 0);
        var density = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Density;
        _layoutParams.Width = (int)(_visualElement.WidthRequest * density);
        _layoutParams.Height = (int)(_visualElement.HeightRequest * density);

        AddView(androidView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
    }

    public void Show()
    {
        if (_state == FormState.CLOSED)
        {
            _windowManager.AddView(this, _layoutParams);
            _state = FormState.SHOWING;
            _closeCompleted = new TaskCompletionSource<bool>();
        }
    }

    public void Close()
    {
        if (_state == FormState.SHOWING)
        {
            _windowManager.RemoveView(this);
            _state = FormState.CLOSED;
            _closeCompleted.TrySetResult(true);
        }
    }

    public void CloseCancel()
    {
        if (_state == FormState.SHOWING)
        {
            _windowManager.RemoveView(this);
            _state = FormState.CLOSED;
            _closeCompleted.TrySetResult(false);
        }
    }

    public override bool OnInterceptTouchEvent(MotionEvent e)
    {
        var movable = (IMovable)_visualElement.BindingContext;
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
                _layoutParams.X = _layoutParams.X + movedX;
                _layoutParams.Y = _layoutParams.Y + movedY;
                if (movable != null && (movedX != 0 || movedY != 0))
                {
                    _isMoving = true;
                    movable.IsMoving = _isMoving;
                    _windowManager.UpdateViewLayout(this, _layoutParams);
                }
                break;
            case MotionEventActions.Up:
                if (_isMoving)
                {
                    _isMoving = false;
                    if (movable != null) { movable.IsMoving = _isMoving; }
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
    }

    public async Task<bool> WaitForClose()
    {
        return await _closeCompleted.Task;
    }
}
