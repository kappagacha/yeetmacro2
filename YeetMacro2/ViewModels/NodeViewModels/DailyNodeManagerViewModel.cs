using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;
public partial class DailyNodeManagerViewModel : NodeManagerViewModel<TodoViewModel, TodoNode, TodoNode>
{
    TodoJsonParentViewModel _emptySubView = new TodoJsonParentViewModel();
    [ObservableProperty]
    bool _showJsonEditor;
    [ObservableProperty]
    TodoJsonParentViewModel _currentSubViewModel;
    public MacroSetViewModel MacroSet { get; set; }
    string _targetSubViewName;

    IRepository<TodoNode> _todoRepository;
    public DailyNodeManagerViewModel(
        int rootNodeId,
        IRepository<TodoNode> todoRepository,
        INodeService<TodoNode, TodoNode> nodeService,
        IInputService inputService,
        IToastService toastService)
        : base(rootNodeId, nodeService, inputService, toastService)
    {
        _todoRepository = todoRepository;
        IsList = true;
        CurrentSubViewModel = _emptySubView;
    }

    protected override void CustomInit()
    {
        // This invokes ScriptNodeManagerViewModel SelectedNode PropertyChangedMessage
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<string>(this, nameof(CustomInit), null, null), nameof(DailyNodeManagerViewModel));
    }

    public async Task OnScriptNodeSelected(ScriptNode scriptNode)
    {
        if (Root is null) await this.WaitForInitialization();
        _targetSubViewName = scriptNode?.Name;
        ResolveCurrentSubViewModel();
    }

    public void ResolveSubViewModelDate()
    {
        var targetDate = ResolveTargetDate(0);
        if (SelectedNode is not null && SelectedNode.Date != targetDate)
        {
            ResolveCurrentSubViewModel();
        }
    }

    private void ResolveCurrentSubViewModel()
    {
        if (string.IsNullOrEmpty(_targetSubViewName))
        {
            CurrentSubViewModel = _emptySubView;
            return;
        };
        var targetDate = ResolveTargetDate(0);
        var existingDaily = Root.Nodes.FirstOrDefault(dn => dn.Date == targetDate);
        if (existingDaily is null)
        {
            existingDaily = new TodoViewModel()
            {
                Date = targetDate,
                Data = GetJsonFromTemplate()
            };
            this.AddNode(existingDaily);
        }
        SelectedNode = existingDaily;
        var targetJsonViewModel = ((TodoViewModel)existingDaily).JsonViewModel;
        CurrentSubViewModel = ((TodoJsonParentViewModel)targetJsonViewModel.Children.FirstOrDefault(c => c.Key == _targetSubViewName)) ?? _emptySubView;
    }

    [RelayCommand]
    public void SaveDaily(object[] values)
    {
        if (values[0] is TodoViewModel daily && values[1] is string stringValue)
        {
            try
            {
                daily.Data = (JsonObject)JsonObject.Parse(stringValue);
                ResolveCurrentSubViewModel();
                SaveDaily(daily);
            }
            catch (Exception ex)
            {
                _toastService.Show($"Error saving daily: {ex.Message}");
            }
        }
    }

    public void SaveDaily(TodoViewModel daily)
    {
        _todoRepository.Update(daily);
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

    private DateOnly ResolveTargetDate(int offset)
    {
        var utcNow = DateTime.UtcNow;
        var targetDate = DateOnly.FromDateTime(utcNow).AddDays(offset);
        if (utcNow.Hour < MacroSet.DailyResetUtcHour)
        {
            targetDate = targetDate.AddDays(-1);
        }
        return targetDate;
    }

    private JsonObject GetJsonFromTemplate()
    {
        if (string.IsNullOrWhiteSpace(MacroSet.DailyTemplate)) return new JsonObject();

        return (JsonObject)JsonObject.Parse(MacroSet.DailyTemplate);
    }
}