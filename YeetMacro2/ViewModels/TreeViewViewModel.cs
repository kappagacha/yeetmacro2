using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;
public class TreeViewViewModel<TParent, TChild> : ObservableObject
        where TParent : Node, IParentNode<TParent, TChild>, TChild
        where TChild : Node, new()
{
    long _lastDragStart;
    protected TParent _root;
    protected INodeService<TParent, TChild> _nodeService;
    protected IToastService _toastService;
    protected IWindowManagerService _windowManagerService;
    protected TChild _selectedNode, _draggedTChild;
    public ICommand AddNodeCommand { get; }
    public ICommand DeleteNodeCommand { get; }
    public ICommand ViewNodeCommand { get; }
    public ICommand TestCommand { get; }
    public ICommand SelectNodeCommand { get; set; }
    public ICommand NodeDragStartingCommand { get; set; }
    public ICommand NodeDragOverCommand { get; set; }
    public ICommand NodeDropCommand { get; set; }
    public ICommand NodeDragLeaveCommand { get; set; }
    public TParent Root
    {
        get { return _root; }
        set { SetProperty(ref _root, value); }
    }
    public TChild SelectedNode
    {
        get { return _selectedNode; }
        set { SetProperty(ref _selectedNode, value); }
    }

    public TreeViewViewModel()
    {
    }

    public TreeViewViewModel(
        INodeService<TParent, TChild> nodeService,
        IWindowManagerService windowManagerService,
        IToastService toastService)
    {
        AddNodeCommand = new Command(() => AddNode());
        DeleteNodeCommand = new Command<TChild>(DeleteNode);
        ViewNodeCommand = new Command<TChild>(ViewNode);
        TestCommand = new Command(() => Test());
        SelectNodeCommand = new Command<TChild>(SelectNode);
        NodeDragStartingCommand = new Command<TChild>(NodeStartDragging);
        NodeDragOverCommand = new Command<TChild>(NodeDragOver);
        NodeDropCommand = new Command<TChild>(NodeDrop);
        NodeDragLeaveCommand = new Command<TChild>(NodeDragLeave);
        _nodeService = nodeService;
        _windowManagerService = windowManagerService;
        _toastService = toastService;
    }

    protected virtual void Init()
    {
        Root = _nodeService.GetRoot();
        Root = ProxyViewModel.Create(Root);
        _nodeService.ReAttachNodes(Root);
    }

    protected virtual void OnBeforeAddNode(TChild newNode)
    {
    }

    private void NodeStartDragging(TChild node)
    {
        var unixNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        // don't invoke for the next 100ms to avoid ancestor start dragging
        if (unixNow - _lastDragStart > 100)
        {
            Console.WriteLine("Start Dragging: " + node.Name);
            _draggedTChild = node;
            _lastDragStart = unixNow;
        }
    }
    private void NodeDragOver(TChild node)
    {
        Console.WriteLine("Drag Over: " + node.Name);
        var proxy = (IProxyNotifyPropertyChanged)node;

        if (_draggedTChild != node)
        {
            proxy.BorderColor = Colors.Green;
        }
        else
        {
            proxy.BorderColor = Colors.Red;
        }

        var draggedProxy = (IProxyNotifyPropertyChanged)_draggedTChild;
        draggedProxy.Color = Colors.Green;
    }
    private void NodeDragLeave(TChild node)
    {
        Console.WriteLine("Drag Leave: " + node.Name);
        var proxy = (IProxyNotifyPropertyChanged)node;
        proxy.BorderColor = Colors.Transparent;
    }
    private void NodeDrop(TChild node)
    {
        if (node == _draggedTChild || _nodeService.IsDescendant((TParent)_draggedTChild, node) || node.NodeId == _draggedTChild.ParentId)
        {
            return;
        }

        if (node is TParent newParent)
        {
            var draggedProxy = (IProxyNotifyPropertyChanged)_draggedTChild;
            draggedProxy.Color = Colors.Transparent;

            Console.WriteLine("Drop: " + node.Name);
            var proxy = (IProxyNotifyPropertyChanged)node;
            proxy.Color = Colors.Transparent;
            proxy.BorderColor = Colors.Transparent;

            if (_draggedTChild.ParentId.HasValue)
            {
                var currentParent = (TParent)_nodeService.Get(_draggedTChild.ParentId.Value);
                currentParent.Children.Remove(_draggedTChild);
            }
            else
            {
                Root.Children.Remove(_draggedTChild);
            }

            _draggedTChild.ParentId = node.NodeId;
            newParent.Children.Add(_draggedTChild);
            _nodeService.Update(_draggedTChild);
        }
    }

    private async void AddNode()
    {
        string name = await _windowManagerService.PromptInput("Please enter node name: ");
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

            try
            {
                newNode.ParentId = SelectedNode.NodeId;
                parent.Children.Add(newNode);
                SelectedNode.IsExpanded = true;
            }
            catch (Exception ex)
            {

            }
        }
        else
        {
            newNode.ParentId = Root.NodeId;
            Root.Children.Add(newNode);
        }

        _nodeService.Insert(newNode);
        _toastService.Show("Created pattern node: " + name);
    }

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

    private void ViewNode(TChild node)
    {
        node.IsSelected = false;
        SelectNode(node);
        _windowManagerService.Show(WindowView.PatternsView);
    }

    protected void SelectNode(TChild target)
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

    private void Test()
    {
        //var patterns = _patternRepository.Get();


        //SelectedPattern.Pattern.Name = DateTime.Now.ToString();

        //_patternRepository.Insert(SelectedPattern.Pattern);
        //_patternRepository.Save();
    }
}