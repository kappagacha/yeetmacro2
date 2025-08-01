﻿using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Color = Android.Graphics.Color;
using Microsoft.Maui.Platform;

namespace YeetMacro2.Platforms.Android.Views;
public class StaticView : RelativeLayout, IShowable
{
    enum FormState { SHOWING, CLOSED };
    private readonly IWindowManager _windowManager;
    private readonly WindowManagerLayoutParams _layoutParams;
    private readonly VisualElement _visualElement;
    public VisualElement VisualElement => _visualElement;
    private FormState _state;
    public bool IsShowing { get => _state == FormState.SHOWING; }
    public Action OnClose { get; set; }
    private readonly global::Android.Views.View _androidView;
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
        setup(_layoutParams);
        RemoveView(_androidView);
        AddView(_androidView, new ViewGroup.LayoutParams(_layoutParams.Width, _layoutParams.Height));

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
        OnClose?.Invoke();
    }

    public void CloseCancel()
    {
        Close();
        OnClose?.Invoke();
    }

    public async Task<bool> WaitForClose()
    {
        return await Task.FromResult(false);
    }
}
