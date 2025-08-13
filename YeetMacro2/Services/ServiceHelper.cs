namespace YeetMacro2.Services;

// https://stackoverflow.com/questions/72939282/net-maui-dependency-injection-in-platform-specific-code
public static class ServiceHelper
{
    public static T GetService<T>() => (T)IPlatformApplication.Current.Services.GetService(typeof(T));
    public static object GetService(Type type) => IPlatformApplication.Current.Services.GetService(type);

    public static string[] ListAssets(string folder) => 
#if WINDOWS10_0_17763_0_OR_GREATER
        // https://github.com/dotnet/maui/blob/main/src/Essentials/src/FileSystem/FileSystem.uwp.cs
        Directory.GetFileSystemEntries(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, folder)).Select(path => Path.GetFileName(path)).ToArray();
#elif ANDROID
    // https://github.com/dotnet/maui/blob/main/src/Essentials/src/FileSystem/FileSystem.android.cs
    // https://stackoverflow.com/questions/6275765/android-how-to-detect-a-directory-in-the-assets-folder
        MauiApplication.Context.Assets.List(folder);
#else
    null;
#endif

    public static async Task<string> GetAssetContentAsync(string path)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(path);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public static Task<Stream> GetAssetStreamAsync(string path)
    {
        return FileSystem.OpenAppPackageFileAsync(path);
    }
}