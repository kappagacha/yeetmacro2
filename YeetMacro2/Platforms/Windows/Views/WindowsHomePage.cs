using System.Diagnostics;
using System.Windows.Input;
using YeetMacro2.Views;

namespace YeetMacro2.Platforms.Windows.Views;

public class WindowsHomePage : ContentPage
{
    public WindowsHomePage()
    {
        Content = new VerticalStackLayout
        {
            Children = {
                new TreeView()
                //new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Hi from Windows" }
                //new Button { HorizontalOptions = LayoutOptions.Center, Command = OpenAppFolderCommand }
            }
        };
    }

    //ICommand OpenAppFolderCommand { get; } = new Command(() => { 
    //    Process.Start()
    //});
}