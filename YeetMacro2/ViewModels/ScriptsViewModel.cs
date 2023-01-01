using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

// https://stackoverflow.com/questions/53884417/net-core-di-ways-of-passing-parameters-to-constructor
public class ScriptsViewModelFactory
{
    IServiceProvider _serviceProvider;
    public ScriptsViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ScriptsViewModel Create(int macroSetId)
    {
        return ActivatorUtilities.CreateInstance<ScriptsViewModel>(_serviceProvider, macroSetId);
    }
}

public partial class ScriptsViewModel : ObservableObject
{
    int _macroSetId;
    [ObservableProperty]
    Script _selectedScript;
    [ObservableProperty]
    ICollection<Script> _scripts;
    IRepository<Script> _scriptRepository;
    IToastService _toastService;
    public ScriptsViewModel(
        int macroSetId,
        IRepository<Script> scriptRepository,
        IToastService toastService)
    {
        _macroSetId = macroSetId;
        _scriptRepository = scriptRepository;
        _toastService = toastService;
        var scripts = _scriptRepository.Get(s => s.MacroSetId == macroSetId);
        _scriptRepository.DetachEntities(scripts.ToArray());
        Scripts = ProxyViewModel.CreateCollection(scripts);
        _scriptRepository.AttachEntities(Scripts.ToArray());
    }

    [RelayCommand]
    private async void AddScript()
    {
        var name = await Application.Current.MainPage.DisplayPromptAsync("Script", "Enter name...");
        if (string.IsNullOrWhiteSpace(name))
        {
            _toastService.Show("Canceled add script");
            return;
        }

        var newScript = ProxyViewModel.Create(new Script() { Name = name, MacroSetId = _macroSetId });
        Scripts.Add(newScript);
        _scriptRepository.Insert(newScript);
        _scriptRepository.Save();
        _toastService.Show($"Added Script: {newScript.Name}");
    }

    [RelayCommand]
    public async Task DeleteScript(Script script)
    {
        if (!await Application.Current.MainPage.DisplayAlert("Delete Script", "Are you sure?", "Ok", "Cancel")) return;

        Scripts.Remove(script);
        _scriptRepository.Delete(script);
        _scriptRepository.Save();
        _toastService.Show($"Deleted Script: {script.Name}");
    }

    [RelayCommand]
    public void UpdateScript(Script script)
    {
        _scriptRepository.Update(script);
        _scriptRepository.Save();
        _toastService.Show($"Updated Script: {script.Name}");
    }

    [RelayCommand]
    private void SelectScript(Script script)
    {
        script.IsSelected = !script.IsSelected;

        if (SelectedScript != null && SelectedScript != script)
        {
            SelectedScript.IsSelected = false;
        }

        if (script.IsSelected && SelectedScript != script)
        {
            SelectedScript = script;
        }
        else if (!script.IsSelected)
        {
            SelectedScript = null;
        }
    }
}
