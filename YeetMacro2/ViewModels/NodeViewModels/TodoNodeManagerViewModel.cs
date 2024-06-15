using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;
public partial class TodoNodeManagerViewModel : NodeManagerViewModel<TodoViewModel, TodoNode, TodoNode>
{
    TodoJsonParentViewModel _emptySubView = new TodoJsonParentViewModel();
    [ObservableProperty]
    bool _showJsonEditor, _showTemplate;
    [ObservableProperty]
    TodoJsonParentViewModel _currentSubViewModel;
    [ObservableProperty]
    TodoJsonElementViewModel _selectedJsonElement;
    public MacroSetViewModel MacroSet { get; set; }
    string _targetSubViewName;

    IRepository<TodoNode> _todoRepository;
    public TodoNodeManagerViewModel(
        int rootNodeId,
        IRepository<TodoNode> todoRepository,
        INodeService<TodoNode, TodoNode> nodeService,
        IInputService inputService,
        IToastService toastService)
        : base(rootNodeId, nodeService, inputService, toastService)
    {
        _todoRepository = todoRepository;
        IsList = true;
        CurrentSubViewModel = _emptySubView;
    }

    public async Task OnScriptNodeSelected(ScriptNode scriptNode)
    {
        if (Root is null) await this.WaitForInitialization();
        _targetSubViewName = scriptNode?.Name;
        ResolveCurrentSubViewModel();
    }

    public void ResolveSubViewModelDate()
    {
        var targetDate = ResolveTargetDate(0);
        if (SelectedNode is not null && SelectedNode.Date != targetDate)
        {
            ResolveCurrentSubViewModel();
        }
    }

    private void ResolveCurrentSubViewModel()
    {
        if (string.IsNullOrEmpty(_targetSubViewName))
        {
            CurrentSubViewModel = _emptySubView;
            return;
        };
        var targetDate = ResolveTargetDate(0);
        var existingDaily = Root.Nodes.FirstOrDefault(dn => dn.Date == targetDate);
        if (existingDaily is null)
        {
            existingDaily = new TodoViewModel()
            {
                Date = targetDate,
                Data = GetJsonFromTemplate()
            };
            this.AddNode(existingDaily);
        }
        SelectedNode = existingDaily;
        var targetJsonViewModel = ((TodoViewModel)existingDaily).JsonViewModel;
        CurrentSubViewModel = ((TodoJsonParentViewModel)targetJsonViewModel.Children.FirstOrDefault(c => c.Key == _targetSubViewName)) ?? _emptySubView;
    }

