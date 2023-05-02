namespace YeetMacro2.Data.Serialization;
public class SizePropertiesResolver : OnlyIncludePropertiesResolver<Size>
{
    public static SizePropertiesResolver Instance = new SizePropertiesResolver();
    public SizePropertiesResolver() : base(new string[] { "width", "height" })
	{

	}
}