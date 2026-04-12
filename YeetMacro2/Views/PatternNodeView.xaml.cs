using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Devices;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Views;

public partial class PatternNodeView : ContentView
{
    public static readonly BindableProperty MacroSetProperty =
            BindableProperty.Create(nameof(MacroSet), typeof(MacroSetViewModel), typeof(PatternNodeView), null);
    public static readonly BindableProperty DisplayOrientationProperty =
            BindableProperty.Create(nameof(DisplayOrientation), typeof(string), typeof(PatternNodeView), "Landscape");

    public MacroSetViewModel MacroSet
    {
        get { return (MacroSetViewModel)GetValue(MacroSetProperty); }
        set { SetValue(MacroSetProperty, value); }
    }

    public string DisplayOrientation
    {
        get { return (string)GetValue(DisplayOrientationProperty); }
        set { SetValue(DisplayOrientationProperty, value); }
    }

    public PatternNodeView()
	{
			InitializeComponent();
        DisplayOrientation = DeviceDisplay.MainDisplayInfo.Orientation.ToString();
        WeakReferenceMessenger.Default.Register<DisplayInfoChangedEventArgs>(this, (r, e) =>
        {
            DisplayOrientation = e.DisplayInfo.Orientation.ToString();
        });
	}
}
