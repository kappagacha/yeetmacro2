﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class TodoViewModel: TodoNode
{
    [ObservableProperty]
    TodoJsonParentViewModel _jsonViewModel;
    public override IList<TodoNode> Nodes
    {
        get => base.Nodes;
        set
        {
            base.Nodes = new NodeObservableCollection<TodoViewModel, TodoNode>(value, (a, b) => b.Date > a.Date ? 1: 0);
            OnPropertyChanged();
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

    public TodoViewModel()
    {
        base.Nodes = new NodeObservableCollection<TodoViewModel, TodoNode>((a, b) => b.Date > a.Date ? 1 : 0);
    }

    public void OnDataTextPropertyChanged()
    {
        OnPropertyChanged(nameof(DataText));
    }
}

public abstract partial class TodoJsonElementViewModel : ObservableObject, ISortable
{
    [ObservableProperty]
    bool _isExpanded = false, _isSelected = false;
    public TodoViewModel Node { get; set; }
    public JsonObject Parent { get; set; }
    public int Position { get; set; }

    [ObservableProperty]
    string _key;
    public virtual bool IsParent => false;
}

public partial class TodoJsonParentViewModel : TodoJsonElementViewModel
{
    [ObservableProperty]
    NodeObservableCollection<TodoJsonElementViewModel, TodoJsonElementViewModel> _children = [];
    readonly Dictionary<string, TodoJsonElementViewModel> _dict = [];

    public override bool IsParent => true;
    public JsonObject JsonObject { get; set; }

    public static TodoJsonParentViewModel Load(JsonObject jsonObject, TodoViewModel node)
    {
        var parent = new TodoJsonParentViewModel
        {
            JsonObject = jsonObject
        };

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

    public static JsonObject Export(TodoJsonParentViewModel todoJsonParentViewModel)
    {
        var jsonObject = new JsonObject();
        foreach (var child in todoJsonParentViewModel.Children)
        {
            if (child is TodoJsonParentViewModel parentViewModel)
            {
                jsonObject[child.Key] = Export(parentViewModel);
            }
            else if (child is TodoJsonCountViewModel countViewModel)
            {
                jsonObject[child.Key] = (double)countViewModel.Count;
            }
            else if (child is TodoJsonBooleanViewModel boolViewModel) {
                jsonObject[child.Key] = boolViewModel.IsChecked;
            }
        }
        return jsonObject;
    }

    public TodoJsonElementViewModel this[string key]
    {
        get
        {
            // Note: cache does not automatically invalidate
            if (!_dict.ContainsKey(key))
            {
                var child = Children.FirstOrDefault(n => n.Key == key) ?? throw new ArgumentException($"Invalid key: {key}");
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

    partial void OnIsCheckedChanged(bool oldValue, bool newValue)
    {
        if (Parent is null || Key is null) return;

        Parent[Key] = newValue;
        Node.OnDataTextPropertyChanged();
        WeakReferenceMessenger.Default.Send(Node);
    }
}

public partial class TodoJsonCountViewModel : TodoJsonElementViewModel
{
    [ObservableProperty]
    int _count;

    partial void OnCountChanged(int value)
    {
        if (Parent is null || Key is null) return;

        Parent[Key] = value;
        Node.OnDataTextPropertyChanged();
        WeakReferenceMessenger.Default.Send(Node);
    }
}