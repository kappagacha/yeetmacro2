namespace YeetMacro2.Data.Models;

public enum SettingType
{
    Parent,
    Boolean,
    Option
}

public class ParentSetting : Setting, IParentNode<ParentSetting, Setting>
{
    public ICollection<Setting> Children { get; set; } = new List<Setting>();
    public override SettingType SettingType => SettingType.Parent;
}

public abstract class Setting : Node
{
    public abstract SettingType SettingType { get; }
}

public abstract class Setting<T> : Setting
{
    public T Value { get; set; }
}

public class BooleanSetting: Setting<Boolean>
{
    public override SettingType SettingType => SettingType.Boolean;
}

public class OptionSetting : Setting<String>
{
    public override SettingType SettingType => SettingType.Option;
    public String[] Options { get; set; }
}