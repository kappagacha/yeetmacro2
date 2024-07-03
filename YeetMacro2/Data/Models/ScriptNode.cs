using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public class ScriptNode : Node, IParentNode<ScriptNode, ScriptNode>
{
    public override bool IsParentNode => false;     // prevents tree heirarchy in the UI
    public virtual bool IsHidden { get; set; }
    public virtual bool IsFavorite { get; set; }
    [JsonIgnore]
    public virtual IList<ScriptNode> Nodes { get; set; } = [];
    public virtual string Text { get; set; }
    [JsonIgnore]
    public virtual string Description { get; set; }
}