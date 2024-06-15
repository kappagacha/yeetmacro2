using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.RegularExpressions;
using YeetMacro2.Data.Models;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class ScriptNodeViewModel : ScriptNode
{
    public bool IsLeaf { get; set; } = true;
    public override IList<ScriptNode> Nodes
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

    public override bool IsHidden
    {
        get => base.IsHidden;
        set
        {
            base.IsHidden = value;
            OnPropertyChanged();
        }
    }

    public override bool IsFavorite
    {
        get => base.IsFavorite;
        set
        {
            base.IsFavorite = value;
            OnPropertyChanged();
        }
    }

    public override int Position
    {
        get => base.Position;
        set
        {
            base.Position = value;
            OnPropertyChanged();
        }
    }

    public override string Description
    {
        get
        { 
            if (Text is null) return String.Empty;

            var lines = Text.Split(new char[] { '\n', '\r' });
            var description = "";
            foreach (var line in lines)
            {
                if (line.StartsWith("//") && (line.Contains("raw-script") || line.Contains("@isFavorite") || line.Contains("@position")))
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