using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Color = Android.Graphics.Color;
using Microsoft.Maui.Platform;

namespace YeetMacro2.Platforms.Android.Views;
public class FormsView : RelativeLayout, IShowable
{
    enum FormState { SHOWING, CLOSED };
    private IWindowManager _windowManager;
    private MainActivity _context;
    private WindowManagerLayoutParams _layoutParams;
    int _displayWidth, _displayHeight;
    double _density;
    private VisualElement _visualElement;
    public VisualElement VisualElement => _visualElement;
    private FormState _state;
    public bool IsModal { get; set; } = true;
    TaskCompletionSource<bool> _closeCompleted;

    //https://www.linkedin.com/pulse/6-floating-windows-android-keyboard-input-v%C3%A1clav-hodek/
    public FormsView(Context context, IWindowManager windowManager, VisualElement visualElement) : base(context)
    {
        _context = (MainActivity)context;
        _windowManager = windowManager;

        _layoutParams = new WindowManagerLayoutParams();
        _layoutParams.Type = WindowManagerTypes.ApplicationOverlay;
        _layoutParams.Format = Format.Translucent;
        _layoutParams.Flags |= WindowManagerFlags.TranslucentNavigation;
        _layoutParams.Flags |= WindowManagerFlags.LayoutNoLimits;
        _layoutParams.Gravity = GravityFlags.Top | GravityFlags.Left;
        SetBackgroundColor(Color.Argb(70, 0, 0, 0));

        _state = FormState.CLOSED;
        InitDisplay();
        DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;

        //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/platform/native-forms
        _visualElement = visualElement;
        var mauiContext = new MauiContext(MauiApplication.Current.Services, context);
        var androidView = visualElement.ToPlatform(mauiContext);
        androidView.SetPadding(0, 0, 0, 0);
        _visualElement.Layout(new Microsoft.Maui.Graphics.Rect(0, 0, _layoutParams.Width / _density, _layoutParams.Height / _density));
        AddView(androidView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

        androidView.Clickable = true;
        androidView.Click += FormsView_Click;
    }

    public void SetBackgroundToTransparent()
    {
        SetBackgroundColor(Color.Transparent);
    }

    public void DisableTranslucentNavigation()
    {
        _layoutParams.Flags &= ~WindowManagerFlags.TranslucentNavigation;
        _layoutParams.Flags &= ~WindowManagerFlags.LayoutNoLimits;

        if (_state == FormState.SHOWING)
        {
            Close();
            Show();
        }
    }

    public void SetIsTouchable(bool touchable)
    {
        if (touchable)
        {
            _layoutParams.Flags &= ~WindowManagerFlags.NotTouchable;
            _layoutParams.Flags &= ~WindowManagerFlags.NotTouchModal;
            _layoutParams.Flags &= ~WindowManagerFlags.NotFocusable;
            _layoutParams.Alpha = 1.0f;
        }
        else
        {
            _layoutParams.Flags |= WindowManagerFlags.NotTouchable;
            _layoutParams.Flags |= WindowManagerFlags.NotTouchModal;
            _layoutParams.Flags |= WindowManagerFlags.NotFocusable;
            //https://medium.com/androiddevelopers/untrusted-touch-events-2c0e0b9c374c#776e
            //to allow clicking past the window, alpha needs to be 0.8 or below
            _layoutParams.Alpha = 0.8f;
        }

        if (_state == FormState.SHOWING)
        {
            Close();
            Show();
        }
    }

    private void FormsView_Click(object sender, System.EventArgs e)
    {
        if (IsModal)
        {
            CloseCancel();
        }
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
        _layoutParams.Width = _displayWidth;
        _layoutParams.Height = _displayHeight;
        _layoutParams.X = 0;
        _layoutParams.Y = 0;

        if (_state == FormState.SHOWING)
        {
            _windowManager.UpdateViewLayout(this, _layoutParams);
            _visualElement.Layout(new Microsoft.Maui.Graphics.Rect(0, 0, _layoutParams.Width / _density, _layoutParams.Height / _density));
        }
    }

    public void Show()
    {
        if (_state != FormState.SHOWING)
        {
            _state = FormState.SHOWING;
            _windowManager.AddView(this, _layoutParams);
            _closeCompleted = new TaskCompletionSource<bool>();
            InitDisplay();
        }
    }

    public void Close()
    {
        if (_state == FormState.CLOSED) return;

        _windowManager.RemoveView(this);
        _closeCompleted.SetResult(true);
        var loc = new int[2];
        this.GetLocationOnScreen(loc);
        //Console.WriteLine($"x{loc[0]} y{loc[1]}");
        _state = FormState.CLOSED;
    }

    public void CloseCancel()
    {
        _windowManager.RemoveView(this);
        _state = FormState.CLOSED;
        _closeCompleted.SetResult(false);
        var loc = new int[2];
        this.GetLocationOnScreen(loc);
        Console.WriteLine($"x{loc[0]} y{loc[1]}");
    }

    //https://stackoverflow.com/questions/12745848/how-to-block-until-an-event-is-fired-in-c-sharp
    public async Task<bool> WaitForClose()
    {
        return await _closeCompleted.Task;
    }
}
