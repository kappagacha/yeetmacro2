using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class DailyNodeViewModel: DailyNode
{
    [ObservableProperty]
    DailyJsonParentViewModel _jsonViewModel;
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

            if (Data is not null)
            {
                JsonViewModel = DailyJsonParentViewModel.Load(Data, this);
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
    public DailyNodeViewModel Node { get; set; }
    public JsonObject Parent { get; set; }
    [ObservableProperty]
    string _key;
}

public partial class DailyJsonParentViewModel : DailyJsonElementViewModel
{
    [ObservableProperty]
    ObservableCollection<DailyJsonElementViewModel> _children = new ObservableCollection<DailyJsonElementViewModel>();
    Dictionary<string, DailyJsonElementViewModel> _dict = new Dictionary<string, DailyJsonElementViewModel>();

    public bool IsLeaf => false;
    public bool IsExpanded => true;

    public static DailyJsonParentViewModel Load(JsonObject jsonObject, DailyNodeViewModel node)
    {
        var parent = new DailyJsonParentViewModel();

        foreach (var item in jsonObject)
        {
            var jsonKind = item.Value.GetValueKind();
            switch (jsonKind)
            {
                case JsonValueKind.Object:
                    var childObject = (JsonObject)item.Value;
                    var subParent = Load(childObject, node);
                    subParent.Key = item.Key;
                    subParent.Parent = jsonObject;
                    subParent.Node = node;
                    parent.Children.Add(subParent);
                    break;
                case JsonValueKind.Number:
                    var count = new DailyJsonCountViewModel()
                    {
                        Count = (int)item.Value.GetValue<double>(),
                        Parent = jsonObject,
                        Key = item.Key,
                        Node = node
                    };
                    parent.Children.Add(count);
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    var boolean = new DailyJsonBooleanViewModel()
                    {
                        IsChecked = item.Value.GetValue<bool>(),
                        Parent = jsonObject,
                        Key = item.Key,
                        Node = node
                    };
                    parent.Children.Add(boolean);
                    break;
            }

        }

        return parent;
    }

    public DailyJsonElementViewModel this[string key]
    {
        get
        {
            // Note: cache does not automatically invalidate
            if (!_dict.ContainsKey(key))
            {
                var child = Children.FirstOrDefault(n => n.Key == key);
                if (child is null) throw new ArgumentException($"Invalid key: {key}");
                _dict.Add(key, child);
            }

            return _dict[key];
        }
    }
}

public partial class DailyJsonBooleanViewModel: DailyJsonElementViewModel
{
    public bool IsLeaf => true;
    [ObservableProperty]
    bool _isChecked;

    partial void OnIsCheckedChanged(bool oldValue, bool newValue)
    {
        if (Parent is null || Key is null) return;

        Parent[Key] = newValue;
        WeakReferenceMessenger.Default.Send(Node);
    }
}

public partial class DailyJsonCountViewModel : DailyJsonElementViewModel
{
    public bool IsLeaf => true;
    [ObservableProperty]
    int _count;

    partial void OnCountChanged(int value)
    {
        if (Parent is null || Key is null) return;

        Parent[Key] = value;
        WeakReferenceMessenger.Default.Send(Node);
    }
}