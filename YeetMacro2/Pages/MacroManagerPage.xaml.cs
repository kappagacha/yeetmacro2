using YeetMacro2.Services;
using YeetMacro2.ViewModels;
using YeetMacro2.Views;

namespace YeetMacro2.Pages;

public partial class MacroManagerPage : ContentPage
{
    private bool _patternsTabLoaded = false;
    private bool _settingsTabLoaded = false;
    private bool _scriptsTabLoaded = false;
    private bool _dailiesTabLoaded = false;
    private bool _weekliesTabLoaded = false;
    private bool _tagsTabLoaded = false;

	public MacroManagerPage()
	{
		InitializeComponent();
    }

    private void OnTabButtonClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var tabName = button?.CommandParameter as string;
        
        // Reset all tab buttons to default state
        MacroSetTabButton.BackgroundColor = Colors.Transparent;
        MacroSetTabButton.TextColor = (Color)Application.Current.Resources["Primary"];
        PatternsTabButton.BackgroundColor = Colors.Transparent;
        PatternsTabButton.TextColor = (Color)Application.Current.Resources["Primary"];
        SettingsTabButton.BackgroundColor = Colors.Transparent;
        SettingsTabButton.TextColor = (Color)Application.Current.Resources["Primary"];
        ScriptsTabButton.BackgroundColor = Colors.Transparent;
        ScriptsTabButton.TextColor = (Color)Application.Current.Resources["Primary"];
        DailiesTabButton.BackgroundColor = Colors.Transparent;
        DailiesTabButton.TextColor = (Color)Application.Current.Resources["Primary"];
        WeekliesTabButton.BackgroundColor = Colors.Transparent;
        WeekliesTabButton.TextColor = (Color)Application.Current.Resources["Primary"];
        TagsTabButton.BackgroundColor = Colors.Transparent;
        TagsTabButton.TextColor = (Color)Application.Current.Resources["Primary"];

        // Hide all tab contents
        MacroSetTabContent.IsVisible = false;
        PatternsTabContent.IsVisible = false;
        SettingsTabContent.IsVisible = false;
        ScriptsTabContent.IsVisible = false;
        DailiesTabContent.IsVisible = false;
        WeekliesTabContent.IsVisible = false;
        TagsTabContent.IsVisible = false;
        
        // Show selected tab and update button state
        switch (tabName)
        {
            case "MacroSet":
                MacroSetTabContent.IsVisible = true;
                MacroSetTabButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                MacroSetTabButton.TextColor = Colors.White;
                break;
            case "Patterns":
                if (!_patternsTabLoaded)
                {
                    var viewModel = ServiceHelper.GetService<MacroManagerViewModel>();
                    var patternsView = new PatternNodeView();
                    patternsView.BindingContext = viewModel;
                    patternsView.SetBinding(PatternNodeView.MacroSetProperty, new Binding("SelectedMacroSet"));
                    PatternsTabContent.Content = patternsView;
                    _patternsTabLoaded = true;
                }
                PatternsTabContent.IsVisible = true;
                PatternsTabButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                PatternsTabButton.TextColor = Colors.White;
                break;
            case "Settings":
                if (!_settingsTabLoaded)
                {
                    var viewModel = ServiceHelper.GetService<MacroManagerViewModel>();
                    var settingsView = new SettingNodeView();
                    settingsView.BindingContext = viewModel;
                    settingsView.SetBinding(SettingNodeView.MacroSetProperty, new Binding("SelectedMacroSet"));
                    SettingsTabContent.Content = settingsView;
                    _settingsTabLoaded = true;
                }
                SettingsTabContent.IsVisible = true;
                SettingsTabButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                SettingsTabButton.TextColor = Colors.White;
                break;
            case "Scripts":
                if (!_scriptsTabLoaded)
                {
                    var viewModel = ServiceHelper.GetService<MacroManagerViewModel>();
                    var scriptsView = new ScriptNodeView();
                    scriptsView.SetBinding(ScriptNodeView.MacroSetProperty, new Binding("SelectedMacroSet") { Source = viewModel });
                    ScriptsTabContent.Content = scriptsView;
                    _scriptsTabLoaded = true;
                }
                ScriptsTabContent.IsVisible = true;
                ScriptsTabButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                ScriptsTabButton.TextColor = Colors.White;
                break;
            case "Dailies":
                if (!_dailiesTabLoaded)
                {
                    var viewModel = ServiceHelper.GetService<MacroManagerViewModel>();
                    var dailiesView = new TodoNodeView();
                    dailiesView.BindingContext = viewModel;
                    dailiesView.SetBinding(TodoNodeView.TodosProperty, new Binding("SelectedMacroSet.Dailies"));
                    DailiesTabContent.Content = dailiesView;
                    _dailiesTabLoaded = true;
                }
                DailiesTabContent.IsVisible = true;
                DailiesTabButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                DailiesTabButton.TextColor = Colors.White;
                break;
            case "Weeklies":
                if (!_weekliesTabLoaded)
                {
                    var viewModel = ServiceHelper.GetService<MacroManagerViewModel>();
                    var weekliesView = new TodoNodeView();
                    weekliesView.BindingContext = viewModel;
                    weekliesView.SetBinding(TodoNodeView.TodosProperty, new Binding("SelectedMacroSet.Weeklies"));
                    WeekliesTabContent.Content = weekliesView;
                    _weekliesTabLoaded = true;
                }
                WeekliesTabContent.IsVisible = true;
                WeekliesTabButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                WeekliesTabButton.TextColor = Colors.White;
                break;
            case "Tags":
                if (!_tagsTabLoaded)
                {
                    var viewModel = ServiceHelper.GetService<MacroManagerViewModel>();
                    var tagsView = new TagManagementView();
                    // Only use binding, don't set BindingContext directly
                    tagsView.SetBinding(TagManagementView.BindingContextProperty, new Binding("SelectedMacroSet.TagManager", source: viewModel));
                    TagsTabContent.Content = tagsView;
                    _tagsTabLoaded = true;
                }
                TagsTabContent.IsVisible = true;
                TagsTabButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                TagsTabButton.TextColor = Colors.White;
                break;
        }
    }
}