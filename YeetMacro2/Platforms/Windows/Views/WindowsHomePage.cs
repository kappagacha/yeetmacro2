using YeetMacro2.Views;

namespace YeetMacro2.Platforms.Windows.Views;

public class WindowsHomePage : ContentPage
{
    public WindowsHomePage()
    {
        Content = new PatternTreeView();
        //{
        //    Children = {
        //        new TreeView()
        //        //new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Hi from Windows" }
        //        //new Button { HorizontalOptions = LayoutOptions.Center, Command = OpenAppFolderCommand }
        //    }
        //};
    }

    //ICommand OpenAppFolderCommand { get; } = new Command(() => { 
    //    Process.Start()
    //});
}