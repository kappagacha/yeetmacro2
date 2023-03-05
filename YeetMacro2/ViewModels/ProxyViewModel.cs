using AutoMapper;
using Castle.DynamicProxy;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;
public interface IProxyNotifyPropertyChanged : INotifyPropertyChanged
{
    void OnPropertyChanged(string propertyChanged);
    object IsLeaf { get; set; }     // using this to not get the warnings from UraniumUI treeview
    object Children { get; set; }   // using this to not get the warnings from UraniumUI treeview
}

//https://kozmic.net/2009/08/12/castle-dynamic-proxy-tutorial-part-xiii-mix-in-this-mix/
//Use ProxyViewModel to add INotifyPropertyChanged to target model
//Model Properties needs to be virtual for this to work
public class ProxyViewModel : IProxyNotifyPropertyChanged
{
    static IMapper _mapper;
    static MethodInfo _createCollectionMethodInfo = typeof(ProxyViewModel).GetMethod(nameof(ProxyViewModel.CreateCollection));
    public object IsLeaf { get; set; }
    public object Children { get; set; }

    static ProxyViewModel()
    {
        _mapper = ServiceHelper.GetService<IMapper>();
    }
    class ProxyViewModelInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            if (invocation.Method.Name.StartsWith("set_"))
            {
                var notifier = (IProxyNotifyPropertyChanged)invocation.Proxy;
                notifier.OnPropertyChanged(invocation.Method.Name.Substring(4));
            }

            if (invocation.Method.Name.StartsWith("get_Children"))
            {
                invocation.ReturnValue = ReflectionHelper.PropertyInfoCollection[invocation.Proxy.GetType()]["Nodes"]?.GetValue(invocation.Proxy);
            }
        }
    }

    public static T Create<T>(T target) where T : class
    {
        if (target is IProxyNotifyPropertyChanged) return target;
        var options = new ProxyGenerationOptions();
        options.AddMixinInstance(new ProxyViewModel());
        options.AddMixinInstance(target);
        var proxyGenerator = new ProxyGenerator();
        var proxy = (T)proxyGenerator.CreateClassProxy(
            target.GetType(),
            options,
            new ProxyViewModelInterceptor());

        _mapper.Map(target, proxy);

        var expressionsProvider = NodeMetadataHelper.GetMetadataProvider<T>();
        if (expressionsProvider?.CollectionPropertiesExpression is not null || expressionsProvider?.ProxyPropertiesExpression is not null)
        {
            ResolveProxyGraph(proxy, expressionsProvider.CollectionPropertiesExpression, expressionsProvider.ProxyPropertiesExpression);
        }

        return proxy;
    }

    private static void ResolveProxyGraph<T>(T proxy, Expression<Func<T, object>> collectionPropertiesExpression, Expression<Func<T, object>> proxyPropertiesExpression) where T : class
    {
        if (collectionPropertiesExpression != null)
        {
            List<PropertyInfo> collectionProperties = ReflectionHelper.GetMultiPropertyInfo<T, object>(collectionPropertiesExpression, proxy.GetType());
            foreach (var collectionProperty in collectionProperties)
            {
                var childCollection = collectionProperty.GetValue(proxy);
                var genericArguments = childCollection.GetType().GetGenericArguments();
                object proxyCollection = null;
                if (genericArguments[0] == typeof(T))
                {
                    proxyCollection = CreateCollection((IEnumerable<T>)childCollection);
                }
                else
                {
                    proxyCollection = _createCollectionMethodInfo.MakeGenericMethod(genericArguments[0]).Invoke(null, new[] { childCollection });
                }

                collectionProperty.SetValue(proxy, proxyCollection);
            }
        }

        if (proxyPropertiesExpression != null)
        {
            List<PropertyInfo> proxyProperties = ReflectionHelper.GetMultiPropertyInfo<T, object>(proxyPropertiesExpression);
            foreach (var proxyProperty in proxyProperties)
            {
                var proxyValue = proxyProperty.GetValue(proxy);
                proxyProperty.SetValue(proxy, Create(proxyValue));
            }
        }
    }

    public static ICollection<T> CreateCollection<T>(IEnumerable<T> target) where T : class
    {
        //https://github.com/castleproject/Core/blob/master/src/Castle.Core/DynamicProxy/ProxyGenerationOptions.cs
        var options = new ProxyGenerationOptions();
        options.AddMixinInstance(new ObservableCollection<T>());
        options.BaseTypeForInterfaceProxy = typeof(ObservableCollection<T>);
        var proxyGenerator = new ProxyGenerator();
        var proxy = proxyGenerator.CreateClassProxy<ObservableCollection<T>>(
            options,
            new ProxyViewModelInterceptor());

        var collection = (ICollection<T>)proxy;
        var list = target.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            var childProxy = Create(item);
            collection.Add(childProxy);
        }

        return proxy;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}