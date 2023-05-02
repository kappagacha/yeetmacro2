using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace YeetMacro2.Data.Serialization;

public interface ICombinableTypeResolver
{
    void ResolveJsonTypeInfo(JsonTypeInfo jsonTypeInfo, Type type);
}

public class CombinedPropertiesResolver : DefaultJsonTypeInfoResolver
{
    private readonly ICombinableTypeResolver[] _resolvers;
    public static CombinedPropertiesResolver Combine(params ICombinableTypeResolver[] resolvers)
    {
        return new CombinedPropertiesResolver(resolvers);
    }

    public CombinedPropertiesResolver(IEnumerable<ICombinableTypeResolver> resolvers)
    {
        _resolvers = resolvers.ToArray();
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

