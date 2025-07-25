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
    readonly TodoJsonParentViewModel _emptySubView = new();
    [ObservableProperty]
    bool _showJsonEditor, _showTemplate;
    [ObservableProperty]
    TodoJsonParentViewModel _currentSubViewModel;
    [ObservableProperty]
    TodoJsonElementViewModel _selectedJsonElement;
    string _targetSubViewName;

    readonly IRepository<TodoNode> _todoRepository;
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

        InitTodoRoot();
    }

    protected override void Init(Action callback = null)
    {
        // Not doing the normal filling of nodes
    }

    private void InitTodoRoot()
    {
        Root = _mapper.Map<TodoViewModel>(_todoRepository.Get(td => td.NodeId == _rootNodeId, noTracking: true).First());
        IsInitialized = true;
        _initializeCompleted.SetResult();
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
        var todo = ResolveTodo(targetDate);

        SelectedNode = todo;
        var targetJsonViewModel = ((TodoViewModel)todo).JsonViewModel;
        if (targetJsonViewModel is null) return;

        CurrentSubViewModel = ((TodoJsonParentViewModel)targetJsonViewModel.Children.FirstOrDefault(c => c.Key == _targetSubViewName)) ?? _emptySubView;
    }

    protected TodoViewModel ResolveTodo(DateOnly targetDate)
    {
        var existingTodo = Root.Nodes.FirstOrDefault(dn => dn.Date == targetDate) ?? _todoRepository.Get(td => td.RootId == _rootNodeId && td.Date == targetDate, noTracking: true).FirstOrDefault();

        if (existingTodo is null)
        {
            var newTodo = new TodoViewModel()
            {
                Date = targetDate,
                Data = Root.Data
            };
            this.AddNode(newTodo);
            return newTodo;
        }
        else if (existingTodo is not TodoViewModel)
        {
            var existingTodoViewModel = _mapper.Map<TodoViewModel>(existingTodo);
            Root.Nodes.Add(existingTodoViewModel);
            return existingTodoViewModel;
        }

        return existingTodo as TodoViewModel;
    }

    [RelayCommand]
    public void SaveTodo(object[] values)
    {
        if (values[0] is TodoViewModel todo && values[1] is string stringValue)
        {
            try
            {
                todo.Data = stringValue;
                todo.JsonViewModel = TodoJsonParentViewModel.Load(stringValue, todo);
                ResolveCurrentSubViewModel();
                SaveTodo(todo);
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
        TodoJsonElementViewModel elementToAdd;

        switch (selectedTypeName)
        {
            case "parent":
                var jsonObject = new JsonObject();
                elementToAdd = new TodoJsonParentViewModel() { Key = elementName, Parent = parent, ViewModel = todo };
                break;
            case "bool":
                elementToAdd = new TodoJsonBooleanViewModel() { Key = elementName, Parent = parent, ViewModel = todo };
                break;
            case "count":
                elementToAdd = new TodoJsonCountViewModel() { Key = elementName, Parent = parent, ViewModel = todo };
                break;
            default:
                throw new NotImplementedException($"Unimplemented todo element type: ${selectedTypeName}");
        }

        parent.Children.Add(elementToAdd);
        todo.Data = TodoJsonParentViewModel.Export(parent.Root);
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
        todo.Data = TodoJsonParentViewModel.Export(parent.Root);
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
            Data = Root.Data
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

    partial void OnShowTemplateChanged(bool value)
    {
        base.SelectNode(value ? Root : null);
    }
}