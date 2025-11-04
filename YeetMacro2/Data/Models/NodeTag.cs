using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public class NodeTag
{
    [JsonIgnore]
    public int TagId { get; set; }

    [JsonIgnore]
    public int MacroSetId { get; set; }

    public virtual string Name { get; set; }

    public virtual string FontFamily { get; set; }

    public virtual string Glyph { get; set; }

    public virtual int Position { get; set; }
}
