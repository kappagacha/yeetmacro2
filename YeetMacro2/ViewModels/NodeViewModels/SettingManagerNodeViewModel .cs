﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.ViewModels;

public partial class SettingNodeManagerViewModel : NodeManagerViewModel<ParentSettingViewModel, ParentSetting, SettingNode>
{
    ParentSetting _emptyParentSetting = new ParentSettingViewModel();
    IRepository<SettingNode> _settingRepository;
    [ObservableProperty]
    PatternNode _selectedPatternNode;
    [ObservableProperty]
    Pattern _selectedPattern;
    [ObservableProperty]
    ParentSetting _currentSubViewModel;

    public SettingNodeManagerViewModel(
        int rootNodeId,
        IRepository<SettingNode> settingRepository,
        INodeService<ParentSetting, SettingNode> nodeService,
        IInputService inputService,
        IToastService toastService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {
        _settingRepository = settingRepository;
        PropertyChanged += SettingNodeManagerViewModel_PropertyChanged;
        CurrentSubViewModel = _emptyParentSetting;
    }

    public async Task OnScriptNodeSelected(ScriptNode scriptNode)
    {
        if (Root is null) await this.WaitForInitialization();
        if (scriptNode is null)
        {
            CurrentSubViewModel = _emptyParentSetting;
            return;
        }

        var targetName = scriptNode.Name;
        var targetNode = Root.Nodes.FirstOrDefault(sn => sn.Name?.ToLower() == targetName.ToLower()) as ParentSetting;
        CurrentSubViewModel = targetNode ?? _emptyParentSetting;

        if (SelectedNode is not null)
        {
            SelectNode(SelectedNode);   // should unselect the node
        }
    }

    private void SettingNodeManagerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingNodeManagerViewModel.SelectedNode) && this.SelectedNode is PatternSetting patternSetting)
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
        else if (e.PropertyName == nameof(SettingNodeManagerViewModel.SelectedNode))
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
            _settingRepository.Save();
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
            _settingRepository.Save();
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

    [RelayCommand]
    public void SaveSetting(SettingNode setting)
    {
        _settingRepository.Update(setting);
        _settingRepository.Save();
    }

    [RelayCommand]
    public void ResetSetting(SettingNode setting)
    {
        if (setting is null) return;

        if (setting is EnabledOptionSettingViewModel enabledOptionSetting)
        {
            enabledOptionSetting.Value = enabledOptionSetting.DefaultValue;
            _settingRepository.Update(enabledOptionSetting);
            _settingRepository.Save();
        }
        else if (setting is OptionSettingViewModel optionSetting)
        {
            optionSetting.Value = optionSetting.DefaultValue;
            _settingRepository.Update(optionSetting);
            _settingRepository.Save();
        }
        else if (setting is EnabledStringSettingViewModel enabledStringSetting)
        {
            enabledStringSetting.Value = enabledStringSetting.DefaultValue;
            _settingRepository.Update(enabledStringSetting);
            _settingRepository.Save();
        }
        else if (setting is StringSettingViewModel stringSetting)
        {
            stringSetting.Value = stringSetting.DefaultValue;
            _settingRepository.Update(stringSetting);
            _settingRepository.Save();
        }
        else if (setting is BooleanSettingViewModel boolSetting)
        {
            boolSetting.Value = boolSetting.DefaultValue;
            _settingRepository.Update(boolSetting);
            _settingRepository.Save();
        }
        else if (setting is IntegerSettingViewModel integerSetting)
        {
            integerSetting.Value = integerSetting.DefaultValue;
            _settingRepository.Update(integerSetting);
            _settingRepository.Save();
        }
        else if (setting is EnabledIntegerSettingViewModel enabledIntegerSetting)
        {
            enabledIntegerSetting.Value = enabledIntegerSetting.DefaultValue;
            _settingRepository.Update(enabledIntegerSetting);
            _settingRepository.Save();
        }
    }
}