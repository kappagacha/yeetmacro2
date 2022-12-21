namespace YeetMacro2.Controls;

public partial class PromptStringInput : ContentPage
{
	public PromptStringInput()
	{
		InitializeComponent();
	}

    private void ContentPage_Appearing(object sender, EventArgs e)
    {
        inputEntry.Focus();
    }
}