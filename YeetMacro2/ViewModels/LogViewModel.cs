using CommunityToolkit.Mvvm.ComponentModel;
namespace YeetMacro2.ViewModels;

public partial class LogViewModel : ObservableObject
{
    [ObservableProperty]
    string _debug;
    [ObservableProperty]
    string _info;

    public LogViewModel()
    {
    }
}
