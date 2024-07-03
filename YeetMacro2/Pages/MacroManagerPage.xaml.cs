using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.Pages;

public partial class MacroManagerPage : ContentPage
{
	public MacroManagerPage()
	{
		InitializeComponent();
    }

    private void ExportEditor_SelectAll(object sender, TappedEventArgs e)
    {
        var exportEditor = (Editor)tabView.Content.FindByName("exportEditor");
        if (exportEditor.Text == null) return;
        exportEditor.Focus();
        exportEditor.CursorPosition = 0;
        exportEditor.SelectionLength = exportEditor.Text.Length;
    }

    private async void WeeklyDayStart_Clicked(object sender, EventArgs e)
    {
        var macroSet = ((ImageButton)sender).BindingContext as MacroSet;
        var options = Enum.GetValues<DayOfWeek>().Select(oct => oct.ToString()).ToArray();
        var selectedOption = await ServiceHelper.GetService<IInputService>().SelectOption("Select option", options);
        if (!String.IsNullOrEmpty(selectedOption) && selectedOption != "cancel" && selectedOption != "ok") macroSet.WeeklyStartDay = Enum.Parse<DayOfWeek>(selectedOption);
    }
}