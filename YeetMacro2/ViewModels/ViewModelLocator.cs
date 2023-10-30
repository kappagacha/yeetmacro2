using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

//https://marcduerst.com/2020/01/18/inject-xamarin-forms-view-models-via-ioc-container/
public static class ViewModelLocator
{
    public static readonly BindableProperty ViewModelTypeProperty =
        BindableProperty.CreateAttached(
            "ViewModelType",
            typeof(Type),
            typeof(ViewModelLocator),
            null,
            propertyChanged: OnViewModelChanged);

    public static Type GetViewModelType(BindableObject bindable)
        => (Type)bindable.GetValue(ViewModelTypeProperty);

    public static void SetViewModelType(BindableObject bindable, Type value)
        => bindable.SetValue(ViewModelTypeProperty, value);

    private static void OnViewModelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = bindable as Element;
        if (newValue is Type type)
        {
            var viewModel = ServiceHelper.GetService(type);
            view.BindingContext = viewModel;
        }
    }
}