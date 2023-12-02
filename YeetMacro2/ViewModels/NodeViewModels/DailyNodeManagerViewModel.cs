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

        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<ScriptNode>, string>(this, nameof(ScriptNodeManagerViewModel), async (r, propertyChangedMessage) =>
        {
            if (Root is null) await this.WaitForInitialization();
            if (propertyChangedMessage.PropertyName != nameof(ScriptNodeManagerViewModel.SelectedNode)) return;
            if (propertyChangedMessage.NewValue is null)
            {
                _targetSubViewName = null;
                return;
            }
            _targetSubViewName = propertyChangedMessage.NewValue.Name;
            ResolveCurrentSubViewModel();
        });
    }

    protected override void CustomInit()
    {
        // This invokes ScriptNodeManagerViewModel SelectedNode PropertyChangedMessage
        WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<string>(this, nameof(CustomInit), null, null), nameof(DailyNodeManagerViewModel));
    }

    private void ResolveCurrentSubViewModel()
    {
        if (string.IsNullOrEmpty(_targetSubViewName)) return;
        var targetDate = ResolveTargetDate(0);
        var existingDaily = Root.Nodes.FirstOrDefault(dn => dn.Date == targetDate);
        if (existingDaily is null)
        {
            existingDaily = new DailyNodeViewModel()
            {
                Date = targetDate,
                Data = (JsonObject)JsonObject.Parse(MacroSet.DailyTemplate)
            };
            this.AddNode(existingDaily);
        }
        SelectedNode = existingDaily;
        var targetJsonViewModel = ((DailyNodeViewModel)existingDaily).JsonViewModel;
        CurrentSubViewModel = (DailyJsonParentViewModel)targetJsonViewModel.Root.Children.FirstOrDefault(c => c.Key == _targetSubViewName);
    }

    [RelayCommand]
    public void SaveDaily(object[] values)
    {
        if (values[0] is DailyNodeViewModel daily)
        {
            try
            {
                var jsonString = string.Empty;
                if (values[1] is string stringValue)
                {
                    jsonString = stringValue;
                    daily.Data = (JsonObject)JsonObject.Parse(jsonString);
                    daily.JsonViewModel = new DailyJsonViewModel(daily.Data);
                }
                else if (values[1] is DailyJsonViewModel jsonViewModel)
                {
                    daily.Data = (JsonObject)JsonObject.Parse(jsonViewModel.Root.JsonString);
                }

                _dailyRespository.Update(daily);
                _dailyRespository.Save();

                ResolveCurrentSubViewModel();
            }
            catch (Exception ex)
            {
                _toastService.Show($"Error saving daily: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    public void Increment(object dailyValue)
    {
        if (dailyValue is DailyJsonCountViewModel dailyJsonCount)
        {
            dailyJsonCount.Count++;
        }

        var dailyViewModel = (DailyNodeViewModel)SelectedNode;
        SaveDaily(new object[] { dailyViewModel, dailyViewModel.JsonViewModel });
    }

    [RelayCommand]
    public void Decrement(object dailyValue)
    {
        if (dailyValue is DailyJsonCountViewModel dailyJsonCount)
        {
            dailyJsonCount.Count--;
        }

        var dailyViewModel = (DailyNodeViewModel)SelectedNode;
        SaveDaily(new object[] { dailyViewModel, dailyViewModel.JsonViewModel });
    }

    [RelayCommand]
    public void ToggleShowJsonEditor()
    {
        ShowJsonEditor = !ShowJsonEditor;
    }

    protected override Task AddNode()
    {
        var newNode = new DailyNodeViewModel()
        {
            Date = ResolveTargetDate(0),
            Data = (JsonObject)JsonObject.Parse(MacroSet.DailyTemplate)
        };
        base.AddNode(newNode);
        return Task.CompletedTask;
    }

    public JsonObject GetDaily(int offset = 0)
    {
        var targetDate = ResolveTargetDate(offset);
        var existingDaily = Root.Nodes.FirstOrDefault(dn => dn.Date == targetDate);
        if (existingDaily is not null) return existingDaily.Data;

        var newDaily = new DailyNodeViewModel()
        {
            Date = targetDate,
            Data = (JsonObject)JsonObject.Parse(MacroSet.DailyTemplate)
        };
        this.AddNode(newDaily);

        return newDaily.Data;
    }

    public void UpdateDaily(JsonObject dailyJson, int offset = 0)
    {
        var targetDate = ResolveTargetDate(offset);
        var daily = Root.Nodes.First(dn => dn.Date == targetDate);
        SaveDaily(new object[] { (DailyNodeViewModel)daily, dailyJson.ToJsonString() });
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
}