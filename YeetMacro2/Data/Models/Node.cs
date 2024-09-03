using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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

public class NodeValueConverter<TParent, TChild>(JsonSerializerOptions options) : JsonConverter<TChild>
    where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
    where TChild : Node
{
    private readonly JsonConverter<TChild> _childConverter = (JsonConverter<TChild>)(options).GetConverter(typeof(TChild));
    private readonly Type _parentType = typeof(TParent);
    private readonly Type _childType = typeof(TChild);
    private readonly JsonSerializerOptions _options = options;

    // incoming options is ignored, we are using the options passed in through the constructor
    public override TChild Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
    {
        var isParent = false;
        TChild node = null;
        TParent parent = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return node;
            }

            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "$isParent")
            {
                reader.Read();
                isParent = reader.GetBoolean();
            }

            // each property named "props" is the properties of the node
            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "props")
            {
                reader.Read();
                node = _childConverter.Read(ref reader, _childType, _options);

                if (isParent)
                {
                    parent = (TParent)node;
                }
            }
            // additional properties are nodes
            else if (reader.TokenType == JsonTokenType.PropertyName)
            {
                parent.Nodes.Add(Read(ref reader, typeToConvert, _options));
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
        writer.WriteStartObject();
        if (child is TParent parent)
        {
            writer.WritePropertyName("$isParent");
            writer.WriteBooleanValue(true);
            writer.WritePropertyName("props");
            _childConverter.Write(writer, parent, _options);
            
            foreach (var subChild in parent.Nodes)
            {
                Write(writer, subChild, _options);
            }
        }
        else
        {
            writer.WritePropertyName("$isParent");
            writer.WriteBooleanValue(false);
            writer.WritePropertyName("props");
            _childConverter.Write(writer, child, _options);
        }

        writer.WriteEndObject();
    }
}