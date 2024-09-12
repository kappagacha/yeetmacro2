using YeetMacro2.ViewModels;

namespace YeetMacro2.Views;

public partial class SettingNodeView : ContentView
{
    public static readonly BindableProperty IsSubViewProperty =
        BindableProperty.Create(nameof(IsSubView), typeof(bool), typeof(SettingNodeView), false);
    public bool IsSubView
    {
        get { return (bool)GetValue(IsSubViewProperty); }
        set { SetValue(IsSubViewProperty, value); }
    }

    public static readonly BindableProperty MacroSetProperty =
        BindableProperty.Create(nameof(MacroSet), typeof(MacroSetViewModel), typeof(SettingNodeView), null, propertyChanged: MacroSet_Changed);

    public MacroSetViewModel MacroSet
    {
        get { return (MacroSetViewModel)GetValue(MacroSetProperty); }
        set { SetValue(MacroSetProperty, value); }
    }

    private static void MacroSet_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is MacroSetViewModel macroSet)
        {
            var settingNodeView = bindable as SettingNodeView;
            settingNodeView.BindingContext = macroSet;
        }
    }

    public SettingNodeView()
    {
        InitializeComponent();
    }
}