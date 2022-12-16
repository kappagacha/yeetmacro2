using CommunityToolkit.Maui;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Services;

public static class ServiceRegistrationHelper
{
    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<HomeViewModel>();
        return mauiAppBuilder;
    }
}
