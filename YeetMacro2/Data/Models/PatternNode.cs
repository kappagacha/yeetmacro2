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

public static class PatternHelper
{
    public static Point TopLeft { get; set; }
    public static Size CurrentResolution { get; set; }
    public static Size ScreenResolution { get; set; }
}

public class Pattern: ISortable
{
    public virtual Rect RawBounds { get; set; }
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
    public virtual BoundsCalcType BoundsCalcType { get; set; } = BoundsCalcType.Default;
    [JsonIgnore]
    public string RawBoundsDisplay => $"X={RawBounds.X:0.####} Y={RawBounds.Y:0.####} W={RawBounds.Width:0.####} H={RawBounds.Height:0.####}";
    public virtual int Position { get; set; }
    [JsonIgnore]
    public Point Offset
    {
        get
        {
            var xOffset = 0;
            var yOffset = 0;
            var topLeft = PatternHelper.TopLeft;
            var currentResolution = PatternHelper.CurrentResolution;

            switch (OffsetCalcType)
            {
                case OffsetCalcType.DockLeft:
                    return PatternHelper.TopLeft;
                case OffsetCalcType.Default:
                case OffsetCalcType.Center:
                    {   // horizontal center handling
                        var deltaX = currentResolution.Width - Resolution.Width + (topLeft.X * 2);
                        xOffset = (int)(deltaX / 2);
                    }
                    break;
                case OffsetCalcType.DockRight:
                    {   // horizontal dock right handling (dock left does not need handling)
                        var right = Resolution.Width - RawBounds.X;
                        var targetX = currentResolution.Width - right + topLeft.X;
                        xOffset = (int)(targetX - RawBounds.X);
                    }
                    break;
                case OffsetCalcType.HorizontalStretchOffset:
                    {
                        // HorizontalStretchMultiplier = targetXOffset / deltaX
                        var deltaX = currentResolution.Width - Resolution.Width;
                        xOffset = (int)(deltaX * HorizontalStretchMultiplier) + (int)topLeft.X;
                    }
                    break;
                case OffsetCalcType.VerticalStretchOffset:
                    {
                        // HorizontalStretchMultiplier = targetYOffset / deltaY
                        var deltaY = currentResolution.Height - Resolution.Height;
                        yOffset = (int)(deltaY * VerticalStretchMultiplier) + (int)topLeft.Y;
                    }
                    break;
            }
            return new Point(xOffset, yOffset);
        }
    }
    [JsonIgnore]
    public Rect Bounds
    {
        get
        {
            switch (BoundsCalcType)
            {
                case BoundsCalcType.Default:
                case BoundsCalcType.None:
                default:
                    return RawBounds;
                case BoundsCalcType.FillWidth:
                    switch (OffsetCalcType)
                    {
                        case OffsetCalcType.None:
                        case OffsetCalcType.DockLeft:
                            return new Rect(RawBounds.Location, new Size(RawBounds.Width + PatternHelper.CurrentResolution.Width - Resolution.Width, RawBounds.Width));
                        case OffsetCalcType.DockRight:
                            return new Rect(RawBounds.Location.Offset(Resolution.Width - PatternHelper.CurrentResolution.Width, 0), new Size(RawBounds.Width + PatternHelper.CurrentResolution.Width - Resolution.Width, RawBounds.Width));
                        default:
                            return new Rect(RawBounds.Location.Offset((Resolution.Width - PatternHelper.CurrentResolution.Width) / 2.0, 0), new Size(RawBounds.Width + PatternHelper.CurrentResolution.Width - Resolution.Width, RawBounds.Width));
                    }
                case BoundsCalcType.FillHeight:
                    return new Rect(RawBounds.Location, new Size(RawBounds.Width, RawBounds.Height + PatternHelper.CurrentResolution.Height - Resolution.Height));
            }
        }
    }
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
    public virtual bool IgnoreBackground { get; set; }
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

public enum BoundsCalcType
{
    Default,
    None,
    FillWidth,
    FillHeight
}