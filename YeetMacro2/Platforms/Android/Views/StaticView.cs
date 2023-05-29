using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Color = Android.Graphics.Color;
using Microsoft.Maui.Platform;

namespace YeetMacro2.Platforms.Android.Views;
public class StaticView : RelativeLayout, IShowable
{
    enum FormState { SHOWING, CLOSED };
    private IWindowManager _windowManager;
    private WindowManagerLayoutParams _layoutParams;
    private VisualElement _visualElement;
    public VisualElement VisualElement => _visualElement;
    private FormState _state;

    //https://www.linkedin.com/pulse/6-floating-windows-android-keyboard-input-v%C3%A1clav-hodek/
    public StaticView(Context context, IWindowManager windowManager, VisualElement visualElement) : base(context)
    {
        _state = FormState.CLOSED;
        _layoutParams = new WindowManagerLayoutParams();
        _layoutParams.Type = WindowManagerTypes.ApplicationOverlay;
        _layoutParams.Format = Format.Translucent;
        _layoutParams.Flags |= WindowManagerFlags.NotFocusable;
        _layoutParams.Flags |= WindowManagerFlags.TranslucentNavigation;
        //_layoutParams.Flags |= WindowManagerFlags.LayoutInsetDecor;
        _layoutParams.Flags |= WindowManagerFlags.LayoutNoLimits;
        _layoutParams.Width = WindowManagerLayoutParams.WrapContent;
        _layoutParams.Height = WindowManagerLayoutParams.WrapContent;
        SetBackgroundColor(Color.Argb(90, 0, 0, 0));

        //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/platform/native-forms
        _visualElement = visualElement;
        _windowManager = windowManager;

        var mauiContext = new MauiContext(MauiApplication.Current.Services, context);
        var androidView = visualElement.ToPlatform(mauiContext);
        androidView.SetPadding(0, 0, 0, 0);
        var density = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Density;
        _layoutParams.Width = (int)(_visualElement.WidthRequest * density);
        _layoutParams.Height = (int)(_visualElement.HeightRequest * density);
        AddView(androidView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
    }

    public void SetUpLayoutParameters(Action<WindowManagerLayoutParams> setup)
    {
        setup(_layoutParams);
        if (_state == FormState.SHOWING)
        {
            Close();
            Show();
        }
    }

    public void Show()
    {
        if (_state == FormState.SHOWING) return;

        _state = FormState.SHOWING;
        _windowManager.AddView(this, _layoutParams);
    }

    public void Close()
    {
        if (_state == FormState.CLOSED) return;

        _windowManager.RemoveView(this);
        _state = FormState.CLOSED;
    }

    public void CloseCancel()
    {
        Close();
    }

    public async Task<bool> WaitForClose()
    {
        return await Task.FromResult(false);
    }
}
