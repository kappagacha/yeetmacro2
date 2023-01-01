namespace YeetMacro2.Data.Models;
public class Script
{
    public int ScriptId { get; set; }
    public int MacroSetId { get; set; }
    public virtual string Name { get; set; }
    public virtual string Text { get; set; }
    public virtual bool IsSelected { get; set; }
}
