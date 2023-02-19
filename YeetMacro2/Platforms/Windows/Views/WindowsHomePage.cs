using System.Diagnostics;
using System.Windows.Input;

namespace YeetMacro2.Platforms.Windows.Views;

public class WindowsHomePage : ContentPage
{
    public WindowsHomePage()
    {
        Content = new VerticalStackLayout
        {
            Children = {
                new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Hi from Windows" }
                //new Button { HorizontalOptions = LayoutOptions.Center, Command = OpenAppFolderCommand }
            }
        };
    }

    //ICommand OpenAppFolderCommand { get; } = new Command(() => { 
    //    Process.Start()
    //});
}