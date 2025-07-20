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

public static class DisplayHelper
{
    private static readonly Dictionary<DisplayRotation, Rect> _rotationToUsableBounds = new ();
    private static readonly Dictionary<DisplayRotation, Size> _rotationToPhysicalBounds = new ();
    public static DisplayRotation DisplayRotation { get; set; }
    public static DisplayInfo DisplayInfo { get; set; }
    public static string CurrentMacroSetPackage { get; set; }
    public static Point TopLeft 
    { 
        get 
        {
            var currentWindowBounds = ResolveUsableResolution();
            return currentWindowBounds.Location;
        }
    }
    public static Size UsableResolution
    {
        get
        {
            var currentWindowBounds = ResolveUsableResolution();
            return currentWindowBounds.Size;
        }
    }
    public static Size PhysicalResolution
    {
        get
        {
            return ResolvePhysicalBounds();
        }
    }

    private static Rect ResolveUsableResolution()
    {
        if (!_rotationToUsableBounds.ContainsKey(DisplayRotation))
        {
#if ANDROID
            _rotationToUsableBounds.Add(DisplayRotation, YeetMacro2.Platforms.Android.Services.AndroidScreenService.GetWindowBounds(DisplayRotation));
#elif WINDOWS
            _rotationToUsableBounds.Add(DisplayRotation, new Rect(0, 0, DisplayInfo.Width, DisplayInfo.Height));
#endif
        }

        return _rotationToUsableBounds[DisplayRotation];
    }

    private static Size ResolvePhysicalBounds()
    {
        if (!_rotationToPhysicalBounds.ContainsKey(DisplayRotation))
        {
            var width = DisplayInfo.Width;
            var height = DisplayInfo.Height;
            if (DisplayInfo.Rotation == DisplayRotation.Rotation0 && DisplayInfo.Orientation == DisplayOrientation.Portrait)
            {
                width = DisplayRotation == DisplayRotation.Rotation0 || DisplayRotation == DisplayRotation.Rotation180 ? DisplayInfo.Width : DisplayInfo.Height;
                height = DisplayRotation == DisplayRotation.Rotation0 || DisplayRotation == DisplayRotation.Rotation180 ? DisplayInfo.Height : DisplayInfo.Width;
            }
           
            _rotationToPhysicalBounds.Add(DisplayRotation, new Size(width, height));
        }

        return _rotationToPhysicalBounds[DisplayRotation];
    }
}

public class Pattern: ISortable
{
    public virtual Rect RawBounds { get; set; }
    public virtual Size Resolution { get; set; }
    [JsonIgnore]
    public virtual bool IsSelected { get; set; }
    public virtual PatternType Type { get; set; }
    public virtual SwipeDirection SwipeDirection { get; set; }
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
            // https://developer.android.com/develop/ui/views/layout/edge-to-edge#enable-edge-to-edge-display
            // Edge to edge display is enabled by default for Android 15 (SDK 35)
            // Even when full screen Eversoul respects offset
            var isFullScreen = OperatingSystem.IsAndroidVersionAtLeast(35) && DisplayHelper.CurrentMacroSetPackage != "com.kakaogames.eversoul";
            var xOffset = 0;
            var yOffset = 0;
            var topLeft = isFullScreen ? Point.Zero : DisplayHelper.TopLeft;
            var usableResolution = isFullScreen ? DisplayHelper.PhysicalResolution : DisplayHelper.UsableResolution;
            var physicalResolution = DisplayHelper.PhysicalResolution;
            var rightMargin = topLeft.X != 0 ? 0: (int)physicalResolution.Width - (int)usableResolution.Width;

            switch (OffsetCalcType)
            {
                case OffsetCalcType.DockLeft:
                    return topLeft;
                case OffsetCalcType.Default:
                case OffsetCalcType.Center:
                    {   // horizontal center handling
                        var deltaX = physicalResolution.Width - Resolution.Width;
                        //var deltaX = usableResolution.Width - Resolution.Width + (topLeft.X * 2);
                        xOffset = (int)((deltaX / 2) + (topLeft.X / 2) - (rightMargin / 2));
                    }
                    break;
                case OffsetCalcType.DockRight:
                    {   // horizontal dock right handling (dock left does not need handling)
                        var right = Resolution.Width - RawBounds.X;
                        var targetX = physicalResolution.Width - right - rightMargin;
                        //var targetX = usableResolution.Width - right + topLeft.X;
                        //var targetX = currentResolution.Width - right + topLeft.X - bottomRightOffset.X;
                        xOffset = (int)(targetX - RawBounds.X);
                    }
                    break;
                case OffsetCalcType.HorizontalStretchOffset:
                    {
                        // HorizontalStretchMultiplier = targetXOffset / deltaX
                        var deltaX = physicalResolution.Width - Resolution.Width;
                        xOffset = (int)(deltaX * HorizontalStretchMultiplier) + (int)topLeft.X;
                    }
                    break;
                case OffsetCalcType.VerticalStretchOffset:
                    {
                        // HorizontalStretchMultiplier = targetYOffset / deltaY
                        var deltaY = physicalResolution.Height - Resolution.Height;
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
                            return new Rect(RawBounds.Location, new Size(RawBounds.Width + DisplayHelper.PhysicalResolution.Width - Resolution.Width, RawBounds.Width));
                        case OffsetCalcType.DockRight:
                            return new Rect(RawBounds.Location.Offset(Resolution.Width - DisplayHelper.PhysicalResolution.Width, 0), new Size(RawBounds.Width + DisplayHelper.PhysicalResolution.Width - Resolution.Width, RawBounds.Width));
                        default:
                            return new Rect(RawBounds.Location.Offset((Resolution.Width - DisplayHelper.PhysicalResolution.Width) / 2.0, 0), new Size(RawBounds.Width + DisplayHelper.PhysicalResolution.Width - Resolution.Width, RawBounds.Width));
                    }
                case BoundsCalcType.FillHeight:
                    return new Rect(RawBounds.Location, new Size(RawBounds.Width, RawBounds.Height + DisplayHelper.PhysicalResolution.Height - Resolution.Height));
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

public enum PatternType
{
    Normal,
    Bounds,
    Swipe
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

public enum SwipeDirection
{
    Auto,
    LeftToRight,
    RightToLeft,
    TopToBottom,
    BottomToTop
}
