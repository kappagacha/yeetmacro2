using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public abstract class Node
{
    [JsonIgnore]
    public virtual bool IsSelected { get; set; }
    [JsonIgnore]
    public virtual bool IsExpanded { get; set; } = true;
    public virtual bool IsParentNode { get => this is IParentNode; }
    public virtual string Name { get; set; }
    [JsonIgnore]
    public int NodeId { get; set; }
    [JsonIgnore]
    public int? ParentId { get; set; }
    [JsonIgnore]
    public int RootId { get; set; }
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
    ICollection<TChild> Nodes { get; set; }
}

public static class NodeMetadataHelper
{
    public static INodeMetadataProvider<T> GetMetadataProvider<T>()
    {
        var modelType = typeof(T);
        NodeMetadataAttribute metadataAttribute = null;

        while (modelType != typeof(Object))
        {
            metadataAttribute = (NodeMetadataAttribute)modelType.GetCustomAttribute(typeof(NodeMetadataAttribute));
            if (metadataAttribute is not null)
            {
                var metadataProviderType = metadataAttribute.NodeMetadataProvider;
                return Activator.CreateInstance(metadataProviderType) as INodeMetadataProvider<T>;
            }
            modelType = modelType.BaseType;
        }

        return null;
    }

    public static Type[] GetNodeTypes<T>()
    {
        var metadataProvider = GetMetadataProvider<T>();
        return metadataProvider?.NodeTypes;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeMetadataAttribute : Attribute
{
    public Type NodeMetadataProvider { get; set; }
}

// https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/
// https://code-maze.com/csharp-generic-attributes/
public interface INodeMetadataProvider<T>
{
    Expression<Func<T, object>> CollectionPropertiesExpression { get; }
    Expression<Func<T, object>> ProxyPropertiesExpression { get; }
    Type[] NodeTypes { get; }
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
    private readonly JsonConverter<TChild> _childConverter;
    private readonly JsonConverter<TParent> _parentConverter;
    private readonly Type _parentType;
    private readonly Type _childType;
    private readonly JsonSerializerOptions _options;
    public NodeValueConverter(JsonSerializerOptions options)
    {
        _childConverter = (JsonConverter<TChild>)(options).GetConverter(typeof(TChild));
        _parentConverter = (JsonConverter<TParent>)(options).GetConverter(typeof(TParent));
        _parentType = typeof(TParent);
        _childType = typeof(TChild);
        _options = options;
    }

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

            // each property named "properties" is the properties of the node
            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "properties")
            {
                reader.Read();
                if (isParent)
                {
                    parent = _parentConverter.Read(ref reader, _parentType, _options);
                    node = parent;
                }
                else
                {
                    node = _childConverter.Read(ref reader, _childType, _options);
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
            writer.WritePropertyName("properties");
            _parentConverter.Write(writer, parent, _options);
            
            foreach (var subChild in parent.Nodes)
            {
                Write(writer, subChild, _options);
            }
        }
        else
        {
            writer.WritePropertyName("$isParent");
            writer.WriteBooleanValue(false);
            writer.WritePropertyName("properties");
            _childConverter.Write(writer, child, _options);
        }

        writer.WriteEndObject();
    }
}