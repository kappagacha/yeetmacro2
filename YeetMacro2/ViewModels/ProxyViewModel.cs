using AutoMapper;
using Castle.DynamicProxy;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;
using Color = Microsoft.Maui.Graphics.Color;

namespace YeetMacro2.ViewModels;
public interface IProxyNotifyPropertyChanged : INotifyPropertyChanged
{
    void OnPropertyChanged(string propertyChanged);
    //Boolean IsLeaf { get; }
    //Color Color { get; set; }
    //Color BorderColor { get; set; }
}

//https://kozmic.net/2009/08/12/castle-dynamic-proxy-tutorial-part-xiii-mix-in-this-mix/
//Use ProxyViewModel to add INotifyPropertyChanged to target model
//Model Properties needs to be virtual for this to work
public class ProxyViewModel : IProxyNotifyPropertyChanged
{
    static IMapper _mapper;
    static MethodInfo _createCollectionMethodInfo = typeof(ProxyViewModel).GetMethod(nameof(ProxyViewModel.CreateCollection));
    //https://github.com/xamarin/XamarinCommunityToolkit/issues/420
    //public Color Color { get; set; } = Colors.Transparent;
    //public Color BorderColor { get; set; } = Colors.Transparent;
    //public Boolean IsLeaf { get; } = false;

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
        }
    }

    public static T Create<T>(T target) where T : class
    {
        if (target is IProxyNotifyPropertyChanged) return target;
        var options = new ProxyGenerationOptions();
        options.AddMixinInstance(new ProxyViewModel());
        options.AddMixinInstance(target);
        var proxyGenerator = new ProxyGenerator();
        var proxy = proxyGenerator.CreateClassProxy<T>(
            options,
            new ProxyViewModelInterceptor());

        _mapper.Map(target, proxy);
        return proxy;
    }

    public static ICollection<T> CreateCollection<T>(IEnumerable<T> target, Expression<Func<T, object>> collectionPropertiesExpression = null, Expression<Func<T, object>> proxyPropertiesExpression = null) where T : class
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

            if (collectionPropertiesExpression != null)
            {
                List<PropertyInfo> includeProperties = ReflectionHelper.GetMultiPropertyInfo<T, object>(collectionPropertiesExpression);
                foreach (var includeProperty in includeProperties)
                {
                    var childCollection = includeProperty.GetValue(childProxy);
                    var genericArguments = childCollection.GetType().GetGenericArguments();
                    object proxyCollection = null;
                    if (genericArguments[0] == typeof(T))
                    {
                        proxyCollection = CreateCollection((IEnumerable<T>)childCollection, collectionPropertiesExpression, proxyPropertiesExpression);
                    }
                    else
                    {
                        proxyCollection = _createCollectionMethodInfo.MakeGenericMethod(genericArguments[0]).Invoke(null, new[] { childCollection, null, null });
                    }

                    includeProperty.SetValue(childProxy, proxyCollection);
                }
            }

            if (proxyPropertiesExpression != null)
            {
                List<PropertyInfo> proxyProperties = ReflectionHelper.GetMultiPropertyInfo<T, object>(proxyPropertiesExpression);
                foreach (var proxyProperty in proxyProperties)
                {
                    var proxyValue = proxyProperty.GetValue(childProxy);
                    proxyProperty.SetValue(childProxy, Create(proxyValue));
                }
            }
        }

        return proxy;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}