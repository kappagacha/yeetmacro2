using System.Dynamic;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public class PatternNodeMetadataProvider : INodeMetadataProvider<PatternNode>
{
    public Expression<Func<PatternNode, object>> CollectionPropertiesExpression => pn => new { pn.Nodes, pn.Patterns };
    public Expression<Func<PatternNode, object>> ProxyPropertiesExpression => null;
    public Type[] NodeTypes => null;
}

[NodeMetadata(NodeMetadataProvider = typeof(PatternNodeMetadataProvider))]
public class PatternNode : Node, IParentNode<PatternNode, PatternNode>
{
    public virtual bool IsMultiPattern { get; set; }
    [JsonIgnore]
    public virtual ICollection<PatternNode> Nodes { get; set; } = new List<PatternNode>();
    public virtual ICollection<Pattern> Patterns { get; set; } = new List<Pattern>();
    public dynamic BuildDynamicObject(string path = "")
    {
        var result = new ExpandoObject();
        var resolvedPath = path + (String.IsNullOrEmpty(path) ? "" : ".") + Name;
        if (resolvedPath == "root") { resolvedPath = ""; }
        ((IDictionary<String, Object>)result)["patterns"] = Patterns;
        ((IDictionary<String, Object>)result)["metadata"] = this;
        ((IDictionary<String, Object>)result)["path"] = resolvedPath;

        foreach (var node in Nodes)
        {
            ((IDictionary<String, Object>)result)[node.Name] = node.BuildDynamicObject(resolvedPath);
        }

        return result;
    }
}
public class PatterneMetadataProvider : INodeMetadataProvider<Pattern>
{
    public Expression<Func<Pattern, object>> CollectionPropertiesExpression => null;
    public Expression<Func<Pattern, object>> ProxyPropertiesExpression => p => new { p.TextMatch, p.ColorThreshold };
    public Type[] NodeTypes => null;
}

[NodeMetadata(NodeMetadataProvider = typeof(PatterneMetadataProvider))]
public class Pattern
{
    public virtual Bounds Bounds { get; set; }
    public virtual Resolution Resolution { get; set; }
    public virtual bool IsSelected { get; set; }
    [JsonIgnore]
    public virtual int PatternId { get; set; }
    [JsonIgnore]
    public virtual int PatternNodeId { get; set; }
    public virtual string Name { get; set; }
    public byte[] ImageData { get; set; }
    public virtual double VariancePct { get; set; } = 20.0;
    public virtual TextMatchProperties TextMatch { get; set; } = new TextMatchProperties();
    public virtual ColorThresholdProperties ColorThreshold { get; set; } = new ColorThresholdProperties();
}

public class TextMatchProperties
{
    public virtual bool IsActive { get; set; }
    public virtual string Text { get; set; }
    public virtual string WhiteList { get; set; }

}
public class ColorThresholdProperties
{
    public virtual bool IsActive { get; set; }
    public virtual double VariancePct { get; set; } = 10.0;
    public virtual string Color { get; set; }
    public byte[] ImageData { get; set; }
}

public class Bounds
{
    public virtual Point Start { get; set; }
    public virtual Point End { get; set; }
    public override string ToString()
    {
        return $"({Start.X:F2}, {Start.Y:F2}),({End.X:F2}, {End.Y:F2})";
    }
}

public class Resolution
{
    public virtual double Width { get; set; }
    public virtual double Height { get; set; }
    public override string ToString()
    {
        return $"{Width:F0}x{Height:F0}";
    }
}