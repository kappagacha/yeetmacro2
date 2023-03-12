using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace YeetMacro2.Data.Models;

public enum SettingType
{
    Parent,
    Boolean,
    Option
}

public class SettingNodeMetadataProvider : INodeMetadataProvider<Setting>
{
    public Expression<Func<Setting, object>> CollectionPropertiesExpression => s => new { ((ParentSetting)s).Nodes };
    public Expression<Func<Setting, object>> ProxyPropertiesExpression => null;

    public Type[] NodeTypes => new Type[] { typeof(ParentSetting), typeof(BooleanSetting), typeof(OptionSetting) };
}

public class ParentSetting : Setting, IParentNode<ParentSetting, Setting>
{
    public ICollection<Setting> Nodes { get; set; } = new List<Setting>();
    public override SettingType SettingType => SettingType.Parent;
}


// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0
[JsonDerivedType(typeof(ParentSetting), typeDiscriminator: "parent")]
[JsonDerivedType(typeof(BooleanSetting), typeDiscriminator: "boolean")]
[JsonDerivedType(typeof(OptionSetting), typeDiscriminator: "option")]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[NodeMetadata(NodeMetadataProvider = typeof(SettingNodeMetadataProvider))]
public abstract class Setting : Node
{
    public abstract SettingType SettingType { get; }
}

public abstract class Setting<T> : Setting
{
    public virtual T Value { get; set; }
}

public class BooleanSetting: Setting<Boolean>
{
    public override SettingType SettingType => SettingType.Boolean;
}

public class OptionSetting : Setting<String>
{
    public override SettingType SettingType => SettingType.Option;
    public ICollection<String> Options { get; set; } = new List<string>();
}