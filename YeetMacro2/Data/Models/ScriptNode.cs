using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;
public class ScriptNodeMetadataProvider : INodeMetadataProvider<ScriptNode>
{
    public Expression<Func<ScriptNode, object>> CollectionPropertiesExpression => pn => new { pn.Nodes };
    public Expression<Func<ScriptNode, object>> ProxyPropertiesExpression => null;
    public Type[] NodeTypes => null;
}

[NodeMetadata(NodeMetadataProvider = typeof(ScriptNodeMetadataProvider))]
public class ScriptNode : Node, IParentNode<ScriptNode, ScriptNode>
{
    public override bool IsParentNode => false;     // prevents tree heirarchy in the UI
    [JsonIgnore]
    public virtual ICollection<ScriptNode> Nodes { get; set; } = new List<ScriptNode>();
    public virtual string Text { get; set; }
}