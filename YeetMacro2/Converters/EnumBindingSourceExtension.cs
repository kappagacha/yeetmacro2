namespace YeetMacro2.Converters;

// https://learn.microsoft.com/en-us/answers/questions/147798/enum-converter-in-xaml
public class EnumBindingSourceExtension : IMarkupExtension
{
    private Type _enumType;
    public Type EnumType
    {
        get { return this._enumType; }
        set
        {
            if (value != this._enumType)
            {
                if (null != value)
                {
                    Type enumType = Nullable.GetUnderlyingType(value) ?? value;
                    if (!enumType.IsEnum) throw new ArgumentException("Type must be for an Enum.");
                }
                this._enumType = value;
            }
        }
    }

    public EnumBindingSourceExtension() { }

    public EnumBindingSourceExtension(Type _) => this.EnumType = _;

    public object ProvideValue(IServiceProvider _)
    {
        if (null == this._enumType) throw new InvalidOperationException("The EnumType must be specified.");
        Type actualEnumType = Nullable.GetUnderlyingType(this._enumType) ?? this._enumType;
        Array enumValues = Enum.GetValues(actualEnumType);
        if (actualEnumType == this._enumType) return enumValues;
        Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
        enumValues.CopyTo(tempArray, 1);
        return tempArray;
    }
}