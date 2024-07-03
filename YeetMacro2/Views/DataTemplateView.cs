namespace YeetMacro2.Views;

public class DataTemplateView : ContentView
{
    public static readonly BindableProperty DataTemplateProperty =
        BindableProperty.Create(nameof(DataTemplate), typeof(DataTemplate), typeof(DataTemplateView), null, propertyChanged: DataTemplate_Changed);
    public static readonly BindableProperty ContentTemplateProperty =
        BindableProperty.Create(nameof(ContentTemplate), typeof(DataTemplate), typeof(DataTemplateView), null, propertyChanged: ContentTemplate_Changed);

    public DataTemplate DataTemplate
    {
        get { return (DataTemplate)GetValue(DataTemplateProperty); }
        set { SetValue(DataTemplateProperty, value); }
    }

    public DataTemplate ContentTemplate
    {
        get { return (DataTemplate)GetValue(ContentTemplateProperty); }
        set { SetValue(ContentTemplateProperty, value); }
    }

    private static void DataTemplate_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is DataTemplate dataTemplate)
        {
            var dataTemplateView = bindable as DataTemplateView;
            dataTemplateView.Content = (View)dataTemplate.CreateContent();

            if (dataTemplateView.ContentTemplate is null) return;
            var contentPresenter = (ContentPresenter)dataTemplateView.Content.FindByName("contentPresenter");
            if (contentPresenter is null) return;
            contentPresenter.Content = (View)dataTemplateView.ContentTemplate.CreateContent();
        }
    }

    private static void ContentTemplate_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is DataTemplate dataTemplate)
        {
            var dataTemplateView = bindable as DataTemplateView;
            var contentPresenter = (ContentPresenter)dataTemplateView.Content.FindByName("contentPresenter");
            if (contentPresenter is null) return;
            contentPresenter.Content = (View)dataTemplate.CreateContent();
            contentPresenter.Content.BindingContext = dataTemplateView.BindingContext;
        }
    }

    public DataTemplateView()
    {
        BindingContextChanged += DataTemplateView_BindingContextChanged;
    }

    private void DataTemplateView_BindingContextChanged(object sender, EventArgs e)
    {
        if (ContentTemplate is null) return;

        var contentPresenter = (ContentPresenter)Content.FindByName("contentPresenter");
        if (contentPresenter is null) return;
        contentPresenter.Content.BindingContext = BindingContext;
    }
}