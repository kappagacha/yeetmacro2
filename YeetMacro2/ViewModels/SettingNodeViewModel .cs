using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class SettingNodeViewModel : NodeViewModel<ParentSetting, SettingNode>
{
    IRepository<SettingNode> _settingRepository;
    public SettingNodeViewModel(
        int rootNodeId,
        IRepository<SettingNode> settingRepository,
        INodeService<ParentSetting, SettingNode> nodeService,
        IInputService inputService,
        IToastService toastService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {
        _settingRepository = settingRepository;
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
}