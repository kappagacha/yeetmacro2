namespace YeetMacro2.Models;

public class SelectOption
{
    public string Text { get; set; }
    public string FontFamily { get; set; }
    public string Glyph { get; set; }

    public SelectOption(string text)
    {
        Text = text;
    }

    public SelectOption(string text, string fontFamily, string glyph)
    {
        Text = text;
        FontFamily = fontFamily;
        Glyph = glyph;
    }

    public static implicit operator SelectOption(string text)
    {
        return new SelectOption(text);
    }
}
