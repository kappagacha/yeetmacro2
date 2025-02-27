using AutoMapper;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Serialization;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

// https://stackoverflow.com/questions/53884417/net-core-di-ways-of-passing-parameters-to-constructor
public class NodeManagerViewModelFactory(IServiceProvider serviceProvider)
{
    readonly IServiceProvider _serviceProvider = serviceProvider;

    public T Create<T>(int rootId) where T : NodeManagerViewModel
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, rootId);
    }
}

public abstract class NodeManagerViewModel : ObservableObject
{

}

[JsonConverter(typeof(NodeViewModelValueConverter))]
public partial class NodeManagerViewModel<TViewModel, TParent, TChild> : NodeManagerViewModel
        where TViewModel : TParent, new()
        where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
        where TChild : Node
{
    protected readonly static IMapper _mapper;
    protected readonly int _rootNodeId;
    private readonly string _nodeTypeName;
    [ObservableProperty]
    protected TParent _root;
    [ObservableProperty]
    protected TChild _selectedNode;
    [ObservableProperty]
    bool _isInitialized, _showExport, _isList, _isBusy;
    protected readonly TaskCompletionSource _initializeCompleted;
    protected INodeService<TParent, TChild> _nodeService;
    protected IToastService _toastService;
    protected IInputService _inputService;
    [ObservableProperty]
    string _exportValue;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasCopyClipboard))]
    TChild _copyClipboard;

    public bool HasCopyClipboard => CopyClipboard != null;

    public static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new()
    {
        Converters = {
            new JsonStringEnumConverter()
        },
        WriteIndented = true,
        TypeInfoResolver = CombinedPropertiesResolver.Combine(RectPropertiesResolver.Instance, SizePropertiesResolver.Instance),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    static NodeManagerViewModel()
    {
        _mapper = ServiceHelper.GetService<IMapper>();

        // https://stackoverflow.com/questions/58331479/how-to-globally-set-default-options-for-system-text-json-jsonserializer/74741382#74741382
        var copy = new JsonSerializerOptions(_defaultJsonSerializerOptions);
        _defaultJsonSerializerOptions.Converters.Add(new NodeValueConverter<TParent, TChild>(copy));

        // setting this internal value to immutable allows dynamic json typeinfo resolving
        typeof(JsonSerializerOptions).GetRuntimeFields().Single(f => f.Name == "_isReadOnly").SetValue(_defaultJsonSerializerOptions, true);
        typeof(JsonSerializerOptions).GetRuntimeFields().Single(f => f.Name == "_isReadOnly").SetValue(copy, true);
    }

    public NodeManagerViewModel(
        int rootNodeId,
        INodeService<TParent, TChild> nodeService,
        IInputService inputService,
        IToastService toastService)
    {
        _rootNodeId = rootNodeId;
        _nodeTypeName = typeof(TChild).Name.Replace("Node", "");
        _nodeService = nodeService;
        _inputService = inputService;
        _toastService = toastService;
        _initializeCompleted = new TaskCompletionSource();

        Init();
    }

    protected virtual void Init()
    {
        if (_rootNodeId <= 0) return;       // serialization sends -1 rootNodeId

        Task.Run(() =>
        {
            var sw = new Stopwatch();
            sw.Start();
            var _mapper = ServiceHelper.GetService<IMapper>();
            var root = _mapper.Map<TViewModel>(_nodeService.GetRoot(_rootNodeId));
            var firstChild = root.Nodes.FirstOrDefault();
            if (SelectedNode == null && firstChild != null)
            {
                root.IsExpanded = true;
                firstChild.IsSelected = false;
                SelectNode(firstChild);
            }
            Root = root;
            ResolvePath(root);
            CustomInit();
            IsInitialized = true;
            _initializeCompleted.SetResult();
            sw.Stop();
            Debug.WriteLine($"Loaded {typeof(TParent).Name}: {sw.ElapsedMilliseconds} ms");
        });
    }

    public void ResolvePath(Node node, string path = "")
    {
        if (node.Name == "root")
        {
            node.Path = "";
        }
        else if (string.IsNullOrWhiteSpace(path))
        {
            node.Path = node.Name;
        }
        else
        {
            node.Path = $"{path}.{node.Name}";
        }

        if (node is IParentNode<TParent, TChild> parent)
        {
            foreach (var child in parent.Nodes)
            {
                ResolvePath(child, node.Path);
            }
        }
    }

    protected virtual void CustomInit()
    {

    }

    public async Task WaitForInitialization()
    {
        await _initializeCompleted.Task;
    }

    [RelayCommand]
    protected virtual async Task AddNode()
    {
        var name = await _inputService.PromptInput($"Please enter {_nodeTypeName} name: ");

        if (string.IsNullOrWhiteSpace(name))
        {
            _toastService.Show($"Canceled add {_nodeTypeName}");
            return;
        }

        TChild newNode = null;
        var nodeTypes = NodeTypesAttribute.GetTypes<TViewModel>();
        if (nodeTypes is not null)
        {
            var selectedTypeName = await _inputService.SelectOption($"Please select {_nodeTypeName} type", nodeTypes.Select(t => t.Name.Replace("ViewModel", "")).ToArray());
            if (string.IsNullOrEmpty(selectedTypeName) || selectedTypeName == "cancel") return;
            var selectedType = nodeTypes.First(t => t.Name.Replace("ViewModel", "") == selectedTypeName);
            newNode = (TChild)Activator.CreateInstance(selectedType);
            newNode.Name = name;
        }
        else
        {
            newNode = new TViewModel() { Name = name };
        }

        AddNode(newNode);
    }

    protected void AddNode(TChild newNode)
    {
        if (!IsList && SelectedNode != null && SelectedNode is TParent parent)
        {
            newNode.ParentId = SelectedNode.NodeId;
            newNode.RootId = SelectedNode.RootId;
            parent.IsExpanded = true;
            parent.Nodes.Add(newNode);
            _nodeService.Update(parent);
        }
        else
        {
            newNode.ParentId = Root.NodeId;
            newNode.RootId = Root.NodeId;
            Root.Nodes.Add(newNode);
            _nodeService.Update(Root);
        }

        ResolvePath(Root);
        _nodeService.Insert(newNode);
        SelectNode(newNode);
        //_toastService.Show($"Created {_nodeTypeName}: " + newNode.Name);
        // Fix attempt for getting stuck upon script initialization. Theorizing that when daily is created, it can get stuck with _toastService
        // TODO: remove this if it doesn't help
        MainThread.BeginInvokeOnMainThread(() => _toastService.Show($"Created {_nodeTypeName}: " + newNode.Name));
    }

    [RelayCommand]
    public async Task RenameNode(TChild node)
    {
        var name = await _inputService.PromptInput($"Please enter {_nodeTypeName} new name: ", node.Name);

        if (string.IsNullOrWhiteSpace(name))
        {
            _toastService.Show($"Canceled rename {_nodeTypeName}");
            return;
        }

        ReflectionHelper.PropertyInfoCollection[node.GetType()]["Name"].SetValue(node, name);
        ResolvePath(Root);
        _nodeService.Update(node);
        _toastService.Show($"Renamed {_nodeTypeName}: " + name);
    }

    [RelayCommand]
    protected virtual void DeleteNode(TChild node)
    {
        if (node.ParentId.HasValue)
        {
            var parent = FindById<TParent>(Root, node.ParentId.Value);
            parent.Nodes.Remove(node);
        }
        else
        {
            Root.Nodes.Remove(node);
        }

        _nodeService.Delete(node);
        _toastService.Show($"Deleted {_nodeTypeName}: " + node.Name);

        if (node.IsSelected)
        {
            SelectedNode = null;
        }
    }

    [RelayCommand]
    public void SelectNode(TChild target)
    {
        if (target == SelectedNode)
        {
            target.IsSelected = !target.IsSelected;
        }
        else
        {
            if (SelectedNode is not null)
            {
                SelectedNode.IsSelected = false;
            }

            if (target is not null) target.IsSelected = true;
            SelectedNode = target;
        }

        if (SelectedNode is not null && !SelectedNode.IsSelected)
        {
            SelectedNode = null;
        }
    }

    [RelayCommand]
    public void CollapseAll()
    {
        Traverse(Root, (node) => node.IsExpanded = false);
    }

    [RelayCommand]
    public void ExpandAll()
    {
        Traverse(Root, (node) => node.IsExpanded = true);
    }

    [RelayCommand]
    public void MoveNodeTop(TChild node)
    {
        var parent = FindById<TParent>(Root, node.ParentId.Value);
        for (int i = 0; i < parent.Nodes.Count; i++)
        {
            parent.Nodes[i].Position = i;
        }

        if (node.Position == 0) return;

        foreach (var child in parent.Nodes)
        {
            if (child.Position < node.Position)
            {
                child.Position++;
            }
        }
        node.Position = 0;
        _nodeService.Save();

        ((NodeObservableCollection<TViewModel, TChild>)parent.Nodes).OnCollectionReset();
    }

    [RelayCommand]
    public void MoveNodeUp(TChild node)
    {
        var parent = FindById<TParent>(Root, node.ParentId.Value);
        for (int i = 0; i < parent.Nodes.Count; i++)
        {
            parent.Nodes[i].Position = i;
        }

        if (node.Position == 0) return;

        var nodeAbove = parent.Nodes.First(n => n.Position == node.Position - 1);
        nodeAbove.Position++;
        node.Position--;
        _nodeService.Save();

        ((NodeObservableCollection<TViewModel, TChild>)parent.Nodes).OnCollectionReset();
    }

    [RelayCommand]
    public void MoveNodeDown(TChild node)
    {
        var parent = FindById<TParent>(Root, node.ParentId.Value);
        for (int i = 0; i < parent.Nodes.Count; i++)
        {
            parent.Nodes[i].Position = i;
        }

        if (node.Position == parent.Nodes.Count - 1) return;

        var nodeBelow = parent.Nodes.First(n => n.Position == node.Position + 1);
        nodeBelow.Position--;
        node.Position++;
        _nodeService.Save();

        ((NodeObservableCollection<TViewModel, TChild>)parent.Nodes).OnCollectionReset();
    }

    [RelayCommand]
    public void MoveNodeBottom(TChild node)
    {
        var parent = FindById<TParent>(Root, node.ParentId.Value);
        for (int i = 0; i < parent.Nodes.Count; i++)
        {
            parent.Nodes[i].Position = i;
        }

        if (node.Position == parent.Nodes.Count - 1) return;

        foreach (var child in parent.Nodes)
        {
            if (child.Position > node.Position)
            {
                child.Position--;
            }
        }
        node.Position = parent.Nodes.Count - 1;
        _nodeService.Save();

        ((NodeObservableCollection<TViewModel, TChild>)parent.Nodes).OnCollectionReset();
    }

    public void Traverse(TChild node, Action<TChild> callback)
    {
        callback(node);
        if (node is TParent parent)
        {
            foreach (var child in parent.Nodes)
            {
                Traverse(child, callback);
            }
        }
    }

    public virtual string ToJson()
    {
        return JsonSerializer.Serialize(this, _defaultJsonSerializerOptions);
    }

    public static NodeManagerViewModel<TViewModel, TParent, TChild> FromJson(string json)
    {
        return JsonSerializer.Deserialize<NodeManagerViewModel<TViewModel, TParent, TChild>>(json, _defaultJsonSerializerOptions);
    }

    public static TChild FromJsonNode(string json)
    {
        return JsonSerializer.Deserialize<TChild>(json, _defaultJsonSerializerOptions);
    }

    public static TChild CloneNode(TChild node)
    {
        var json = JsonSerializer.Serialize(node);
        return (TChild)JsonSerializer.Deserialize(json, node.GetType());
    }

    [RelayCommand]
    public async Task Export(string name)
    {
#if ANDROID
        // https://stackoverflow.com/questions/75880663/maui-on-android-listing-folder-contents-of-an-sd-card-and-writing-in-it
        if (OperatingSystem.IsAndroidVersionAtLeast(30) && !Android.OS.Environment.IsExternalStorageManager)
        {
            var intent = new Android.Content.Intent();
            intent.SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", Platform.CurrentActivity.PackageName, null);
            intent.SetData(uri);
            Platform.CurrentActivity.StartActivity(intent);
            return;
        }
        else if (!OperatingSystem.IsAndroidVersionAtLeast(30) && await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
        {
            return;
        }
#endif
        var json = JsonSerializer.Serialize(this, _defaultJsonSerializerOptions);
        var defaultFolder = FileSystem.Current.AppDataDirectory;

#if ANDROID
        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
        defaultFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
#endif

        var result = await FolderPicker.Default.PickAsync(defaultFolder, new CancellationToken());
        if (!result.IsSuccessful) return;
        var targetFile = Path.Combine(result.Folder.Path, $"{name}_{_nodeTypeName}s_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.json");
        File.WriteAllText(targetFile, json);
        _toastService.Show($"Exported {name} {_nodeTypeName} on ${result.Folder.Path}");
    }

    [RelayCommand]
    private void CloseExport()
    {
        ShowExport = false;
    }

    [RelayCommand]
    public void Import(NodeManagerViewModel<TViewModel, TParent, TChild> nodeViewModel)
    {
        var rootTemp = _mapper.Map<TViewModel>(nodeViewModel.Root);
        var currentChildren = Root.Nodes.ToList();
        foreach (var currentChild in currentChildren)
        {
            _nodeService.Delete(currentChild);
            Root.Nodes.Remove(currentChild);
        }

        var newChildren = rootTemp.Nodes;
        foreach (var newChild in newChildren)
        {
            newChild.RootId = Root.NodeId;
            newChild.ParentId = Root.NodeId;
            _nodeService.Insert(newChild);
            Root.Nodes.Add(newChild);
        }

        ResolvePath(Root);
    }

    [RelayCommand]
    public void CopyNode(TChild node)
    {
        CopyClipboard = node;
    }

    [RelayCommand]
    public void ClearCopyNode()
    {
        CopyClipboard = null;
    }

    [RelayCommand]
    public void PasteNode()
    {
        var newNode = CloneNode(CopyClipboard);
        if (SelectedNode != null && SelectedNode is TParent parent)
        {
            newNode.ParentId = SelectedNode.NodeId;
            newNode.RootId = SelectedNode.RootId;
            parent.Nodes.Add(newNode);
            SelectedNode.IsExpanded = true;
        }
        else
        {
            newNode.ParentId = Root.NodeId;
            newNode.RootId = Root.NodeId;
            Root.Nodes.Add(newNode);
        }

        _nodeService.Insert(newNode);
        _toastService.Show($"Copied {_nodeTypeName}: " + newNode.Name);
    }

    [RelayCommand]
    public void RefreshCollections()
    {
        RefreshCollections(Root);
    }

    public void RefreshCollections(IParentNode<TParent, TChild> node)
    {
        ((NodeObservableCollection<TViewModel, TChild>)node.Nodes).OnCollectionReset();

        foreach (var childNode in node.Nodes)
        {
            if (childNode is IParentNode<TParent, TChild> subParentNode)
            {
                RefreshCollections(subParentNode);
            }
        }
    }

    public TTarget FindById<TTarget>(TChild node, int targetNodeId) where TTarget : TChild
    {
        if (node.NodeId == targetNodeId) return node as TTarget;

        if (node is TParent parent)
        {
            foreach (var child in parent.Nodes)
            {
                var childResult = FindById<TTarget>(child, targetNodeId);
                if (childResult is not null) return childResult;
            }
        }

        return null;
    }

    [RelayCommand]
    public void Save()
    {
        _nodeService.Save();
    }
}

// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0
// https://devblogs.microsoft.com/dotnet/system-text-json-in-dotnet-7/#contract-customization
public class NodeViewModelValueConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(NodeManagerViewModel<,,>))
        {
            return false;
        }

        return true;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type viewModelType = typeToConvert.GetGenericArguments()[0];
        Type parentType = typeToConvert.GetGenericArguments()[1];
        Type childType = typeToConvert.GetGenericArguments()[2];

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(GenericNodeViewModelValueConverter<,,>).MakeGenericType(
                [viewModelType, parentType, childType]),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [options],
            culture: null)!;

        return converter;
    }

    private class GenericNodeViewModelValueConverter<TViewModel, TParent, TChild>(JsonSerializerOptions options) : JsonConverter<NodeManagerViewModel<TViewModel, TParent, TChild>>
        where TViewModel : TParent, new()
        where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
        where TChild : Node
    {
        private readonly JsonConverter<TChild> _childConverter = (JsonConverter<TChild>)options.GetConverter(typeof(TChild));

        public override NodeManagerViewModel<TViewModel, TParent, TChild> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var tree = new NodeManagerViewModel<TViewModel, TParent, TChild>(-1, null, null, null)
            {
                Root = new TParent()
            };
            tree.Root.Nodes = [];

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return tree;
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var child = _childConverter.Read(ref reader, typeof(TChild), options);
                    tree.Root.Nodes.Add(child);
                }
            }

            throw new JsonException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            NodeManagerViewModel<TViewModel, TParent, TChild> tree,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (tree.Root != null)
            {
                foreach (var child in tree.Root.Nodes)
                {
                    _childConverter.Write(writer, child, options);
                }
            }

            writer.WriteEndObject();
        }
    }
}