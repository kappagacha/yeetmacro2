using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;
public partial class WeeklyNodeManagerViewModel : NodeManagerViewModel<TodoViewModel, TodoNode, TodoNode>
{
    [ObservableProperty]
    bool _showJsonEditor;
    public MacroSetViewModel MacroSet { get; set; }

    IRepository<TodoNode> _todoRepository;
    public WeeklyNodeManagerViewModel(
        int rootNodeId,
        IRepository<TodoNode> todoRepository,
        INodeService<TodoNode, TodoNode> nodeService,
        IInputService inputService,
        IToastService toastService)
        : base(rootNodeId, nodeService, inputService, toastService)
    {
        _todoRepository = todoRepository;
        IsList = true;
    }

    [RelayCommand]
    public void SaveWeekly(object[] values)
    {
        if (values[0] is TodoViewModel weekly && values[1] is string stringValue)
        {
            try
            {
                weekly.Data = (JsonObject)JsonObject.Parse(stringValue);
                SaveWeekly(weekly);
            }
            catch (Exception ex)
            {
                _toastService.Show($"Error saving weekly: {ex.Message}");
            }
        }
    }

    public void SaveWeekly(TodoViewModel weekly)
    {
        _todoRepository.Update(weekly);
        _todoRepository.Save();
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

    public TodoJsonParentViewModel GetWeelky(int offset = 0)
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

    private DateOnly ResolveTargetDate(int offset)
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

    private JsonObject GetJsonFromTemplate()
    {
        if (string.IsNullOrWhiteSpace(MacroSet.WeeklyTemplate)) return new JsonObject();

        return (JsonObject)JsonObject.Parse(MacroSet.WeeklyTemplate);
    }
}