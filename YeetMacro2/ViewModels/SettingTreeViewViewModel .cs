using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

public partial class SettingTreeViewViewModel : NodeViewModel<ParentSetting, Setting>
{
    public SettingTreeViewViewModel(
        int rootNodeId,
        INodeService<ParentSetting, Setting> nodeService,
        IInputService inputService,
        IToastService toastService)
            : base(rootNodeId, nodeService, inputService, toastService)
    {

    }
}