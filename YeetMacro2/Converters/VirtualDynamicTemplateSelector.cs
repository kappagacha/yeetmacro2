using System.Collections.Concurrent;
using Microsoft.Maui.Controls.Xaml;

namespace YeetMacro2.Converters;

[AcceptEmptyServiceProvider]
public class VirtualDynamicTemplateSelector : VirtualListViewItemTemplateSelector, IMarkupExtension
//public class DynamicTemplateSelector : DataTemplateSelector, IMarkupExtension
{
    static readonly VirtualDynamicTemplateSelector _instance = new();
    static readonly ConcurrentDictionary<string, DataTemplate> _keyToDataTemplate = new();
    static readonly ConcurrentBag<Type> _processedViewType = [];
    public object Root
    {
        set
        {
            VisualElement rootObject = null;
            
            // Handle direct object, Binding, and TypedBinding
            if (value is Binding binding)
            {
                rootObject = (VisualElement)binding.Source;
            }
            else if (value is VisualElement visualElement)
            {
                rootObject = visualElement;
            }
            else if (value?.GetType().FullName?.Contains("TypedBinding") == true)
            {
                // In Release mode, bindings are optimized to TypedBinding
                // We need to get the Source property via reflection
                var sourceProperty = value.GetType().GetProperty("Source");
                if (sourceProperty != null)
                {
                    var source = sourceProperty.GetValue(value);
                    rootObject = source as VisualElement;
                    if (rootObject == null)
                    {
                        throw new InvalidOperationException($"VirtualDynamicTemplateSelector: TypedBinding Source is not a VisualElement. Source type: {source?.GetType()?.FullName ?? "null"}");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"VirtualDynamicTemplateSelector: Could not find Source property on TypedBinding");
                }
            }
            else
            {
                throw new InvalidOperationException($"VirtualDynamicTemplateSelector: Unable to process rootObject of type {value?.GetType()?.FullName ?? "null"}. Expected VisualElement, Binding, or TypedBinding to VisualElement.");
            }
            
            if (rootObject == null)
            {
                throw new InvalidOperationException($"VirtualDynamicTemplateSelector: rootObject is null after extraction from {value?.GetType()?.FullName ?? "null"}");
            }
            
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
    //protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    //{
    //    string typeKey = item.GetType().Name.Replace("Proxy", "").Replace("ViewModel", "") + "Template";
    //    if (!_keyToDataTemplate.ContainsKey(typeKey)) throw new Exception($"DynamicTemplateSelector: template {typeKey} not found.");

    //    var dataTemplate = _keyToDataTemplate[typeKey];
    //    return dataTemplate;
    //}

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

    public override DataTemplate SelectTemplate(object item, int sectionIndex, int itemIndex)
    {
        if (item == null) return null;
        
        string typeKey = item.GetType().Name.Replace("ViewModel", "") + "Template";
        
        if (!_keyToDataTemplate.ContainsKey(typeKey)) 
        {
            return null; // Return null instead of throwing to see if it falls back to default
        }

        var dataTemplate = _keyToDataTemplate[typeKey];
        return dataTemplate;
    }
}