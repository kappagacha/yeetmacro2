namespace YeetMacro2.Services;
public interface IInputService
{
    Task<string> PromptInput(string message);
    Task<string> SelectOption(string message, params string[] options);
    Task<Rect> DrawUserRectangle();
}
