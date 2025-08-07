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
    [JsonIgnore]
    public virtual int RootWeeklyNodeId { get; set; }
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
    public virtual DateTimeOffset? WeeklyTemplateLastUpdated { get; set; }
    public virtual DateTimeOffset? DailyTemplateLastUpdated { get; set; }
    public virtual string Description { get; set; }
    public string DailyTemplate { get; set; }
    public virtual int DailyResetUtcHour { get; set; }
    public string WeeklyTemplate { get; set; }
    public virtual DayOfWeek WeeklyStartDay { get; set; } = DayOfWeek.Monday;
    [JsonIgnore]
    public virtual CutoutCalculationType CutoutCalculationType { get; set; } = CutoutCalculationType.Normal;
    [JsonIgnore]
    public virtual bool UsePatternsSnapshot { get; set; } = true;
}

public enum CutoutCalculationType
{
    Normal,
    Center,
    None
}