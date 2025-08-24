using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Views;

public partial class SettingNodeView : ContentView
{
    public static readonly BindableProperty MacroSetProperty =
        BindableProperty.Create(nameof(MacroSet), typeof(MacroSetViewModel), typeof(SettingNodeView), null);

    public MacroSetViewModel MacroSet
    {
        get { return (MacroSetViewModel)GetValue(MacroSetProperty); }
        set { SetValue(MacroSetProperty, value); }
    }


    public static readonly BindableProperty SubViewProperty =
        BindableProperty.Create(nameof(SubView), typeof(ParentSetting), typeof(SettingNodeView), null);

    public ParentSetting SubView
    {
        get { return (ParentSetting)GetValue(SubViewProperty); }
        set { SetValue(SubViewProperty, value); }
    }

    public SettingNodeView()
    {
        InitializeComponent();
        
        // Register templates with VirtualDynamicTemplateSelector
        // This ensures templates are available for SettingCoreNodeView
        var selector = new Converters.VirtualDynamicTemplateSelector();
        selector.Root = this;
    }
}