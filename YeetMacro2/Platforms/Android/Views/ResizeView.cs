using Android.Widget;
using static Android.Views.View;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables.Shapes;
using Color = Android.Graphics.Color;
using Microsoft.Maui.Platform;
using YeetMacro2.Platforms.Android.Services;

namespace YeetMacro2.Platforms.Android.Views;

public class ResizeView : RelativeLayout, IOnTouchListener, IShowable
{
    enum FormState { SHOWING, CLOSED };
    private IWindowManager _windowManager;
    private AndroidWindowManagerService _windowManagerService;
    private MainActivity _context;
    private WindowManagerLayoutParams _layoutParams;
    private ImageView _topLeft, _bottomRight, _topRight;
    int _x, _y, _displayWidth, _displayHeight;
    double _density;
    private VisualElement _visualElement;
    private const int MIN_WIDTH = 50, MIN_HEIGHT = 50;
    private FormState _state;
    public VisualElement VisualElement => _visualElement;
    TaskCompletionSource<bool> _closeCompleted;
    public Action OnShow { get; set; }
    public Action OnClose { get; set; }

    //https://www.linkedin.com/pulse/6-floating-windows-android-keyboard-input-v%C3%A1clav-hodek/

    public ResizeView(Context context, IWindowManager windowManager, AndroidWindowManagerService windowManagerService, VisualElement visualElement) : base(context)
    {
        _context = (MainActivity)context;
        _windowManager = windowManager;
        _windowManagerService = windowManagerService;
        _layoutParams = new WindowManagerLayoutParams();
        _layoutParams.Type = WindowManagerTypes.ApplicationOverlay;
        _layoutParams.Format = Format.Translucent;
        _layoutParams.Flags |= WindowManagerFlags.NotFocusable;
        _layoutParams.Flags |= WindowManagerFlags.TranslucentNavigation;
        _layoutParams.Flags |= WindowManagerFlags.LayoutNoLimits;

        _layoutParams.Gravity = GravityFlags.Top | GravityFlags.Left;
        var density = DeviceDisplay.MainDisplayInfo.Density;
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

        RelativeLayout.LayoutParams topLeftParams = new RelativeLayout.LayoutParams(cornerShapeSize, cornerShapeSize);
        topLeftParams.AddRule(LayoutRules.AlignParentTop);
        topLeftParams.AddRule(LayoutRules.AlignParentLeft);
        _topLeft.Clickable = true;
        _topLeft.SetOnTouchListener(this);

        _bottomRight = new ImageView(_context);
        _bottomRight.SetImageDrawable(shape);
        _bottomRight.SetX(cornerShapeSize / 2.0f);
        _bottomRight.SetY(cornerShapeSize / 2.0f);

        RelativeLayout.LayoutParams bottomRightParams = new RelativeLayout.LayoutParams(cornerShapeSize, cornerShapeSize);
        bottomRightParams.AddRule(LayoutRules.AlignParentBottom);
        bottomRightParams.AddRule(LayoutRules.AlignParentRight);
        _bottomRight.Clickable = true;
        _bottomRight.SetOnTouchListener(this);

        _topRight = new ImageView(_context);
        _topRight.SetImageResource(global::Android.Resource.Drawable.IcMenuCloseClearCancel);
        _topRight.SetColorFilter(Color.Red);    //https://stackoverflow.com/questions/1309629/how-to-change-colors-of-a-drawable-in-android

        RelativeLayout.LayoutParams topRightParams = new RelativeLayout.LayoutParams(cornerShapeSize, cornerShapeSize);
        topRightParams.AddRule(LayoutRules.AlignParentTop);
        topRightParams.AddRule(LayoutRules.AlignParentRight);
        _topRight.Clickable = true;
        _topRight.SetOnTouchListener(this);

        SetBackgroundColor(global::Android.Graphics.Color.Argb(70, 0, 0, 0));

        _state = FormState.CLOSED;
        InitDisplay();
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;

        // https://www.andreasnesheim.no/embedding-net-maui-pages-into-your-net-android-ios-application/
        _visualElement = visualElement;
        var mauiContext = new MauiContext(MauiApplication.Current.Services, context);
        var androidView = visualElement.ToContainerView(mauiContext);

        AddView(androidView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        AddView(_topLeft, topLeftParams);
        AddView(_bottomRight, bottomRightParams);
        AddView(_topRight, topRightParams);


        androidView.Clickable = true;
        Clickable = true;
    }

    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
        InitDisplay();
    }

    private void InitDisplay()
    {
        var displayInfo = DeviceDisplay.MainDisplayInfo;
        _displayWidth = (int)displayInfo.Width;
        _displayHeight = (int)displayInfo.Height;
        _density = displayInfo.Density;
        _layoutParams.Width = (int)(_displayWidth * 0.75);
        _layoutParams.Height = (int)(_displayHeight * 0.75);
        _layoutParams.X = (int)(_displayWidth * 0.25);
        _layoutParams.Y = (int)(_displayHeight * 0.25);

        if (_state == FormState.SHOWING)
        {
            _windowManager.UpdateViewLayout(this, _layoutParams);
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
        if (_state != FormState.SHOWING)
        {
            _state = FormState.SHOWING;
            _windowManager.AddView(this, _layoutParams);
            _closeCompleted = new TaskCompletionSource<bool>();
            OnShow?.Invoke();
        }
    }

    public void Close()
    {
        if (_state == FormState.SHOWING)
        {
            _windowManager.RemoveView(this);
            _state = FormState.CLOSED;
            _closeCompleted.TrySetResult(true);
            OnClose?.Invoke();
        }
    }

    public void CloseCancel()
    {
        if (_state == FormState.SHOWING)
        {
            _windowManager.RemoveView(this);
            _state = FormState.CLOSED;
            _closeCompleted.TrySetResult(false);
            OnClose?.Invoke();
        }
    }

    private void EnableKeyboard()
    {
        if (Focusable)
        {
            _layoutParams.Flags &= ~WindowManagerFlags.NotFocusable;
            _windowManager.UpdateViewLayout(this, _layoutParams);
        }
    }

    private void DisableKeyboard()
    {
        _layoutParams.Flags |= WindowManagerFlags.NotFocusable;
        _windowManager.UpdateViewLayout(this, _layoutParams);
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

                if (v == _topLeft)
                {
                    _x = nowX;
                    _y = nowY;
                    _layoutParams.X = _layoutParams.X + movedX;
                    _layoutParams.Y = _layoutParams.Y + movedY;
                    _windowManager.UpdateViewLayout(this, _layoutParams);
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

                    _windowManager.UpdateViewLayout(this, _layoutParams);
                    _visualElement.Layout(new Microsoft.Maui.Graphics.Rect(0, 0, _layoutParams.Width / _density, _layoutParams.Height / _density));
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
