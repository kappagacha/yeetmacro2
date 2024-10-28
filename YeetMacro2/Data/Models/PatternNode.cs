using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public class PatternNode : Node, IParentNode<PatternNode, PatternNode>
{
    public virtual bool IsMultiPattern { get; set; }
    [JsonIgnore]
    public virtual IList<PatternNode> Nodes { get; set; } = [];
    public virtual IList<Pattern> Patterns { get; set; } = [];
    [JsonIgnore]
    public Pattern Pattern { get => Patterns.FirstOrDefault(); }
}

public class Pattern: ISortable
{
    public virtual Rect Rect { get; set; }
    public virtual Size Resolution { get; set; }
    [JsonIgnore]
    public virtual bool IsSelected { get; set; }
    public virtual bool IsBoundsPattern { get; set; }
    [JsonIgnore]
    public virtual int PatternId { get; set; }
    [JsonIgnore]
    public virtual int PatternNodeId { get; set; }
    public virtual string Name { get; set; }
    public byte[] ImageData { get; set; }
    public virtual double VariancePct { get; set; } = 20.0;
    public virtual double HorizontalStretchMultiplier { get; set; }
    public virtual double VerticalStretchMultiplier { get; set; }
    public virtual TextMatchProperties TextMatch { get; set; }
    public virtual ColorThresholdProperties ColorThreshold { get; set; }
    public virtual OffsetCalcType OffsetCalcType { get; set; } = OffsetCalcType.Default;
    [JsonIgnore]
    public string RectDisplay => $"X={Rect.X:0.####} Y={Rect.Y:0.####} W={Rect.Width:0.####} H={Rect.Height:0.####}";
    public virtual int Position { get; set; }
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
    public virtual double VariancePct { get; set; } = 30.0;
    public virtual string Color { get; set; }
    public byte[] ImageData { get; set; }
}

public enum OffsetCalcType
{
    Default,
    None,
    Center,
    DockLeft,
    DockRight,
    HorizontalStretchOffset,
    VerticalStretchOffset
}