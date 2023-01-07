using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;
public class Script
{
    [JsonIgnore]
    public int ScriptId { get; set; }
    [JsonIgnore]
    public int MacroSetId { get; set; }
    public virtual string Name { get; set; }
    public virtual string Text { get; set; }
    public virtual bool IsSelected { get; set; }
}
