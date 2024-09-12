using YeetMacro2.ViewModels;

namespace YeetMacro2.Views;

public partial class ScriptNodeView : ContentView
{
    public static readonly BindableProperty ShowExecuteButtonProperty =
        BindableProperty.Create(nameof(ShowExecuteButton), typeof(bool), typeof(ScriptNodeView), false);
    public bool ShowExecuteButton
    {
        get { return (bool)GetValue(ShowExecuteButtonProperty); }
        set { SetValue(ShowExecuteButtonProperty, value); }
    }

    public static readonly BindableProperty MacroSetProperty =
        BindableProperty.Create(nameof(MacroSet), typeof(MacroSetViewModel), typeof(ScriptNodeView), null, propertyChanged: MacroSet_Changed);

    public MacroSetViewModel MacroSet
    {
        get { return (MacroSetViewModel)GetValue(MacroSetProperty); }
        set { SetValue(MacroSetProperty, value); }
    }

    private static void MacroSet_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is MacroSetViewModel macroSet)
        {
            var scriptNodeView = bindable as ScriptNodeView;
            scriptNodeView.BindingContext = macroSet;
        }
    }

    public ScriptNodeView()
    {
        InitializeComponent();
    }

    private void ScriptEditor_SelectAll(object sender, EventArgs e)
    {
        if (scriptEditor.Text == null) return;
        scriptEditor.Focus();
        scriptEditor.CursorPosition = 0;
        scriptEditor.SelectionLength = scriptEditor.Text.Length;
    }
}