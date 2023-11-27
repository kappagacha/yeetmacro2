using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;
public partial class DailyNodeManagerViewModel : NodeManagerViewModel<DailyNodeViewModel, DailyNode, DailyNode>
{
    [ObservableProperty]
    bool _showJsonEditor;

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
}