using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class SettingNodeViewModel : NodeViewModel<ParentSetting, SettingNode>
{
    IRepository<SettingNode> _settingRepository;
    [ObservableProperty]
    PatternNode _selectedPatternNode;
    [ObservableProperty]
    Pattern _selectedPattern;

    public SettingNodeViewModel(
        int rootNodeId,
        IRepository<SettingNode> settingRepository,
        INodeService<ParentSetting, SettingNode> nodeService,
        IInputService inputService,
        IToastService toastService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {
        _settingRepository = settingRepository;
        PropertyChanged += SettingNodeViewModel_PropertyChanged; ;
    }

    private void SettingNodeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingNodeViewModel.SelectedNode) && this.SelectedNode is PatternSetting patternSetting)
        {
            this.SelectedPatternNode = patternSetting.Value;
            if (patternSetting.Value.Patterns.Count > 0)
            {
                this.SelectedPattern = patternSetting.Value.Patterns.First();
            }
            else
            {
                this.SelectedPattern = null;
            }
        } 
        else if (e.PropertyName == nameof(SettingNodeViewModel.SelectedNode))
        {
            this.SelectedPatternNode = null;
            this.SelectedPattern = null;
        }
    }

    [RelayCommand]
    public async Task AddOption(object setting)
    {
        if (setting is OptionSetting optionSetting)
        {
            var newOption = await _inputService.PromptInput("New option value");
            if (String.IsNullOrEmpty(newOption)) return;

            optionSetting.Options.Add(newOption);
            _settingRepository.Update(optionSetting);
        }
    }

    [RelayCommand]
    public async Task SelectOption(object setting)
    {
        if (setting is OptionSetting optionSetting)
        {
            var selectedOption = await _inputService.SelectOption("Select option", optionSetting.Options.ToArray());
            if (String.IsNullOrEmpty(selectedOption) || selectedOption == "ok" || selectedOption == "cancel") return;

            optionSetting.Value = selectedOption;
            _settingRepository.Update(optionSetting);
        }
    }

    [RelayCommand]
    private void SelectPattern(Pattern pattern)
    {
        if (SelectedPattern != null && SelectedPattern != pattern)
        {
            SelectedPattern.IsSelected = false;
        }

        if (pattern == null)
        {
            SelectedPattern = null;
            return;
        }

        pattern.IsSelected = !pattern.IsSelected;

        if (pattern.IsSelected && SelectedPattern != pattern)
        {
            SelectedPattern = pattern;
        }
        else if (!pattern.IsSelected)
        {
            SelectedPattern = null;
        }
    }
}