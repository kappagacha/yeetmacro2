using System.Dynamic;

namespace YeetMacro2.Data.Models;
public class PatternNode : Node, IParentNode<PatternNode, PatternNode>
{
    public virtual bool IsMultiPattern { get; set; }
    public virtual ICollection<PatternNode> Children { get; set; } = new List<PatternNode>();
    public virtual ICollection<Pattern> Patterns { get; set; }
    public virtual ICollection<UserPattern> UserPatterns { get; set; }
    public dynamic BuildDynamicObject(string path = "")
    {
        var result = new ExpandoObject();
        var resolvedPath = path + (String.IsNullOrEmpty(path) ? "" : ".") + Name;
        if (resolvedPath == "root") { resolvedPath = ""; }
        ((IDictionary<String, Object>)result)["patterns"] = Patterns;
        ((IDictionary<String, Object>)result)["metadata"] = this;
        ((IDictionary<String, Object>)result)["path"] = resolvedPath;

        foreach (var node in Children)
        {
            ((IDictionary<String, Object>)result)[node.Name] = node.BuildDynamicObject(resolvedPath);
        }

        return result;
    }
}

public abstract class PatternBase
{
    public virtual Bounds Bounds { get; set; }
    public virtual Resolution Resolution { get; set; }
    public virtual bool IsSelected { get; set; }
    public virtual int PatternId { get; set; }
    public virtual int ParentNodeId { get; set; }
    public virtual string Name { get; set; }
    public byte[] ImageData { get; set; }
}

public class Bounds
{
    public virtual double X { get; set; }
    public virtual double Y { get; set; }
    public virtual double W { get; set; }
    public virtual double H { get; set; }
    public override string ToString()
    {
        return $"x{X:F0},y{Y:F0},w{W:F0},h{H:F0}";
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

public class Pattern : PatternBase
{

}

public class UserPattern : PatternBase
{

}