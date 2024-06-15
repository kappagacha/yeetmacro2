
namespace YeetMacro2.Views;

public class DataTemplateView : ContentView
{
    public static readonly BindableProperty DataTemplateProperty =
        BindableProperty.Create("DataTemplate", typeof(DataTemplate), typeof(DataTemplateView), null, propertyChanged: DataTemplate_Changed);

    public DataTemplate DataTemplate
    {
        get { return (DataTemplate)GetValue(DataTemplateProperty); }
        set { SetValue(DataTemplateProperty, value); }
    }

    private static void DataTemplate_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is DataTemplate dataTemplate)
        {
            var dataTemplateView = bindable as DataTemplateView;
            dataTemplateView.Content = (View)dataTemplate.CreateContent();
        }
    }

    public DataTemplateView()
	{

	}
}