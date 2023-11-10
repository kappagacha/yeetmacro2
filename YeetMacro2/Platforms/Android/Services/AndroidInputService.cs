using YeetMacro2.Platforms.Android.ViewModels;
using YeetMacro2.Platforms.Android.Views;
using YeetMacro2.Services;

namespace YeetMacro2.Platforms.Android.Services;
public class AndroidInputService : IInputService
{
    AndroidScreenService _screenService;
    public AndroidInputService(AndroidScreenService screenService)
    {
        _screenService = screenService;
    }

    public async Task<Rect> DrawUserRectangle()
    {
        _screenService.IsDrawing = true;
        var patternsViewIsShowing = _screenService.Views.TryGetValue(AndroidWindowView.PatternsNodeView, out IShowable patternsView) && patternsView.IsShowing;
        var macroOverlayViewIsShowing = _screenService.Views.TryGetValue(AndroidWindowView.MacroOverlayView, out IShowable macroOverlayView) && macroOverlayView.IsShowing;
        var scriptsViewIsShowing = _screenService.Views.TryGetValue(AndroidWindowView.ScriptsNodeView, out IShowable scriptsView) && scriptsView.IsShowing;
        if (patternsViewIsShowing) _screenService.Views[AndroidWindowView.PatternsNodeView].Close();
        if (macroOverlayViewIsShowing) _screenService.Views[AndroidWindowView.MacroOverlayView].Close();
        if (scriptsViewIsShowing) _screenService.Views[AndroidWindowView.ScriptsNodeView].Close();

        _screenService.Show(AndroidWindowView.UserDrawView);
        var drawControl = (DrawControl)_screenService.Views[AndroidWindowView.UserDrawView].VisualElement;
        drawControl.ClearRectangles();
        var formsView = (FormsView)_screenService.Views[AndroidWindowView.UserDrawView];
        if (await formsView.WaitForClose())
        {
            if (patternsViewIsShowing) _screenService.Views[AndroidWindowView.PatternsNodeView].Show();
            if (macroOverlayViewIsShowing) _screenService.Views[AndroidWindowView.MacroOverlayView].Show();
            if (scriptsViewIsShowing) _screenService.Views[AndroidWindowView.ScriptsNodeView].Show();

            _screenService.IsDrawing = false;
            return drawControl.Rect;
        }
        _screenService.IsDrawing = false;
        return Rect.Zero;
    }

    public async Task<string> PromptInput(string message, string placeholderInput = "")
    {
        _screenService.Show(AndroidWindowView.PromptStringInputView);
        var viewModel = (PromptStringInputViewModel)_screenService.Views[AndroidWindowView.PromptStringInputView].VisualElement.BindingContext;
        viewModel.Message = message;
        if (!string.IsNullOrWhiteSpace(placeholderInput)) viewModel.Input = placeholderInput;
        var formsView = (FormsView)_screenService.Views[AndroidWindowView.PromptStringInputView];
        if (await formsView.WaitForClose())
        {
            return viewModel.Input;
        }
        return null;
    }

    public async Task<string> SelectOption(string message, params string[] options)
    {
        _screenService.Show(AndroidWindowView.PromptSelectOptionView);
        var viewModel = (PromptSelectOptionViewModel)_screenService.Views[AndroidWindowView.PromptSelectOptionView].VisualElement.BindingContext;
        viewModel.Message = message;
        // Needs x:DataType="{x:Type x:String}" in DataTemplate
        // https://stackoverflow.com/questions/75283345/collectionview-working-in-debug-but-not-in-release-in-net-maui
        viewModel.Options = options;
        var formsView = (FormsView)_screenService.Views[AndroidWindowView.PromptSelectOptionView];
        if (await formsView.WaitForClose())
        {
            return viewModel.SelectedOption;
        }
        return null;
    }
}
