using YeetMacro2.Services;
using Microsoft.EntityFrameworkCore;
using YeetMacro2.Data.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Microsoft.Maui.LifecycleEvents;
using YeetMacro2.Platforms.Windows.ViewModels;
using YeetMacro2.Platforms.Windows.Services;

namespace YeetMacro2.Platforms;

public static class PlatformServiceRegistrationHelper
{
    public static MauiAppBuilder RegisterPlatformServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<WindowsHomeViewModel>();
        mauiAppBuilder.Services.AddSingleton<WindowsScreenService>();
        mauiAppBuilder.Services.AddSingleton<IInputService, WindowsInputService>();
        mauiAppBuilder.Services.AddSingleton<IScreenService>(sp => sp.GetRequiredService<WindowsScreenService>());
        mauiAppBuilder.Services.AddSingleton<IRecorderService>(sp => sp.GetRequiredService<WindowsScreenService>());

        mauiAppBuilder.Services.AddYeetMacroData(setup =>
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "yeetmacro.db3");
            setup.UseSqlite($"Filename={dbPath}");
        }, ServiceLifetime.Transient);

        // https://github.com/dotnet/maui/discussions/2370
        mauiAppBuilder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(wndLifeCycleBuilder =>
            {
                wndLifeCycleBuilder.OnWindowCreated(window =>
                {
                    IntPtr nativeWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    WindowId win32WindowsId = Win32Interop.GetWindowIdFromWindow(nativeWindowHandle);
                    AppWindow winuiAppWindow = AppWindow.GetFromWindowId(win32WindowsId);

                    // https://stackoverflow.com/questions/71806578/maui-how-to-remove-the-title-bar-and-fix-the-window-size
                    if (winuiAppWindow.Presenter is OverlappedPresenter p)
                    {
                        p.SetBorderAndTitleBar(false, false);
                        
                    }

                    const int width = 1024;
                    const int height = 768;
                    winuiAppWindow.MoveAndResize(new RectInt32(1920 / 2 - width / 2, 1080 / 2 - height / 2, width, height));
                });
            });
        });

        return mauiAppBuilder;
    }
}
