using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public enum SettingType
{
    Parent,
    Boolean,
    Option,
    String,
    Integer,
    Pattern
}

public class SettingNodeMetadataProvider : INodeMetadataProvider<SettingNode>
{
    public Expression<Func<SettingNode, object>> CollectionPropertiesExpression => s => new { ((ParentSetting)s).Nodes };
    public Expression<Func<SettingNode, object>> ProxyPropertiesExpression => s => new { ((PatternSetting)s).Value };

    public Type[] NodeTypes => new Type[] { typeof(ParentSetting), typeof(BooleanSetting), typeof(OptionSetting), typeof(StringSetting), typeof(IntegerSetting), typeof(PatternSetting) };
}

public class ParentSetting : SettingNode, IParentNode<ParentSetting, SettingNode>
{
    [JsonIgnore]
    public virtual ICollection<SettingNode> Nodes { get; set; } = new List<SettingNode>();
    public override SettingType SettingType => SettingType.Parent;
    public override TTarget GetValue<TTarget>()
    {
        throw new InvalidOperationException("ParentSetting does not have a value.");
    }
}

// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0
[JsonDerivedType(typeof(ParentSetting), typeDiscriminator: "parent")]
[JsonDerivedType(typeof(BooleanSetting), typeDiscriminator: "boolean")]
[JsonDerivedType(typeof(OptionSetting), typeDiscriminator: "option")]
[JsonDerivedType(typeof(StringSetting), typeDiscriminator: "string")]
[JsonDerivedType(typeof(IntegerSetting), typeDiscriminator: "integer")]
[JsonDerivedType(typeof(PatternSetting), typeDiscriminator: "pattern")]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[NodeMetadata(NodeMetadataProvider = typeof(SettingNodeMetadataProvider))]
public abstract class SettingNode : Node
{
    public abstract SettingType SettingType { get; }
    public abstract T GetValue<T>();
    public virtual SettingNode this[string key]
    {
        get
        {
            throw new InvalidOperationException($"Not implemented for type: {this.GetType()}");
        }
    }
}

public abstract class SettingNode<T> : SettingNode
{
    public virtual T Value { get; set; }
    public virtual T DefaultValue { get; set; }
    public override TTarget GetValue<TTarget>()
    {
        return (TTarget)(object)Value;
    }
}

public class BooleanSetting: SettingNode<Boolean>
{
    public override SettingType SettingType => SettingType.Boolean;
}

public class OptionSetting : SettingNode<String>
{
    public override SettingType SettingType => SettingType.Option;
    public virtual ICollection<String> Options { get; set; } = new List<String>();
}

public class StringSetting : SettingNode<String>
{
    public override SettingType SettingType => SettingType.String;
}

public class IntegerSetting : SettingNode<int>
{
    public override SettingType SettingType => SettingType.Integer;
    public virtual bool IsActive { get; set; }
    public virtual int Increment { get; set; } = 1;
}

public class PatternSetting : SettingNode<PatternNode>
{
    public override SettingType SettingType => SettingType.Pattern;

    public PatternSetting()
    {
        Value = new PatternNode();
    }
}