namespace YeetMacro2.Data.Models;
public abstract class Node
{
    public virtual bool IsSelected { get; set; }
    public virtual bool IsExpanded { get; set; } = true;
    public virtual string Name { get; set; }
    public int NodeId { get; set; }
    public int? ParentId { get; set; }
}

public class LeafNode : Node
{
}

public interface IParentNode
{

}

public interface IParentNode<TParent, TChild> : IParentNode
    where TParent : Node, TChild
    where TChild : Node
{
    ICollection<TChild> Children { get; set; }
}

public class ParentNode : Node, IParentNode<ParentNode, Node>
{
    public virtual ICollection<Node> Children { get; set; } = new List<Node>();
}

public class NodeClosure
{
    public int ClosureId { get; set; }
    public string Name { get; set; }
    public Node Ancestor { get; set; }
    public int AncestorId { get; set; }
    public string AncestorName { get; set; }
    public Node Descendant { get; set; }
    public int DescendantId { get; set; }
    public string DescendantName { get; set; }
    public int Depth { get; set; }
}