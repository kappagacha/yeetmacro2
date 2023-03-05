using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

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

public abstract class Node
{
    public virtual bool IsSelected { get; set; }
    public virtual bool IsExpanded { get; set; } = true;
    public bool IsParentNode { get => this is IParentNode; }
    public virtual string Name { get; set; }
    [JsonIgnore]
    public int NodeId { get; set; }
    [JsonIgnore]
    public int? ParentId { get; set; }
    [JsonIgnore]
    public int RootId { get; set; }
}

//public class LeafNode : Node
//{
//}

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

//public class ParentNode : Node, IParentNode<ParentNode, Node>
//{
//    public virtual ICollection<Node> Children { get; set; } = new List<Node>();
//}

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