using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Color = Android.Graphics.Color;
using Microsoft.Maui.Platform;
using Java.Lang;
using Exception = System.Exception;

namespace YeetMacro2.Platforms.Android.Views;
public class FormsView : RelativeLayout, IShowable, IDisposable
{
    enum FormState { SHOWING, CLOSED };
    private readonly IWindowManager _windowManager;
    private readonly WindowManagerLayoutParams _layoutParams;
    private readonly VisualElement _visualElement;
    public VisualElement VisualElement => _visualElement;
    private volatile FormState _state;
    private readonly global::Android.Views.View _androidView;
    public bool IsModal { get; set; } = true;
    TaskCompletionSource<bool> _closeCompleted;
    public bool IsShowing { get => _state == FormState.SHOWING; }
    private readonly object _stateLock = new object();
    private bool _disposed = false;

    //https://www.linkedin.com/pulse/6-floating-windows-android-keyboard-input-v%C3%A1clav-hodek/
    public FormsView(Context context, IWindowManager windowManager, VisualElement visualElement) : base(context)
    {
        _windowManager = windowManager;
        _layoutParams = new WindowManagerLayoutParams
        {
            //_layoutParams.Type = WindowManagerTypes.ApplicationOverlay;
            Type = OperatingSystem.IsAndroidVersionAtLeast(26) ? WindowManagerTypes.ApplicationOverlay : WindowManagerTypes.Phone,
            Format = Format.Translucent
        };
        _layoutParams.Flags |= WindowManagerFlags.LayoutNoLimits;
        //_layoutParams.Flags |= WindowManagerFlags.Fullscreen;
        _layoutParams.Flags |= WindowManagerFlags.TranslucentNavigation;
        _layoutParams.Flags |= WindowManagerFlags.LayoutInScreen;
        //_layoutParams.Gravity = GravityFlags.Top | GravityFlags.Left;
        _layoutParams.Width = WindowManagerLayoutParams.MatchParent;
        _layoutParams.Height = WindowManagerLayoutParams.MatchParent;
        SetBackgroundColor(Color.Argb(70, 0, 0, 0));

        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            _layoutParams.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
        }

        _state = FormState.CLOSED;

        _visualElement = visualElement;
        _androidView = visualElement.ToPlatform(IPlatformApplication.Current.Application.Handler.MauiContext);
        _androidView.SetPadding(0, 0, 0, 0);
        AddView(_androidView, new ViewGroup.LayoutParams(_layoutParams.Width, _layoutParams.Height));

        _androidView.Clickable = true;
        _androidView.Click += FormsView_Click;
    }

    public void SetUpLayoutParameters(Action<WindowManagerLayoutParams> setup)
    {
        setup(_layoutParams);
        RemoveView(_androidView);
        AddView(_androidView, new ViewGroup.LayoutParams(_layoutParams.Width, _layoutParams.Height));

        if (_state == FormState.SHOWING)
        {
            Close();
            Show();
        }
    }

    public void SetBackgroundToTransparent()
    {
        SetBackgroundColor(Color.Transparent);
    }

    public void DisableTranslucentNavigation()
    {
        _layoutParams.Flags &= ~WindowManagerFlags.TranslucentNavigation;
        //_layoutParams.Flags &= ~WindowManagerFlags.LayoutNoLimits;

        if (_state == FormState.SHOWING)
        {
            Close();
            Show();
        }
    }

    public void SetIsTouchable(bool touchable)
    {
        lock (_stateLock)
        {
            if (_disposed) return;
            
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
                //to allow clicking through the window, alpha needs to be 0.8 or below
                _layoutParams.Alpha = 0.5f;
            }

            if (_state == FormState.SHOWING)
            {
                try
                {
                    _windowManager?.UpdateViewLayout(this, _layoutParams);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception updating layout in SetIsTouchable: {ex.Message}");
                    // Fallback to recreating the view
                    Close();
                    Show();
                }
            }
        }
    }

    private void FormsView_Click(object sender, System.EventArgs e)
    {
        if (IsModal)
        {
            CloseCancel();
        }
    }

    public void Show()
    {
        lock (_stateLock)
        {
            if (_state != FormState.SHOWING && !_disposed)
            {
                try
                {
                    _state = FormState.SHOWING;
                    _windowManager?.AddView(this, _layoutParams);
                    _closeCompleted = new TaskCompletionSource<bool>();
                }
                catch (WindowManagerBadTokenException ex)
                {
                    // Activity was destroyed
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
    }

    public void Close()
    {
        lock (_stateLock)
        {
            if (_state == FormState.CLOSED || _disposed) return;

            try
            {
                _windowManager?.RemoveView(this);
                _closeCompleted?.TrySetResult(true);
                _state = FormState.CLOSED;
            }
            catch (IllegalArgumentException)
            {
                // View was not attached to window manager
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
                // View was not attached to window manager
                _state = FormState.CLOSED;
            }
            catch (Exception ex)
            {
                _state = FormState.CLOSED;
                System.Diagnostics.Debug.WriteLine($"Exception in CloseCancel: {ex.Message}");
            }
        }
    }

    //https://stackoverflow.com/questions/12745848/how-to-block-until-an-event-is-fired-in-c-sharp
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
                    if (_androidView != null)
                    {
                        _androidView.Click -= FormsView_Click;
                    }

                    // Complete any pending tasks
                    _closeCompleted?.TrySetCanceled();
                }
                _disposed = true;
            }
        }
        base.Dispose(disposing);
    }
}
