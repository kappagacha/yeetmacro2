namespace YeetMacro2.Data.Serialization;
public class RectPropertiesResolver : OnlyIncludePropertiesResolver<Rect>
{
    public static readonly RectPropertiesResolver Instance = new();
    public RectPropertiesResolver() : base(["x", "y", "width", "height"])
    {

    }
}