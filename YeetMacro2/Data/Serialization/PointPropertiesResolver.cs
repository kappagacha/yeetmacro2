namespace YeetMacro2.Data.Serialization;
public class PointPropertiesResolver : OnlyIncludePropertiesResolver<Point>
{
    public static readonly PointPropertiesResolver Instance = new();
    public PointPropertiesResolver() : base(["x", "y"])
	{

	}
}