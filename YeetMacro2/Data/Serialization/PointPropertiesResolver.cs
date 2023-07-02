namespace YeetMacro2.Data.Serialization;
public class PointPropertiesResolver : OnlyIncludePropertiesResolver<Size>
{
    public static PointPropertiesResolver Instance = new PointPropertiesResolver();
    public PointPropertiesResolver() : base(new string[] { "x", "y" })
	{

	}
}