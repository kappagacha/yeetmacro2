using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.RegularExpressions;
using YeetMacro2.Data.Models;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class ScriptNodeViewModel : ScriptNode
{
    public bool IsLeaf => true;
    public override ICollection<ScriptNode> Nodes
    {
        get => base.Nodes;
        set {
            base.Nodes = new NodeObservableCollection<ScriptNodeViewModel, ScriptNode>(value);
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

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Description));
        }
    }

    public override string Description
    {
        get
        { 
            if (Text is null) return String.Empty;

            var lines = Text.Split('\n');
            var description = "";
            foreach (var line in lines)
            {
                if (line.StartsWith("//") && line.Contains("raw-script"))
                {
                    continue;
                }
                else if (line.StartsWith("//"))
                {
                    description += Regex.Replace(line, "^//\\s+", "") + "\n";
                }
                else
                {
                    break;
                }
            }
            return description.TrimEnd();
        }
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