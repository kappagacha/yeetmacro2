using CommunityToolkit.Maui.Alerts;

namespace YeetMacro2.Services;

public interface IToastService
{
    void Show(string text);
}

public class ToastService : IToastService
{
    public void Show(string text)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var toast = Toast.Make(text);
            await toast.Show();
        });
    }
}
