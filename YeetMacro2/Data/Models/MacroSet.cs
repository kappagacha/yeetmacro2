namespace YeetMacro2.Data.Models;
public class MacroSet
{
    public int MacroSetId { get; set; }
    public virtual string Name { get; set; }
    public virtual int RootPatternNodeId { get; set; }
    public virtual PatternNode RootPattern { get; set; }
    public virtual int RootScriptNodeId { get; set; }
    public virtual ScriptNode RootScript { get; set; }
    public virtual int RootSettingNodeId { get; set; }
    public virtual ParentSetting RootSetting { get; set; }
    public virtual Resolution Resolution { get; set; }
    public virtual string Package { get; set; }
    public virtual MacroSetSource Source { get; set; }
}

public enum MacroSetSourceType
{
    LOCAL_ASSET
}

public class MacroSetSource
{
    public virtual MacroSetSourceType Type { get; set; }
    public virtual string Link { get; set; }
    public override string ToString()
    {
        return $"{Type}: {Type}";
    }
}