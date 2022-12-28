using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;
public partial class TreeViewViewModel<TParent, TChild> : ObservableObject
        where TParent : Node, IParentNode<TParent, TChild>, TChild
        where TChild : Node, new()
{
    [ObservableProperty]
    protected TParent _root;
    [ObservableProperty]
    protected TChild _selectedNode;
    protected INodeService<TParent, TChild> _nodeService;
    protected IToastService _toastService;
    protected IWindowManagerService _windowManagerService;

    public TreeViewViewModel(
        INodeService<TParent, TChild> nodeService,
        IWindowManagerService windowManagerService,
        IToastService toastService)
    {
        _nodeService = nodeService;
        _windowManagerService = windowManagerService;
        _toastService = toastService;
    }

    //protected virtual void Init()
    //{
    //    Root = _nodeService.GetRoot();
    //    Root = ProxyViewModel.Create(Root);
    //    _nodeService.ReAttachNodes(Root);
    //}

    protected virtual void OnBeforeAddNode(TChild newNode)
    {
    }

    [RelayCommand]
    public async void AddNode()
    {
        var name = await _windowManagerService.PromptInput("Please enter node name: ");

        if (string.IsNullOrWhiteSpace(name))
        {
            _toastService.Show("Canceled add pattern node");
            return;
        }

        var newNode = ProxyViewModel.Create(new TChild()
        {
            Name = name
        });

        OnBeforeAddNode(newNode);

        if (SelectedNode != null && SelectedNode is TParent parent)
        {
            newNode.ParentId = SelectedNode.NodeId;
            parent.Children.Add(newNode);
            SelectedNode.IsExpanded = true;
        }
        else
        {
            newNode.ParentId = Root.NodeId;
            Root.Children.Add(newNode);
        }

        _nodeService.Insert(newNode);
        _toastService.Show("Created pattern node: " + name);
    }

    [RelayCommand]
    private void DeleteNode(TChild node)
    {
        if (node.ParentId.HasValue)
        {
            var parent = (TParent)_nodeService.Get(node.ParentId.Value);
            parent.Children.Remove(node);
        }
        else
        {
            Root.Children.Remove(node);
        }

        _nodeService.Delete(node);

        _toastService.Show("Deleted pattern node: " + node.Name);

        if (node.IsSelected)
        {
            SelectedNode = null;
        }
    }

    [RelayCommand]
    public void ViewNode(TChild node)
    {
        node.IsSelected = false;
        SelectNode(node);
        _windowManagerService.Show(WindowView.PatternsView);
    }

    [RelayCommand]
    public void SelectNode(TChild target)
    {
        target.IsSelected = !target.IsSelected;

        if (SelectedNode != null && SelectedNode != target)
        {
            SelectedNode.IsSelected = false;
        }

        if (target.IsSelected && SelectedNode != target)
        {
            SelectedNode = target;
        }
        else if (!target.IsSelected)
        {
            SelectedNode = null;
        }
    }

    [RelayCommand]
    public void Test()
    {
        //var patterns = _patternRepository.Get();


        //SelectedPattern.Pattern.Name = DateTime.Now.ToString();

        //_patternRepository.Insert(SelectedPattern.Pattern);
        //_patternRepository.Save();
    }
}