using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;
public partial class DailyNodeManagerViewModel : TodoNodeManagerViewModel
{
    public DailyNodeManagerViewModel(
        int rootNodeId,
        IRepository<TodoNode> todoRepository,
        INodeService<TodoNode, TodoNode> nodeService,
        IInputService inputService,
        IToastService toastService)
        : base(rootNodeId, todoRepository, nodeService, inputService, toastService)
    {
    }

    protected override void CustomInit()
    {
        // This invokes ScriptNodeManagerViewModel SelectedNode PropertyChangedMessage
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<string>(this, nameof(CustomInit), null, null), nameof(DailyNodeManagerViewModel));
    }

    protected override Task AddNode()
    {
        var newNode = new TodoViewModel()
        {
            Date = ResolveTargetDate(0),
            Data = GetJsonFromTemplate()
        };
        base.AddNode(newNode);
        return Task.CompletedTask;
    }

    public TodoJsonParentViewModel GetCurrentDaily(int offset = 0)
    {
        var targetDate = ResolveTargetDate(offset);
        var existingDaily = Root.Nodes.FirstOrDefault(dn => dn.Date == targetDate);
        if (existingDaily is not null) return ((TodoViewModel)existingDaily).JsonViewModel;

        var newDaily = new TodoViewModel()
        {
            Date = targetDate,
            Data = GetJsonFromTemplate()
        };
        this.AddNode(newDaily);

        return newDaily.JsonViewModel;
    }

    public override DateOnly ResolveTargetDate(int offset)
    {
        var utcNow = DateTime.UtcNow;
        var targetDate = DateOnly.FromDateTime(utcNow).AddDays(offset);
        if (utcNow.Hour < MacroSet.DailyResetUtcHour)
        {
            targetDate = targetDate.AddDays(-1);
        }
        return targetDate;
    }
}