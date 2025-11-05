using System.Collections.ObjectModel;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels;

namespace YeetMacro2.Views;

public partial class TagDisplayView : ContentView
{
    public static readonly BindableProperty TagsProperty = BindableProperty.Create(
        nameof(Tags),
        typeof(string[]),
        typeof(TagDisplayView),
        null,
        propertyChanged: OnTagsChanged);

    public static readonly BindableProperty MacroSetProperty = BindableProperty.Create(
        nameof(MacroSet),
        typeof(MacroSetViewModel),
        typeof(TagDisplayView),
        null);

    public string[] Tags
    {
        get => (string[])GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    public MacroSetViewModel MacroSet
    {
        get => (MacroSetViewModel)GetValue(MacroSetProperty);
        set => SetValue(MacroSetProperty, value);
    }

    private HorizontalStackLayout _stackLayout;

    public TagDisplayView()
    {
        _stackLayout = new HorizontalStackLayout
        {
            Spacing = 3,
            Margin = new Thickness(0, 2, 0, 0)
        };

        Content = _stackLayout;
    }

    private static void OnTagsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TagDisplayView view)
        {
            view.UpdateTags();
        }
    }

    private void UpdateTags()
    {
        _stackLayout.Children.Clear();

        if (Tags == null || Tags.Length == 0 || MacroSet == null)
            return;

        var macroSetTags = MacroSet.Tags;
        if (macroSetTags == null)
            return;

        // Convert tag string keys to NodeTag objects
        foreach (var tagKey in Tags)
        {
            var tag = macroSetTags.FirstOrDefault(t => $"{t.FontFamily}-{t.Glyph}" == tagKey);
            if (tag != null)
            {
                var tagBorder = new Border
                {
                    Stroke = Application.Current.Resources["Primary"] as Color ?? Colors.Blue,
                    StrokeThickness = 1,
                    Padding = new Thickness(3, 1),
                    BackgroundColor = Colors.Transparent,
                    Content = new HorizontalStackLayout
                    {
                        Spacing = 2,
                        Children =
                        {
                            new ImageView
                            {
                                FontFamily = tag.FontFamily,
                                Glyph = tag.Glyph,
                                ImageWidth = 12,
                                ImageHeight = 12
                            },
                            new Label
                            {
                                Text = tag.Name,
                                FontSize = 10,
                                VerticalOptions = LayoutOptions.Center
                            }
                        }
                    }
                };

                _stackLayout.Children.Add(tagBorder);
            }
        }
    }
}
