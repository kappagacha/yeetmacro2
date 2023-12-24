using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public class MacroSet
{
    [JsonIgnore]
    public int MacroSetId { get; set; }
    public virtual string Name { get; set; }
    [JsonIgnore]
    public virtual int RootPatternNodeId { get; set; }
    [JsonIgnore]
    public virtual int RootScriptNodeId { get; set; }
    [JsonIgnore]
    public virtual int RootSettingNodeId { get; set; }
    [JsonIgnore]
    public virtual int RootDailyNodeId { get; set; }
    public virtual Size Resolution { get; set; }
    public virtual Point DefaultLocation { get; set; }
    public virtual bool SupportsGreaterWidth { get; set; }
    public virtual bool SupportsGreaterHeight { get; set; }
    public virtual string Package { get; set; }
    public virtual string Source { get; set; }
    public virtual DateTimeOffset? MacroSetLastUpdated { get; set; }
    public virtual DateTimeOffset? PatternsLastUpdated { get; set; }
    public virtual DateTimeOffset? ScriptsLastUpdated { get; set; }
    public virtual DateTimeOffset? SettingsLastUpdated { get; set; }
    public virtual string Description { get; set; }
    public virtual string DailyTemplate { get; set; }
    public virtual int DailyResetUtcHour { get; set; }
}