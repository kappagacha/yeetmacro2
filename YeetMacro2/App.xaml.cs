namespace YeetMacro2;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();

        AppTheme currentTheme = Application.Current.RequestedTheme;
        Application.Current.UserAppTheme = AppTheme.Dark;
    }
}
