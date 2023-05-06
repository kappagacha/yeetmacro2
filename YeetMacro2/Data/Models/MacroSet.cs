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
    public virtual PatternNode RootPattern { get; set; }
    [JsonIgnore]
    public virtual int RootScriptNodeId { get; set; }
    [JsonIgnore]
    public virtual ScriptNode RootScript { get; set; }
    [JsonIgnore]
    public virtual int RootSettingNodeId { get; set; }
    [JsonIgnore]
    public virtual ParentSetting RootSetting { get; set; }
    public virtual Size Resolution { get; set; }
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
        return $"{Type}: {Link}";
    }
}

public class MacroSetHash
{
    public string MacroSet { get; set; }
    public string Patterns { get; set; }
    public string Scripts { get; set; }
    public string Settings { get; set; }
}