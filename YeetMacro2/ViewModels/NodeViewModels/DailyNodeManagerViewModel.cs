using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;
public class DailyNodeManagerViewModel : NodeManagerViewModel<DailyNodeViewModel, DailyNode, DailyNode>
{
    public DailyNodeManagerViewModel(
        int rootNodeId,
        INodeService<DailyNode, DailyNode> nodeService,
        IInputService inputService,
        IToastService toastService)
        : base(rootNodeId, nodeService, inputService, toastService)
    {
        IsList = true;
    }
}

[ObservableObject]
public partial class DailyNodeViewModel: DailyNode
{
    public bool IsLeaf => true;
    public override ICollection<DailyNode> Nodes
    {
        get => base.Nodes;
        set
        {
            base.Nodes = new NodeObservableCollection<DailyNodeViewModel, DailyNode>(value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLeaf));
        }
    }

    public override string Name
    {
        get => base.Name;
        set
        {
            base.Name = value;
            OnPropertyChanged();
        }
    }

    public override bool IsSelected
    {
        get => base.IsSelected;
        set
        {
            base.IsSelected = value;
            OnPropertyChanged();
        }
    }

    public override bool IsExpanded
    {
        get => base.IsExpanded;
        set
        {
            base.IsExpanded = value;
            OnPropertyChanged();
        }
    }

    public override DateOnly Date
    {
        get => base.Date;
        set
        {
            base.Date = value;
            OnPropertyChanged();
        }
    }

    public override JsonObject Data
    {
        get => base.Data;
        set
        {
            base.Data = value;
            OnPropertyChanged();
        }
    }

    public ICollection<DailyNode> Children
    {
        get => base.Nodes;
    }

    public DailyNodeViewModel()
    {
        base.Nodes = new NodeObservableCollection<DailyNodeViewModel, DailyNode>();
    }
}