using System.Collections.Concurrent;
using UraniumUI.Material.Controls;

namespace YeetMacro2.Views;

public class BindingContextView : ContentView
{
    static ConcurrentDictionary<string, DataTemplate> _typeKeyToDataTemplate = new();
    protected override void OnParentSet()
    {
        base.OnParentSet();
        ResolveContent();
    }

    protected override void OnBindingContextChanged()
    {
        ResolveContent();
    }

    private void ResolveContent()
    {
        if (BindingContext == null || Parent == null) return;

        var view = (VisualElement)this;
        string typeKey = BindingContext.GetType().Name.Replace("Proxy", "") + "Template";

        if (!_typeKeyToDataTemplate.ContainsKey(typeKey))
        {
            object typeTemplate = null;
            while (view != null && typeTemplate == null)
            {
                view.Resources.TryGetValue(typeKey, out typeTemplate);

                if (view is TreeViewNodeHolderView treeViewNode)
                {
                    view = treeViewNode.TreeView;
                }
                else
                {
                    view = (VisualElement)view.Parent;
                }
            }

            if (typeTemplate != null)
            {
                _typeKeyToDataTemplate.TryAdd(typeKey, (DataTemplate)typeTemplate);
            }
            else
            {
                throw new Exception($"TypeTemplate {typeKey} not found");
            }
        }


        this.Content = (View)_typeKeyToDataTemplate[typeKey].CreateContent();
        this.Content.BindingContext = BindingContext;
    }

    public BindingContextView()
	{
	}
}