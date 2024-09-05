using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using YeetMacro2.Data.Services;

namespace YeetMacro2.Data.Models;

public interface ISortable
{
    int Position { get; set; }
}

public abstract class Node: ISortable
{
    [JsonIgnore]
    public virtual bool IsSelected { get; set; }
    [JsonIgnore]
    public virtual bool IsExpanded { get; set; } = false;
    public virtual bool IsParentNode { get => this is IParentNode; }
    public virtual string Name { get; set; }
    public virtual int Position { get; set; }
    [JsonIgnore]
    public int NodeId { get; set; }
    [JsonIgnore]
    public int? ParentId { get; set; }
    [JsonIgnore]
    public int RootId { get; set; }
    public string Path { get; set; }
}

public interface IParentNode
{
    string Name { get; set; }
    bool IsSelected { get; set; }
}

public interface IParentNode<TParent, TChild> : IParentNode
    where TParent : Node, TChild
    where TChild : Node
{
    IList<TChild> Nodes { get; set; }
}

// https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/
// https://code-maze.com/csharp-generic-attributes/
[AttributeUsage(AttributeTargets.Class)]
public class NodeTypesAttribute(params Type[] types) : Attribute
{
    public static Type[] GetTypes<T>()
    {
        var nodeTypeAttribute = (NodeTypesAttribute)typeof(T).GetCustomAttribute(typeof(NodeTypesAttribute));
        return nodeTypeAttribute?.Types;
    }

    public Type[] Types { get; } = types;
}

public class NodeTypeMapping(Type key, Type value)
{
    public Type Key { get; } = key;
    public Type Value { get; } = value;

    public static NodeTypeMapping Create(Type key, Type value)
    {
        return new NodeTypeMapping(key, value);
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NodeTypeMappingAttribute(Type key, Type value) : Attribute
{
    static readonly Dictionary<Type, Dictionary<Type, Type>> _typeToMappings = [];
    public Type Key { get; } = key;
    public Type Value { get; } = value;

    public static Dictionary<Type, Type> GetMappedType<T>()
    {
        if (!_typeToMappings.ContainsKey(typeof(T)))
        {
            var nodeTypeMappingAttribute = (IEnumerable<NodeTypeMappingAttribute>)typeof(T).GetCustomAttributes(typeof(NodeTypeMappingAttribute));
            _typeToMappings.Add(typeof(T), new Dictionary<Type, Type>(nodeTypeMappingAttribute.Select(att => KeyValuePair.Create(att.Key, att.Value))));
        }
       
        return _typeToMappings[typeof(T)];
    }
}

public class NodeClosure
{
    public int ClosureId { get; set; }
    public int NodeRootId { get; set; }
    public string Name { get; set; }
    public Node Ancestor { get; set; }
    public int AncestorId { get; set; }
    public string AncestorName { get; set; }
    public Node Descendant { get; set; }
    public int DescendantId { get; set; }
    public string DescendantName { get; set; }
    public int Depth { get; set; }
}

public class NodeValueConverter<TParent, TChild> : JsonConverter<TChild>
    where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
    where TChild : Node
{
    //private readonly JsonConverter<TChild> _childConverter = (JsonConverter<TChild>)(options).GetConverter(typeof(TChild));
    private readonly Type _parentType = typeof(TParent);
    private readonly Type _childType = typeof(TChild);
    private readonly JsonSerializerOptions _options;


    public NodeValueConverter(JsonSerializerOptions options)
    {
        _options = options;
    }

    // incoming options is ignored, we are using the options passed in through the constructor
    public override TChild Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
    {
        return (TChild)ReadNodeObject(ref reader, typeToConvert, options);
    }

    public object ReadNodeObject(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        object node = null;
        Type type = null;

        var jsonDerivedTypeAttributes = typeToConvert.GetCustomAttributes<JsonDerivedTypeAttribute>();
        var discriminatorToType = jsonDerivedTypeAttributes.ToDictionary(jdta => jdta.TypeDiscriminator.ToString(), jdta => jdta.DerivedType);

        if (discriminatorToType.Count == 0)    // not polymorphic
        {
            node = Activator.CreateInstance(typeToConvert);
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return node;
            }

            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "$type") // polymorphic
            {
                reader.Read();
                var discriminator = reader.GetString();
                type = discriminatorToType[discriminator];
                reader.Read();

                node = (TChild)Activator.CreateInstance(type);
                ReflectionHelper.PropertyInfoCollection[type].Load();
            }

            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString().StartsWith("$"))
            {
                var propJson = reader.GetString();
                var prop = $"{propJson[1].ToString().ToUpper()}{propJson.Substring(2)}";
                var propInfo = ReflectionHelper.PropertyInfoCollection[node.GetType()][prop];
                if (propInfo.CanWrite)
                {
                    reader.Read();

                    if (propInfo.PropertyType.IsAssignableTo(typeof(Node)))
                    {
                        propInfo.SetValue(node, ReadNodeObject(ref reader, propInfo.PropertyType, _options));
                    }
                    else
                    {
                        propInfo.SetValue(node, JsonSerializer.Deserialize(ref reader, propInfo.PropertyType, _options));
                    }
                }
            }
            else if (reader.TokenType == JsonTokenType.PropertyName && node is TParent parent)
            {
                reader.Read();
                parent.Nodes.Add((TChild)ReadNodeObject(ref reader, typeToConvert, _options));
            }
        }

        return node;
    }

    // incoming options is ignored, we are using the options passed in through the constructor
    public override void Write(
            Utf8JsonWriter writer,
            TChild child,
            JsonSerializerOptions options)
    {
        writer.WritePropertyName(child.Name);
        WriteNodeObject(writer, child);
    }

    public void WriteNodeObject(Utf8JsonWriter writer, Object obj)
    {
        writer.WriteStartObject();

        ReflectionHelper.PropertyInfoCollection[obj.GetType()].Load();
        var properties = ReflectionHelper.PropertyInfoCollection[obj.GetType()];
        var jsonDerivedTypeAttributes = typeof(TChild).GetCustomAttributes<JsonDerivedTypeAttribute>();
        var descriminator = jsonDerivedTypeAttributes.FirstOrDefault(jdta => jdta.DerivedType == obj.GetType() || jdta.DerivedType == obj.GetType().BaseType)?.TypeDiscriminator?.ToString();
        if (!String.IsNullOrEmpty(descriminator))
        {
            writer.WritePropertyName("$type");
            writer.WriteStringValue(descriminator);
        }
        var ignoreProperties = new List<string>() { nameof(IParentNode<Node, Node>.Nodes), nameof(Node.IsExpanded), nameof(Node.IsSelected), nameof(Node.IsParentNode), nameof(SettingNode.SettingType), "Item" };

        foreach (var property in properties)
        {
            if (ignoreProperties.Contains(property.Name)) continue;

            bool jsonIgnore = property.IsDefined(typeof(JsonIgnoreAttribute), false);
            if (jsonIgnore) continue;

            writer.WritePropertyName($"${property.Name[0].ToString().ToLower()}{property.Name.Substring(1)}");

            WriteValue(writer, property.PropertyType, property.GetValue(obj));
        }

        if (obj is TParent parent)
        {
            foreach (var subChild in parent.Nodes)
            {
                Write(writer, subChild, _options);
            }
        }

        writer.WriteEndObject();
    }

    public void WriteValue(Utf8JsonWriter writer, Type type, Object val)
    {
        if (type.IsAssignableTo(typeof(Node)))
        {
            WriteNodeObject(writer, val);
        }
        else
        {
            JsonSerializer.Serialize(writer, val, _options);
        }
    }
}
