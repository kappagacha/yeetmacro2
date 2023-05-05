using Microsoft.Maui.Controls;

namespace YeetMacro2.Pages;

public partial class MacroManagerPage : ContentPage
{
	public MacroManagerPage()
	{
		InitializeComponent();
	}

    private void ExportEditor_SelectAll(object sender, TappedEventArgs e)
    {
        if (exportEditor.Text == null) return;
        exportEditor.Focus();
        exportEditor.CursorPosition = 0;
        exportEditor.SelectionLength = exportEditor.Text.Length;
    }
}