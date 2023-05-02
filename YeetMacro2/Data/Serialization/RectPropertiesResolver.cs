namespace YeetMacro2.Data.Serialization;
public class RectPropertiesResolver : OnlyIncludePropertiesResolver<Rect>
{
    public static RectPropertiesResolver Instance = new RectPropertiesResolver();
    public RectPropertiesResolver() : base(new string[] { "x", "y", "width", "height" })
    {

    }
}