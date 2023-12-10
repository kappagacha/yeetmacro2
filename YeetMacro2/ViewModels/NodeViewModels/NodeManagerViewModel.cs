using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Serialization;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

// https://stackoverflow.com/questions/53884417/net-core-di-ways-of-passing-parameters-to-constructor
public class NodeManagerViewModelFactory
{
    IServiceProvider _serviceProvider;
    public NodeManagerViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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
    static IMapper _mapper;
    private int _rootNodeId;
    private string _nodeTypeName;
    [ObservableProperty]
    protected TParent _root;
    [ObservableProperty]
    protected TChild _selectedNode;
    [ObservableProperty]
    bool _isInitialized, _showExport, _isList;
    TaskCompletionSource _initializeCompleted;
    protected INodeService<TParent, TChild> _nodeService;
    protected IToastService _toastService;
    protected IInputService _inputService;
    [ObservableProperty]
    string _exportValue;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasCopyClipboard))]
    TChild _copyClipboard;

    public bool HasCopyClipboard => CopyClipboard != null;

    public static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new JsonSerializerOptions()
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

    private void Init()
    {
        if (_rootNodeId <= 0) return;       // serialization sends -1 rootNodeId

        Task.Run(() =>
        {
            var sw = new Stopwatch();
            sw.Start();
            var _mapper = ServiceHelper.GetService<IMapper>();
            var root = _mapper.Map<TViewModel>(_nodeService.GetRoot(_rootNodeId));
            _nodeService.ReAttachNodes(root);
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
        var nodeTypes = NodeMetadataHelper.GetNodeTypes<TViewModel>();
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
            parent.Nodes.Add(newNode);
            SelectedNode.IsExpanded = true;
        }
        else
        {
            newNode.ParentId = Root.NodeId;
            newNode.RootId = Root.NodeId;
            Root.Nodes.Add(newNode);
        }

        ResolvePath(Root);
        _nodeService.Insert(newNode);
        _toastService.Show($"Created {_nodeTypeName}: " + newNode.Name);
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
            var parent = (TParent)_nodeService.Get(node.ParentId.Value);
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

            target.IsSelected = true;
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

    public string ToJson()
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
    public void Export(string name)
    {
        //        if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted) return;
        var json = JsonSerializer.Serialize(this, _defaultJsonSerializerOptions);
#if ANDROID
        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
        var targetDirectory = DeviceInfo.Current.Platform == DevicePlatform.Android ?
            Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath :
            FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(targetDirectory, $"{name}_{_nodeTypeName}s.json");
        File.WriteAllText(targetFile, json);
#elif WINDOWS
        var targetDirectory = FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(targetDirectory, $"{name}_{_nodeTypeName}s.json");
        File.WriteAllText(targetFile, json);
#endif
        // Clipboard seems to have a character limit in Android
        ExportValue = json;
        ShowExport = true;
        _toastService.Show($"Exported {_nodeTypeName}: {name}");
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
                new Type[] { viewModelType, parentType, childType }),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: new object[] { options },
            culture: null)!;

        return converter;
    }

    private class GenericNodeViewModelValueConverter<TViewModel, TParent, TChild> : JsonConverter<NodeManagerViewModel<TViewModel, TParent, TChild>>
        where TViewModel : TParent, new()
        where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
        where TChild : Node
    {
        private readonly JsonConverter<TChild> _childConverter;

        public GenericNodeViewModelValueConverter(JsonSerializerOptions options)
        {
            _childConverter = (JsonConverter<TChild>)options.GetConverter(typeof(TChild));
        }

        public override NodeManagerViewModel<TViewModel, TParent, TChild> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var tree = new NodeManagerViewModel<TViewModel, TParent, TChild>(-1, null, null, null);
            tree.Root = new TParent();
            tree.Root.Nodes = new List<TChild>();

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