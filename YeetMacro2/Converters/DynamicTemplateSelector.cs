
using System.Collections.Concurrent;

namespace YeetMacro2.Converters;

public class DynamicTemplateSelector : DataTemplateSelector, IMarkupExtension
{
    static DynamicTemplateSelector _instance = new DynamicTemplateSelector();
    static ConcurrentDictionary<string, DataTemplate> _keyToDataTemplate = new ConcurrentDictionary<string, DataTemplate>();
    static ConcurrentBag<Type> _processedViewType = new ConcurrentBag<Type>();
    public object Root
    {
        set
        {
            var rootObject = (VisualElement)((Binding)value).Source;
            if (!_processedViewType.Contains(rootObject.GetType()))
            {
                foreach (var resource in rootObject.Resources)
                {
                    if (resource.Value is DataTemplate dataTemplate)
                    {
                        _keyToDataTemplate.TryAdd(resource.Key, dataTemplate);
                    }
                }

                _processedViewType.Add(rootObject.GetType());
            }
        }
    }
    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        string typeKey = item.GetType().Name.Replace("Proxy", "").Replace("ViewModel", "") + "Template";
        if (!_keyToDataTemplate.ContainsKey(typeKey)) throw new Exception($"DynamicTemplateSelector: template {typeKey} not found.");

        var dataTemplate = _keyToDataTemplate[typeKey];
        return dataTemplate;
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        // https://github.com/dotnet/maui/issues/16881
        //var rootObjectProvider = serviceProvider.GetService<IRootObjectProvider>();
        //var rootObject = (VisualElement)rootObjectProvider.RootObject;
        //if (!_processedViewType.Contains(rootObject.GetType())) 
        //{
        //    foreach (var resource in rootObject.Resources)
        //    {
        //        if (resource.Value is DataTemplate dataTemplate)
        //        {
        //            _keyToDataTemplate.TryAdd(resource.Key, dataTemplate);
        //        }
        //    }

        //    _processedViewType.Add(rootObject.GetType());
        //}

        return _instance;
    }
}