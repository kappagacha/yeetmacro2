namespace YeetMacro2.Data.Serialization;
public class SizePropertiesResolver : OnlyIncludePropertiesResolver<Size>
{
    public static readonly SizePropertiesResolver Instance = new();
    public SizePropertiesResolver() : base(["width", "height"])
	{

	}
}