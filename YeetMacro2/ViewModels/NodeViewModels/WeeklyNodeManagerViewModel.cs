using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;
public partial class WeeklyNodeManagerViewModel : TodoNodeManagerViewModel
{
    public WeeklyNodeManagerViewModel(
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
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<string>(this, nameof(CustomInit), null, null), nameof(WeeklyNodeManagerViewModel));
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

    public TodoJsonParentViewModel GetCurrentWeekly(int offset = 0)
    {
        var targetDate = ResolveTargetDate(offset);
        var existingWeekly = Root.Nodes.FirstOrDefault(dn => dn.Date == targetDate);
        if (existingWeekly is not null) return ((TodoViewModel)existingWeekly).JsonViewModel;

        var newWeekly = new TodoViewModel()
        {
            Date = targetDate,
            Data = GetJsonFromTemplate()
        };
        this.AddNode(newWeekly);

        return newWeekly.JsonViewModel;
    }

    public override DateOnly ResolveTargetDate(int offset)
    {
        var utcNow = DateTime.UtcNow;
        var targetDate = DateOnly.FromDateTime(utcNow).AddDays(offset);
        if (utcNow.Hour < MacroSet.DailyResetUtcHour)
        {
            targetDate = targetDate.AddDays(-1);
        }

        while (targetDate.DayOfWeek != MacroSet.WeeklyStartDay)
        {
            targetDate = targetDate.AddDays(-1);
        }

        return targetDate;
    }

    public DayOfWeek GetDayOfWeek()
    {
        var utcNow = DateTime.UtcNow;
        var targetDate = DateOnly.FromDateTime(utcNow);
        if (utcNow.Hour < MacroSet.DailyResetUtcHour)
        {
            targetDate = targetDate.AddDays(-1);
        }

        return targetDate.DayOfWeek;
    }
}