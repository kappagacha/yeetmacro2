namespace YeetMacro2.Data.Models;
public class MacroSet
{
    public int MacroSetId { get; set; }
    public virtual string Name { get; set; }
    public virtual string Source { get; set; }
    public virtual int RootPatternNodeId { get; set; }
    public virtual PatternNode RootPattern { get; set; }
    public virtual ICollection<Script> Scripts { get; set; }
    public virtual int RootSettingNodeId { get; set; }
    public virtual ParentSetting RootSetting { get; set; }
}
