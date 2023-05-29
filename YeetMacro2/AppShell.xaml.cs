using YeetMacro2.Pages;
#if ANDROID
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Services;
#elif WINDOWS
using YeetMacro2.Platforms.Windows.Views;
#endif


namespace YeetMacro2;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

#if ANDROID
        this.Items.Add(new ShellContent()
        {
            Title = "Home",
            Route = "Home",
            ContentTemplate = new DataTemplate(typeof(AndroidHomePage))
        });
#elif WINDOWS
        this.Items.Add(new ShellContent()
        {
            Title = "Home",
            Route = "Home",
            ContentTemplate = new DataTemplate(typeof(WindowsHomePage))
        });
#endif

        this.Items.Add(new ShellContent()
        {
            Title = "Macro Manager",
            Route = "MacroManager",
            ContentTemplate = new DataTemplate(typeof(MacroManagerPage))
        });

#if ANDROID
        this.Items.Add(new ShellContent()
        {
            Title = "Developer",
            Route = "Developer",
            ContentTemplate = new DataTemplate(typeof(AndroidDeveloperPage))
        });
#endif
    }
}
