using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public enum SettingType
{
    Parent,
    Boolean,
    Option
}

public class SettingNodeMetadataProvider : INodeMetadataProvider<SettingNode>
{
    public Expression<Func<SettingNode, object>> CollectionPropertiesExpression => s => new { ((ParentSetting)s).Nodes };
    public Expression<Func<SettingNode, object>> ProxyPropertiesExpression => null;

    public Type[] NodeTypes => new Type[] { typeof(ParentSetting), typeof(BooleanSetting), typeof(OptionSetting) };
}

public class ParentSetting : SettingNode, IParentNode<ParentSetting, SettingNode>
{
    [JsonIgnore]
    public ICollection<SettingNode> Nodes { get; set; } = new List<SettingNode>();
    public override SettingType SettingType => SettingType.Parent;
}

// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0
[JsonDerivedType(typeof(ParentSetting), typeDiscriminator: "parent")]
[JsonDerivedType(typeof(BooleanSetting), typeDiscriminator: "boolean")]
[JsonDerivedType(typeof(OptionSetting), typeDiscriminator: "option")]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[NodeMetadata(NodeMetadataProvider = typeof(SettingNodeMetadataProvider))]
public abstract class SettingNode : Node
{
    public abstract SettingType SettingType { get; }
}

public abstract class SettingNode<T> : SettingNode
{
    public virtual T Value { get; set; }
}

public class BooleanSetting: SettingNode<Boolean>
{
    public override SettingType SettingType => SettingType.Boolean;
}

public class OptionSetting : SettingNode<String>
{
    public override SettingType SettingType => SettingType.Option;
    public ICollection<String> Options { get; set; } = new List<string>();
}