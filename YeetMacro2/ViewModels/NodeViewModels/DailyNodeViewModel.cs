using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class DailyNodeViewModel: DailyNode
{
    [ObservableProperty]
    DailyJsonViewModel _jsonViewModel;
    public bool IsLeaf => true;
    public override ICollection<DailyNode> Nodes
    {
        get => base.Nodes;
        set
        {
            base.Nodes = new SortedNodeObservableCollection<DailyNodeViewModel, DailyNode>(value, (a, b) => b.Date > a.Date ? 1: 0);
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
            Name = value.ToString();
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
            OnPropertyChanged(nameof(DataText));

            if (Data is not null && JsonViewModel is null)
            {
                JsonViewModel = new DailyJsonViewModel(Data);
            }
        }
    }

    public ICollection<DailyNode> Children
    {
        get => base.Nodes;
    }

    public DailyNodeViewModel()
    {
        base.Nodes = new SortedNodeObservableCollection<DailyNodeViewModel, DailyNode>((a, b) => b.Date > a.Date ? 1 : 0);
    }
}

public abstract partial class DailyJsonElementViewModel : ObservableObject
{
    [ObservableProperty]
    string _key;
    public abstract string JsonString
    {
        get;
    }
}

public partial class DailyJsonParentViewModel : DailyJsonElementViewModel
{
    [ObservableProperty]
    ObservableCollection<DailyJsonElementViewModel> _children = new ObservableCollection<DailyJsonElementViewModel>();
    public bool IsLeaf => false;
    public bool IsExpanded => true;
    public override string JsonString 
    {  
        get 
        {
            var sb = new StringBuilder();
            sb.Append("{");
            foreach (var child in Children)
            {
                sb.Append($"\"{child.Key}\": {child.JsonString},");
            }
            sb.Length--;        //remove final comma
            sb.Append("}");
            return sb.ToString();
        } 
    }

    public static DailyJsonParentViewModel Load(JsonObject jsonObject)
    {
        var parent = new DailyJsonParentViewModel();

        foreach (var item in jsonObject)
        {
            var jsonKind = item.Value.GetValueKind();
            switch (jsonKind)
            {
                case JsonValueKind.Object:
                    var childObject = (JsonObject)item.Value;
                    var subParent = Load(childObject);
                    subParent.Key = item.Key;
                    parent.Children.Add(subParent);
                    break;
                case JsonValueKind.Number:
                    parent.Children.Add(new DailyJsonCountViewModel() { 
                        Count = (int)item.Value.GetValue<double>(), 
                        Key = item.Key
                    });
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    parent.Children.Add(new DailyJsonBooleanViewModel()
                    {
                        IsChecked = item.Value.GetValue<bool>(),
                        Key = item.Key
                    });
                    break;
            }

        }

        return parent;
    }
}

public partial class DailyJsonBooleanViewModel: DailyJsonElementViewModel
{
    public bool IsLeaf => true;
    [ObservableProperty]
    bool _isChecked;

    public override string JsonString => IsChecked.ToString().ToLower();
}

public partial class DailyJsonCountViewModel : DailyJsonElementViewModel
{
    public bool IsLeaf => true;
    [ObservableProperty]
    int _count;

    public override string JsonString => Count.ToString();
}

public partial class DailyJsonViewModel: ObservableObject
{
    [ObservableProperty]
    DailyJsonParentViewModel _root;

    public DailyJsonViewModel(JsonObject jsonObject)
    {
        Root = DailyJsonParentViewModel.Load(jsonObject);
    }
}