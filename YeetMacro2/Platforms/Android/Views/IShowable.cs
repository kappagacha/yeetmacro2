namespace YeetMacro2.Platforms.Android.Views;
public interface IShowable
{
    void Show();
    void Close();
    void CloseCancel();
    Task<bool> WaitForClose();
    VisualElement VisualElement { get; }
}
