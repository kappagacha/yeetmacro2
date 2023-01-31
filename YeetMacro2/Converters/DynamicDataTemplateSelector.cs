using YeetMacro2.Data.Models;

namespace YeetMacro2.Converters;

//https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/templates/data-templates/selector
//https://codemilltech.com/attached-properties-what-are-they-good-for/
public class DynamicDataTemplateSelector : DataTemplateSelector, IMarkupExtension
{
    static DynamicDataTemplateSelector _instance = new DynamicDataTemplateSelector();

    public static BindableProperty ResourceSourceProperty =
    BindableProperty.Create("ResourceSource", typeof(object),
                                typeof(DynamicDataTemplateSelector), null);

    public static void SetResourceSource(BindableObject view, object resourceSource)
    {
        view.SetValue(ResourceSourceProperty, resourceSource);
    }

    public static object GetResourceSource(BindableObject view)
    {
        return view.GetValue(ResourceSourceProperty);
    }

    public object ResourceSource { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var source = (VisualElement)GetResourceSource(container);
        if (source == null) return null;

        if (source.Resources.TryGetValue(item.GetType().Name.Replace("Proxy", "") + "Template", out var typeTemplate))
        {
            return (DataTemplate)typeTemplate;
        }

        if (item is IParentNode && source.Resources.TryGetValue("DefaultParentNodeTemplate", out var defaultParentTemplate))
        {
            return (DataTemplate)defaultParentTemplate;
        }

        if (item is Node && source.Resources.TryGetValue("DefaultNodeTemplate", out var defaultNodeTemplate))
        {
            return (DataTemplate)defaultNodeTemplate;
        }

        return null;
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return _instance;
    }
}