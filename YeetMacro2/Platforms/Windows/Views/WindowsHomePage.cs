using YeetMacro2.Platforms.Windows.ViewModels;
using YeetMacro2.ViewModels;
using YeetMacro2.Views;

namespace YeetMacro2.Platforms.Windows.Views;

public class WindowsHomePage : ContentPage
{
    public WindowsHomePage()
    {
        ViewModelLocator.SetViewModelType(this, typeof(WindowsHomeViewModel));
        var button = new Button();
        button.SetBinding(Button.CommandProperty, nameof(WindowsHomeViewModel.TestCommand));
        button.Text = "Test";
        //Content = new TreeView();
        Content = button;
    }
}