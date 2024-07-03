using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.ViewModels;

public partial class SettingNodeManagerViewModel : NodeManagerViewModel<ParentSettingViewModel, ParentSetting, SettingNode>
{
    readonly ParentSetting _emptyParentSetting = new ParentSettingViewModel();
    readonly IRepository<SettingNode> _settingRepository;
    readonly IRepository<Pattern> _patternRepository;
    [ObservableProperty]
    PatternNode _selectedPatternNode;
    [ObservableProperty]
    Pattern _selectedPattern;
    [ObservableProperty]
    ParentSetting _currentSubViewModel;
    [ObservableProperty]
    bool _showResetButton;

    public SettingNodeManagerViewModel(
        int rootNodeId,
        IRepository<SettingNode> settingRepository,
        IRepository<Pattern> patternRepository,
        INodeService<ParentSetting, SettingNode> nodeService,
        IInputService inputService,
        IToastService toastService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {
        _settingRepository = settingRepository;
        _patternRepository = patternRepository;
        PropertyChanged += SettingNodeManagerViewModel_PropertyChanged;
        CurrentSubViewModel = _emptyParentSetting;
    }

    protected override void CustomInit()
    {
        foreach (var settingNode in _nodeService.GetDescendants<SettingNode>(Root).ToList())
        {
            if (settingNode is PatternSetting patternSetting)
            {
                _patternRepository.AttachEntities([..patternSetting.Value.Patterns]);
            }
        }
    }

    public async Task OnScriptNodeSelected(ScriptNode scriptNode)
    {
        if (Root is null) await this.WaitForInitialization();
        if (scriptNode is null)
        {
            CurrentSubViewModel = _emptyParentSetting;
            return;
        }

        var targetName = scriptNode.Name.TrimStart('_');
        var targetNode = Root.Nodes.FirstOrDefault(sn => (sn.Name?.ToLower()).Equals(targetName, StringComparison.CurrentCultureIgnoreCase)) as ParentSetting;
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
            var selectedOption = await _inputService.SelectOption("Select option", [.. optionSetting.Options]);
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
    public async Task ResetSetting(SettingNode setting)
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
        else if (setting is EnabledIntegerSettingViewModel enabledIntegerSetting)
        {
            enabledIntegerSetting.Value = enabledIntegerSetting.DefaultValue;
            _settingRepository.Update(enabledIntegerSetting);
            _settingRepository.Save();
        }
        else if (setting is IntegerSettingViewModel integerSetting)
        {
            integerSetting.Value = integerSetting.DefaultValue;
            _settingRepository.Update(integerSetting);
            _settingRepository.Save();
        }
        else if (setting is EnabledDoubleSettingViewModel enabledDoubleSetting)
        {
            enabledDoubleSetting.Value = enabledDoubleSetting.DefaultValue;
            _settingRepository.Update(enabledDoubleSetting);
            _settingRepository.Save();
        }
        else if (setting is DoubleSettingViewModel doubleSetting)
        {
            doubleSetting.Value = doubleSetting.DefaultValue;
            _settingRepository.Update(doubleSetting);
            _settingRepository.Save();
        }
        else if (setting is EnabledPatternSettingViewModel enabledPatternSetting)
        {
            enabledPatternSetting.Value = enabledPatternSetting.DefaultValue;
            _settingRepository.Update(enabledPatternSetting);
            _settingRepository.Save();
        }
        else if (setting is PatternSettingViewModel patternSetting)
        {
            var option = await _inputService.SelectOption($"Reset pattern setting {patternSetting.Name}?", "OK");
            if (option != "OK") return;

            foreach (var pattern in patternSetting.Value.Patterns)
            {
                _patternRepository.Delete(pattern);
            }
            patternSetting.Value.Patterns.Clear();
            _patternRepository.Save();

            if (patternSetting.DefaultValue is not null)
            {
                var newPatternNode = PatternNodeManagerViewModel.CloneNode(patternSetting.DefaultValue);
                foreach (var pattern in newPatternNode.Patterns)
                {
                    pattern.PatternNodeId = patternSetting.Value.NodeId;
                    patternSetting.Value.Patterns.Add(pattern);
                    _patternRepository.Insert(pattern);
                }
                _patternRepository.Save();
            }

            SelectedPattern = patternSetting.Value.Pattern;
        }

        _toastService.Show($"Reset ${setting.Path}");
    }

    [RelayCommand]
    public async Task ImportSettings()
    {
        var result = await FilePicker.Default.PickAsync();
        if (result is null) return;

        IsBusy = true;
        var fileSettingsJson = File.ReadAllText(result.FullPath);
        var fileSettings = SettingNodeManagerViewModel.FromJson(fileSettingsJson);
        var mappedFileSettings = _mapper.Map<ParentSettingViewModel>(fileSettings.Root);
        MergeSettings(mappedFileSettings, this.Root);
        IsBusy = false;

        Save();
        _toastService.Show($"Imported settings from {result.FullPath}");
    }

    public static void MergeSettings(SettingNode source, SettingNode dest)
    {
        if (source is ParentSetting parentSource && dest is ParentSetting parentDest)
        {
            foreach (var childSource in parentSource.Nodes)
            {
                // Not supporting duplicate names
                var childDest = parentDest.Nodes.FirstOrDefault(sn => sn.Name == childSource.Name);
                if (childDest is not null)
                {
                    MergeSettings(childSource, childDest);
                }
            }
        }
        else
        {
            switch (source.SettingType)
            {
                case SettingType.Boolean when dest.SettingType == SettingType.Boolean:
                    ((BooleanSettingViewModel)dest).Value = ((BooleanSettingViewModel)source).Value;
                    break;
                case SettingType.String when dest.SettingType == SettingType.String:
                    ((StringSettingViewModel)dest).Value = ((StringSettingViewModel)source).Value;
                    break;
                case SettingType.Option when dest.SettingType == SettingType.Option:
                    ((OptionSettingViewModel)dest).Value = ((OptionSettingViewModel)source).Value;
                    break;
                case SettingType.EnabledString when dest.SettingType == SettingType.EnabledString:
                    ((EnabledStringSettingViewModel)dest).Value = ((EnabledStringSettingViewModel)source).Value;
                    ((EnabledStringSettingViewModel)dest).IsEnabled = ((EnabledStringSettingViewModel)source).IsEnabled;
                    break;
                case SettingType.EnabledOption when dest.SettingType == SettingType.EnabledOption:
                    ((EnabledOptionSettingViewModel)dest).Value = ((EnabledOptionSettingViewModel)source).Value;
                    ((EnabledOptionSettingViewModel)dest).IsEnabled = ((EnabledOptionSettingViewModel)source).IsEnabled;
                    break;
                case SettingType.Integer when dest.SettingType == SettingType.Integer:
                    ((IntegerSettingViewModel)dest).Value = ((IntegerSettingViewModel)source).Value;
                    break;
                case SettingType.EnabledInteger when dest.SettingType == SettingType.EnabledInteger:
                    ((EnabledIntegerSettingViewModel)dest).IsEnabled = ((EnabledIntegerSettingViewModel)source).IsEnabled;
                    ((EnabledIntegerSettingViewModel)dest).Value = ((EnabledIntegerSettingViewModel)source).Value;
                    break;
                case SettingType.Double when dest.SettingType == SettingType.Double:
                    ((DoubleSettingViewModel)dest).Value = ((DoubleSettingViewModel)source).Value;
                    break;
                case SettingType.EnabledDouble when dest.SettingType == SettingType.EnabledDouble:
                    ((EnabledDoubleSettingViewModel)dest).IsEnabled = ((EnabledDoubleSettingViewModel)source).IsEnabled;
                    ((EnabledDoubleSettingViewModel)dest).Value = ((EnabledDoubleSettingViewModel)source).Value;
                    break;
                case SettingType.Pattern when dest.SettingType == SettingType.Pattern:
                case SettingType.EnabledPattern when dest.SettingType == SettingType.EnabledPattern:
                    var patternNodeCopy = PatternNodeManagerViewModel.CloneNode(((PatternSettingViewModel)source).Value);
                    ((PatternSettingViewModel)dest).Value = patternNodeCopy;
                    if (source.SettingType == SettingType.EnabledPattern)
                    {
                        ((EnabledPatternSettingViewModel)dest).IsEnabled = ((EnabledPatternSettingViewModel)source).IsEnabled;
                    }
                    break;
                case SettingType.TimeStamp when dest.SettingType == SettingType.TimeStamp:
                    ((TimestampSettingViewModel)dest).Value = ((TimestampSettingViewModel)source).Value;
                    break;
            }
        }
    }
}