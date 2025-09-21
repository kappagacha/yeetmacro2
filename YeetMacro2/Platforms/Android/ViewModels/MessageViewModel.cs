using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class MessageViewModel : ObservableObject
{
    IToastService _toastService;
    public MessageViewModel(IToastService toastSevice)
    {
        _toastService = toastSevice;
    }

    [ObservableProperty]
    string _message;

    [RelayCommand]
    public async Task CopyMessageToClipboard(String message)
    {
        await Clipboard.Default.SetTextAsync(message);
        _toastService.Show($"Copied message to clipboard: {message}");
    }
}