using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace YeetMacro2.Data.Serialization;

public interface ICombinableTypeResolver
{
    void ResolveJsonTypeInfo(JsonTypeInfo jsonTypeInfo, Type type);
}

public class CombinedPropertiesResolver(IEnumerable<ICombinableTypeResolver> resolvers) : DefaultJsonTypeInfoResolver
{
    private readonly ICombinableTypeResolver[] _resolvers = resolvers.ToArray();
    public static CombinedPropertiesResolver Combine(params ICombinableTypeResolver[] resolvers)
    {
        return new CombinedPropertiesResolver(resolvers);
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        foreach (var resolver in _resolvers)
        {
            resolver.ResolveJsonTypeInfo(jsonTypeInfo, type);
        }

        return jsonTypeInfo;
    }
}

