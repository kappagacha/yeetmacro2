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
}

public class PatternMetadataProvider : INodeMetadataProvider<Pattern>
{
    public Expression<Func<Pattern, object>> CollectionPropertiesExpression => null;
    public Expression<Func<Pattern, object>> ProxyPropertiesExpression => p => new { p.TextMatch, p.ColorThreshold };
    public Type[] NodeTypes => null;
}

[NodeMetadata(NodeMetadataProvider = typeof(PatternMetadataProvider))]
public class Pattern
{
    public virtual Rect Rect { get; set; }
    public virtual Size Resolution { get; set; }
    [JsonIgnore]
    public virtual bool IsSelected { get; set; }
    public virtual bool IsBoundsPattern { get; set; }
    public virtual bool IsNotCachingOffset { get; set; }
    public virtual bool IsNotUsingCalcResolution { get; set; }
    [JsonIgnore]
    public virtual int PatternId { get; set; }
    [JsonIgnore]
    public virtual int PatternNodeId { get; set; }
    public virtual string Name { get; set; }
    public byte[] ImageData { get; set; }
    public virtual double VariancePct { get; set; } = 20.0;
    public virtual TextMatchProperties TextMatch { get; set; }
    public virtual ColorThresholdProperties ColorThreshold { get; set; }
    public virtual OffsetCalcType OffsetCalcType { get; set; } = OffsetCalcType.Default;
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

public enum OffsetCalcType
{
    Default,
    None,
    Center,
    DockLeft,
    DockRight
}