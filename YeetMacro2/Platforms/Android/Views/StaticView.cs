using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Color = Android.Graphics.Color;
using Microsoft.Maui.Platform;
using Java.Lang;
using Exception = System.Exception;

namespace YeetMacro2.Platforms.Android.Views;
public class StaticView : RelativeLayout, IShowable, IDisposable
{
    enum FormState { SHOWING, CLOSED };
    private readonly IWindowManager _windowManager;
    private readonly WindowManagerLayoutParams _layoutParams;
    private readonly VisualElement _visualElement;
    public VisualElement VisualElement => _visualElement;
    private volatile FormState _state;
    public bool IsShowing { get => _state == FormState.SHOWING; }
    public Action OnClose { get; set; }
    private global::Android.Views.View _androidView;
    private readonly object _stateLock = new object();
    private bool _disposed = false;
    //https://www.linkedin.com/pulse/6-floating-windows-android-keyboard-input-v%C3%A1clav-hodek/
    public StaticView(Context context, IWindowManager windowManager, VisualElement visualElement) : base(context)
    {
        _state = FormState.CLOSED;
        _layoutParams = new WindowManagerLayoutParams
        {
            //_layoutParams.Type = WindowManagerTypes.ApplicationOverlay;
            Type = OperatingSystem.IsAndroidVersionAtLeast(26) ? WindowManagerTypes.ApplicationOverlay : WindowManagerTypes.Phone,
            Format = Format.Translucent
        };
        _layoutParams.Flags |= WindowManagerFlags.NotFocusable;
        _layoutParams.Flags |= WindowManagerFlags.TranslucentNavigation;
        //_layoutParams.Flags |= WindowManagerFlags.LayoutInsetDecor;
        _layoutParams.Flags |= WindowManagerFlags.LayoutNoLimits;
        _layoutParams.Width = WindowManagerLayoutParams.WrapContent;
        _layoutParams.Height = WindowManagerLayoutParams.WrapContent;
        SetBackgroundColor(Color.Argb(90, 0, 0, 0));

        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            _layoutParams.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
        }

        //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/platform/native-forms
        _visualElement = visualElement;
        _windowManager = windowManager;

        _androidView = visualElement.ToPlatform(IPlatformApplication.Current.Application.Handler.MauiContext);
        _androidView.SetPadding(0, 0, 0, 0);
        AddView(_androidView, new ViewGroup.LayoutParams(_layoutParams.Width, _layoutParams.Height));
    }

    public void SetUpLayoutParameters(Action<WindowManagerLayoutParams> setup)
    {
        lock (_stateLock)
        {
            if (_disposed) return;
            
            setup(_layoutParams);
            RemoveView(_androidView);
            AddView(_androidView, new ViewGroup.LayoutParams(_layoutParams.Width, _layoutParams.Height));

            if (_state == FormState.SHOWING)
            {
                try
                {
                    _windowManager?.UpdateViewLayout(this, _layoutParams);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception updating layout in SetUpLayoutParameters: {ex.Message}");
                    // Fallback to recreating the view
                    Close();
                    Show();
                }
            }
        }
    }

    public void Show()
    {
        lock (_stateLock)
        {
            if (_state == FormState.SHOWING || _disposed) return;

            try
            {
                _state = FormState.SHOWING;
                _windowManager?.AddView(this, _layoutParams);
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
                OnClose?.Invoke();
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
        Close();
    }

    public async Task<bool> WaitForClose()
    {
        return await Task.FromResult(false);
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
                    
                    // Clear references
                    _androidView = null;
                }
                _disposed = true;
            }
        }
        base.Dispose(disposing);
    }
}
