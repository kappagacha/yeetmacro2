using CommunityToolkit.Mvvm.ComponentModel;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class MessageViewModel : ObservableObject
{
    [ObservableProperty]
    string _message;
}