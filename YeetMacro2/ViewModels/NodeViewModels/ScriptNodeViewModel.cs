using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class ScriptNodeViewModel : ScriptNode
{
    NodeObservableCollection<ScriptNodeViewModel, ScriptNode> _nodes;

    public override ICollection<ScriptNode> Nodes
    {
        get => _nodes;
        set {
            _nodes = new NodeObservableCollection<ScriptNodeViewModel, ScriptNode>(value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLeaf));
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

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            OnPropertyChanged();
        }
    }

    public bool IsLeaf
    {
        get => true;
    }

    public NodeObservableCollection<ScriptNodeViewModel, ScriptNode> Children
    {
        get => _nodes;
    }

    public ScriptNodeViewModel()
    {
        _nodes = new NodeObservableCollection<ScriptNodeViewModel, ScriptNode>();
    }
}