    [RelayCommand]
    public void SaveTodo(object[] values)
    {
        if (values[0] is TodoViewModel daily && values[1] is string stringValue)
        {
            try
            {
                daily.Data = (JsonObject)JsonObject.Parse(stringValue);
                ResolveCurrentSubViewModel();
                SaveTodo(daily);
            }
            catch (Exception ex)
            {
                _toastService.Show($"Error saving daily: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    public void SelectJsonElement(TodoJsonElementViewModel jsonElementViewModel)
    {
        if (jsonElementViewModel == SelectedJsonElement)
        {
            jsonElementViewModel.IsSelected = !jsonElementViewModel.IsSelected;
        }
        else
        {
            if (SelectedJsonElement is not null)
            {
                SelectedJsonElement.IsSelected = false;
            }

            if (jsonElementViewModel is not null) jsonElementViewModel.IsSelected = true;
            SelectedJsonElement = jsonElementViewModel;
        }

        if (SelectedJsonElement is not null && !SelectedJsonElement.IsSelected)
        {
            SelectedJsonElement = null;
        }
    }

    [RelayCommand]
    public async Task AddJsonElement()
    {
        var todo = (TodoViewModel)SelectedNode;
        var parent = (SelectedJsonElement?.IsParent ?? false) ? (TodoJsonParentViewModel)SelectedJsonElement: todo.JsonViewModel;
        var selectedTypeName = await _inputService.SelectOption("Please select todo element type", "parent", "bool", "count");
        if (string.IsNullOrEmpty(selectedTypeName) || selectedTypeName == "cancel") return;
        var elementName = await _inputService.PromptInput($"Enter name for element");
        TodoJsonElementViewModel elementToAdd = null;

        switch (selectedTypeName)
        {
            case "parent":
                var jsonObject = new JsonObject();
                elementToAdd = new TodoJsonParentViewModel() { Key = elementName, Parent = parent.JsonObject, Node = todo, JsonObject = jsonObject };
                parent.JsonObject[elementName] = jsonObject;
                break;
            case "bool":
                elementToAdd = new TodoJsonBooleanViewModel() { Key = elementName, Parent = parent.JsonObject, Node = todo };
                parent.JsonObject[elementName] = false;
                break;
            case "count":
                elementToAdd = new TodoJsonCountViewModel() { Key = elementName, Parent = parent.JsonObject, Node = todo };
                parent.JsonObject[elementName] = 0;
                break;
            default:
                throw new NotImplementedException($"Unimplemented todo element type: ${selectedTypeName}");
        }

        parent.Children.Add(elementToAdd);
        todo.OnDataTextPropertyChanged();
        WeakReferenceMessenger.Default.Send(todo);
    }

    [RelayCommand]
    public void CollapseAllElements()
    {
        var todo = (TodoViewModel)SelectedNode;
        Traverse(todo.JsonViewModel, (element) => element.IsExpanded = false);
    }

    [RelayCommand]
    public void ExpandAllElements()
    {
        var todo = (TodoViewModel)SelectedNode;
        Traverse(todo.JsonViewModel, (element) => element.IsExpanded = true);
    }

    [RelayCommand]
    public void MoveElementTop(TodoJsonElementViewModel jsonElementViewModel)
    {
        var todo = (TodoViewModel)SelectedNode;
        var parent = FindJsonElementParent(todo.JsonViewModel, jsonElementViewModel);
        for (int i = 0; i < parent.Children.Count; i++)
        {
            parent.Children[i].Position = i;
        }

        if (jsonElementViewModel.Position == 0) return;

        foreach (var child in parent.Children)
        {
            if (child.Position < jsonElementViewModel.Position)
            {
                child.Position++;
            }
        }
        jsonElementViewModel.Position = 0;
        parent.Children.OnCollectionReset();

        todo.Data = TodoJsonParentViewModel.Export(todo.JsonViewModel);
        WeakReferenceMessenger.Default.Send(todo);
    }

    [RelayCommand]
    public void MoveElementUp(TodoJsonElementViewModel jsonElementViewModel)
    {
        var todo = (TodoViewModel)SelectedNode;
        var parent = FindJsonElementParent(todo.JsonViewModel, jsonElementViewModel);
        for (int i = 0; i < parent.Children.Count; i++)
        {
            parent.Children[i].Position = i;
        }

        if (jsonElementViewModel.Position == 0) return;

        var nodeAbove = parent.Children.First(n => n.Position == jsonElementViewModel.Position - 1);
        nodeAbove.Position++;
        jsonElementViewModel.Position--;
        parent.Children.OnCollectionReset();

        todo.Data = TodoJsonParentViewModel.Export(todo.JsonViewModel);
        WeakReferenceMessenger.Default.Send(todo);
    }

    [RelayCommand]
    public void MoveElementDown(TodoJsonElementViewModel jsonElementViewModel)
    {
        var todo = (TodoViewModel)SelectedNode;
        var parent = FindJsonElementParent(todo.JsonViewModel, jsonElementViewModel);
        for (int i = 0; i < parent.Children.Count; i++)
        {
            parent.Children[i].Position = i;
        }

        if (jsonElementViewModel.Position == parent.Children.Count - 1) return;

        var nodeBelow = parent.Children.First(n => n.Position == jsonElementViewModel.Position + 1);
        nodeBelow.Position--;
        jsonElementViewModel.Position++;
        parent.Children.OnCollectionReset();

        todo.Data = TodoJsonParentViewModel.Export(todo.JsonViewModel);
        WeakReferenceMessenger.Default.Send(todo);
    }

    [RelayCommand]
    public void MoveElementBottom(TodoJsonElementViewModel jsonElementViewModel)
    {
        var todo = (TodoViewModel)SelectedNode;
        var parent = FindJsonElementParent(todo.JsonViewModel, jsonElementViewModel);
        for (int i = 0; i < parent.Children.Count; i++)
        {
            parent.Children[i].Position = i;
        }

        if (jsonElementViewModel.Position == parent.Children.Count - 1) return;

        foreach (var child in parent.Children)
        {
            if (child.Position > jsonElementViewModel.Position)
            {
                child.Position--;
            }
        }
        jsonElementViewModel.Position = parent.Children.Count - 1;
        parent.Children.OnCollectionReset();

        todo.Data = TodoJsonParentViewModel.Export(todo.JsonViewModel);
        WeakReferenceMessenger.Default.Send(todo);
    }

    [RelayCommand]
    public void DeleteJsonElement(TodoJsonElementViewModel jsonElementViewModel)
    {
        var todo = (TodoViewModel)SelectedNode;
        var parent = FindJsonElementParent(todo.JsonViewModel, jsonElementViewModel);
        parent.Children.Remove(jsonElementViewModel);
        jsonElementViewModel.Parent.Remove(jsonElementViewModel.Key);
        todo.OnDataTextPropertyChanged();
        WeakReferenceMessenger.Default.Send(todo);
    }

    public void Traverse(TodoJsonElementViewModel element, Action<TodoJsonElementViewModel> callback)
    {
        callback(element);
        if (element is TodoJsonParentViewModel parent)
        {
            foreach (var child in parent.Children)
            {
                Traverse(child, callback);
            }
        }
    }

    public TodoJsonParentViewModel FindJsonElementParent(TodoJsonParentViewModel ancestor, TodoJsonElementViewModel targetJsonElementViewModel)
    {
        foreach (var jsonElement in ancestor.Children)
        {
            if (jsonElement == targetJsonElementViewModel)
            {
                return ancestor;
            }

            if (jsonElement is TodoJsonParentViewModel subElement)
            {
                var subElementResult = FindJsonElementParent(subElement, targetJsonElementViewModel);
                if (subElementResult is not null)
                {
                    return subElementResult;
                }
            }
        }

        return null;
    }

    public void SaveTodo(TodoViewModel todo)
    {
        _todoRepository.Update(todo);
        _todoRepository.Save();
    }

    protected override Task AddNode()
    {
        var newNode = new TodoViewModel()
        {
            Date = ResolveTargetDate(0),
            Data = GetJsonFromTemplate()
        };
        base.AddNode(newNode);
        return Task.CompletedTask;
    }

    public virtual DateOnly ResolveTargetDate(int offset)
    {
        var utcNow = DateTime.UtcNow;
        var targetDate = DateOnly.FromDateTime(utcNow).AddDays(offset);
        return targetDate;
    }

    protected JsonObject GetJsonFromTemplate()
    {
        if (Root.Data == null) Root.Data = new JsonObject();

        return (JsonObject)Root.Data.DeepClone();
    }

    partial void OnShowTemplateChanged(bool value)
    {
        base.SelectNode(value ? Root : null);
    }
}