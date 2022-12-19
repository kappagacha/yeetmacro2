using CommunityToolkit.Mvvm.ComponentModel;

namespace YeetMacro2.ViewModels;

public partial class LogViewModel : ObservableObject
{
    [ObservableProperty]
    string _message;
}
