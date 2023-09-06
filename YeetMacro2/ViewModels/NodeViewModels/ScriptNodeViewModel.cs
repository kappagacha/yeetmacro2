using CommunityToolkit.Mvvm.ComponentModel;
using YeetMacro2.Data.Models;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class ScriptNodeViewModel : ScriptNode
{

    public override ICollection<ScriptNode> Nodes
    {
        get => base.Nodes;
        set {
            base.Nodes = new NodeObservableCollection<ScriptNodeViewModel, ScriptNode>(value);
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

    public ICollection<ScriptNode> Children
    {
        get => base.Nodes;
    }

    public ScriptNodeViewModel()
    {
        base.Nodes = new NodeObservableCollection<ScriptNodeViewModel, ScriptNode>();
    }
}