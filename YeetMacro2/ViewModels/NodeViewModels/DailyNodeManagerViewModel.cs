using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;
public partial class DailyNodeManagerViewModel : NodeManagerViewModel<DailyNodeViewModel, DailyNode, DailyNode>
{
    DailyJsonParentViewModel _emptySubView = new DailyJsonParentViewModel();
    [ObservableProperty]
    bool _showJsonEditor;
    [ObservableProperty]
    DailyJsonParentViewModel _currentSubViewModel;
    public MacroSetViewModel MacroSet { get; set; }
    string _targetSubViewName;

    IRepository<DailyNode> _dailyRespository;
    public DailyNodeManagerViewModel(
        int rootNodeId,
        IRepository<DailyNode> dailyRespository,
        INodeService<DailyNode, DailyNode> nodeService,
        IInputService inputService,
        IToastService toastService)
        : base(rootNodeId, nodeService, inputService, toastService)
    {
        _dailyRespository = dailyRespository;
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
            existingDaily = new DailyNodeViewModel()
            {
                Date = targetDate,
                Data = GetJsonFromTemplate()
            };
            this.AddNode(existingDaily);
        }
        SelectedNode = existingDaily;
        var targetJsonViewModel = ((DailyNodeViewModel)existingDaily).JsonViewModel;
        CurrentSubViewModel = ((DailyJsonParentViewModel)targetJsonViewModel.Children.FirstOrDefault(c => c.Key == _targetSubViewName)) ?? _emptySubView;
    }

    [RelayCommand]
    public void SaveDaily(object[] values)
    {
        if (values[0] is DailyNodeViewModel daily && values[1] is string stringValue)
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

    public void SaveDaily(DailyNodeViewModel daily)
    {
        _dailyRespository.Update(daily);
        _dailyRespository.Save();
    }

    [RelayCommand]
    public void Increment(DailyJsonCountViewModel dailyJsonCount)
    {
        dailyJsonCount.Count++;
    }

    [RelayCommand]
    public void Decrement(DailyJsonCountViewModel dailyJsonCount)
    {
        dailyJsonCount.Count--;
    }

    protected override Task AddNode()
    {
        var newNode = new DailyNodeViewModel()
        {
            Date = ResolveTargetDate(0),
            Data = GetJsonFromTemplate()
        };
        base.AddNode(newNode);
        return Task.CompletedTask;
    }

    public DailyJsonParentViewModel GetDaily(int offset = 0)
    {
        var targetDate = ResolveTargetDate(offset);
        var existingDaily = Root.Nodes.FirstOrDefault(dn => dn.Date == targetDate);
        if (existingDaily is not null) return ((DailyNodeViewModel)existingDaily).JsonViewModel;

        var newDaily = new DailyNodeViewModel()
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