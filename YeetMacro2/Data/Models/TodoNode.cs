using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public class TodoNode
    : Node, IParentNode<TodoNode, TodoNode>
{
    public override bool IsParentNode => false;     // prevents tree heirarchy in the UI
    [JsonIgnore]
    public virtual IList<TodoNode> Nodes { get; set; } = [];
    public virtual DateOnly Date { get; set; }
    public virtual string Data { get; set; }
}