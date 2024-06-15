using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class TodoViewModel: TodoNode
{
    [ObservableProperty]
    TodoJsonParentViewModel _jsonViewModel;
    public bool IsLeaf { get; set; } = true;
    public override IList<TodoNode> Nodes
    {
        get => base.Nodes;
        set
        {
            base.Nodes = new NodeObservableCollection<TodoViewModel, TodoNode>(value, (a, b) => b.Date > a.Date ? 1: 0);
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
                JsonViewModel = TodoJsonParentViewModel.Load(Data, this);
            }
        }
    }

    public ICollection<TodoNode> Children
    {
        get => base.Nodes;
    }

    public TodoViewModel()
    {
        base.Nodes = new NodeObservableCollection<TodoViewModel, TodoNode>((a, b) => b.Date > a.Date ? 1 : 0);
    }
}

public abstract partial class TodoJsonElementViewModel : ObservableObject
{
    public virtual bool IsLeaf { get; set; } = true;
    [ObservableProperty]
    bool _isExpanded = false;
    //public virtual bool IsExpanded { get; set; } = false;
    public TodoViewModel Node { get; set; }
    public JsonObject Parent { get; set; }
    [ObservableProperty]
    string _key;
}

public partial class TodoJsonParentViewModel : TodoJsonElementViewModel
{
    [ObservableProperty]
    ObservableCollection<TodoJsonElementViewModel> _children = new ObservableCollection<TodoJsonElementViewModel>();
    Dictionary<string, TodoJsonElementViewModel> _dict = new Dictionary<string, TodoJsonElementViewModel>();

    public override bool IsLeaf { get; set; } = false;

    public static TodoJsonParentViewModel Load(JsonObject jsonObject, TodoViewModel node)
    {
        var parent = new TodoJsonParentViewModel();

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
                    var count = new TodoJsonCountViewModel()
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
                    var boolean = new TodoJsonBooleanViewModel()
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

    public TodoJsonElementViewModel this[string key]
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

public partial class TodoJsonBooleanViewModel: TodoJsonElementViewModel
{
    [ObservableProperty]
    bool _isChecked;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    partial void OnIsCheckedChanged(bool oldValue, bool newValue)
    {
        if (Parent is null || Key is null) return;

        Parent[Key] = newValue;
        WeakReferenceMessenger.Default.Send(Node);
    }
}

public partial class TodoJsonCountViewModel : TodoJsonElementViewModel
{
    [ObservableProperty]
    int _count;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    partial void OnCountChanged(int value)
    {
        if (Parent is null || Key is null) return;

        Parent[Key] = value;
        WeakReferenceMessenger.Default.Send(Node);
    }
}