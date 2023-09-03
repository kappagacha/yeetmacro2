using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace YeetMacro2.Data.Serialization;

public class ExcludePropertiesResolver<T> : DefaultJsonTypeInfoResolver, ICombinableTypeResolver
{
    private readonly List<string> _propertyNames;

    public ExcludePropertiesResolver(IEnumerable<string> propertyNames)
    {
        _propertyNames = propertyNames.ToList();
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);
        ResolveJsonTypeInfo(jsonTypeInfo, type);
        return jsonTypeInfo;
    }

    public void ResolveJsonTypeInfo(JsonTypeInfo jsonTypeInfo, Type type)
    {
        if (type == typeof(T) && jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (JsonPropertyInfo prop in jsonTypeInfo.Properties)
            {
                if (_propertyNames.Contains(prop.Name))
                {
                    prop.ShouldSerialize = (parent, value) => false;
                }
            }
        }
    }
}