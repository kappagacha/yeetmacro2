using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using System.IO;

namespace YeetMacro2.Platforms.Android.ViewModels;
public partial class ImagePreviewViewModel : ObservableObject
{
    byte[] _imageData;
    public byte[] ImageData
    {
        get => _imageData;
        set
        {
            _imageData = value;
            OnPropertyChanged(nameof(ImageSource));
        }
    }

    [ObservableProperty]
    string _imageInfo;

    public ImageSource ImageSource => ImageData != null ? ImageSource.FromStream(() => new MemoryStream(ImageData)) : null;
}
