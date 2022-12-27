using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;

namespace YeetMacro2.ViewModels;
public partial class MacroManagerViewModel : ObservableObject
{
    IRepository<MacroSet> _macroSetRepository;
    [ObservableProperty]
    ObservableCollection<MacroSet> _macroSets;

    public MacroManagerViewModel()
	{

	}

    public MacroManagerViewModel(IRepository<MacroSet> macroSetRepository)
    {
        _macroSetRepository = macroSetRepository;
        _macroSets = new ObservableCollection<MacroSet>(_macroSetRepository.Get());
    }

    [RelayCommand]
    public async Task AddMacroSet()
    {
        string macroSetName = await Application.Current.MainPage.DisplayPromptAsync("Macro Set", "Enter name...");
        if (string.IsNullOrEmpty(macroSetName)) return;

        var macroSet = new MacroSet() { Name = macroSetName };
        _macroSetRepository.Insert(macroSet);
        _macroSetRepository.Save();
    }
}
