using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Serialization;
using YeetMacro2.Data.Services;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels;

// https://stackoverflow.com/questions/53884417/net-core-di-ways-of-passing-parameters-to-constructor
public class NodeViewModelFactory
{
    IServiceProvider _serviceProvider;
    public NodeViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Create<T>(int rootId) where T : NodeViewModel
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, rootId);
    }
}

public abstract class NodeViewModel : ObservableObject
{

}

[JsonConverter(typeof(NodeViewModelValueConverter))]
public partial class NodeViewModel<TParent, TChild> : NodeViewModel
        where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
        where TChild : Node
{
    private int _rootNodeId;
    private string _nodeTypeName;
    [ObservableProperty]
    protected TParent _root;
    [ObservableProperty]
    protected TChild _selectedNode;
    [ObservableProperty]
    bool _isInitialized, _showExport;
    TaskCompletionSource _initializeCompleted;
    protected INodeService<TParent, TChild> _nodeService;
    protected IToastService _toastService;
    protected IInputService _inputService;
    [ObservableProperty]
    string _exportValue;

    private static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new JsonSerializerOptions()
    {
        Converters = {
            new JsonStringEnumConverter()
        },
        WriteIndented = true,
        TypeInfoResolver  = CombinedPropertiesResolver.Combine(RectPropertiesResolver.Instance, SizePropertiesResolver.Instance),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    static NodeViewModel()
    {
        // https://stackoverflow.com/questions/58331479/how-to-globally-set-default-options-for-system-text-json-jsonserializer/74741382#74741382
        var copy = new JsonSerializerOptions(_defaultJsonSerializerOptions);
        _defaultJsonSerializerOptions.Converters.Add(new NodeValueConverter<TParent, TChild>(copy));

        // setting this internal value to immutable allows dynamic json typeinfo resolving
        typeof(JsonSerializerOptions).GetRuntimeFields().Single(f => f.Name == "_isImmutable").SetValue(_defaultJsonSerializerOptions, true);
        typeof(JsonSerializerOptions).GetRuntimeFields().Single(f => f.Name == "_isImmutable").SetValue(copy, true);
    }

    public NodeViewModel(
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
            var root = (TParent)ProxyViewModel.Create<TChild>(_nodeService.GetRoot(_rootNodeId));
            _nodeService.ReAttachNodes(root);
            var firstChild = root.Nodes.FirstOrDefault();
            if (SelectedNode == null && firstChild != null)
            {
                root.IsExpanded = true;
                firstChild.IsSelected = false;
                SelectNode(firstChild);
            }
            Root = root;
            CustomInit();
            IsInitialized = true;
            _initializeCompleted.SetResult();
        });
    }

    protected virtual void CustomInit()
    {

    }

    public async Task WaitForInitialization()
    {
        await _initializeCompleted.Task;
    }

    [RelayCommand]
    public async void AddNode()
    {
        var name = await _inputService.PromptInput($"Please enter {_nodeTypeName} name: ");

        if (string.IsNullOrWhiteSpace(name))
        {
            _toastService.Show($"Canceled add {_nodeTypeName}");
            return;
        }

        TChild newNode = null;
        var nodeTypes = NodeMetadataHelper.GetNodeTypes<TChild>();
        if (nodeTypes is not null)
        {
            var selectedTypeName = await _inputService.SelectOption($"Please select {_nodeTypeName} type", nodeTypes.Select(t => t.Name).ToArray());
            if (String.IsNullOrEmpty(selectedTypeName) || selectedTypeName == "cancel") return;
            var selectedType = nodeTypes.First(t => t.Name == selectedTypeName);
            newNode = ProxyViewModel.Create((TChild)Activator.CreateInstance(selectedType));
            newNode.Name = name;
        }
        else
        {
            newNode = ProxyViewModel.Create<TChild>(new TParent()
            {
                Name = name
            });
        }

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
        _toastService.Show($"Created {_nodeTypeName}: " + name);
    }

    [RelayCommand]
    private void DeleteNode(TChild node)
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

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, _defaultJsonSerializerOptions);
    }

    public static NodeViewModel<TParent, TChild> FromJson(string json)
    {
        return JsonSerializer.Deserialize<NodeViewModel<TParent, TChild>>(json, _defaultJsonSerializerOptions);
    }

    public static TChild FromJsonNode(string json)
    {
        return JsonSerializer.Deserialize<TChild>(json, _defaultJsonSerializerOptions);
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
    public void Import(NodeViewModel<TParent, TChild> nodeViewModel)
    {
        var rootTemp = (TParent)ProxyViewModel.Create<TChild>(nodeViewModel.Root);
        var currentChildren = Root.Nodes.ToList();
        foreach (var currentChild in currentChildren)
        {
            Root.Nodes.Remove(currentChild);
            _nodeService.Delete(currentChild);
        }

        var newChildren = rootTemp.Nodes;
        foreach (var newChild in newChildren)
        {
            newChild.RootId = Root.NodeId;
            newChild.ParentId = Root.NodeId;
            Root.Nodes.Add(newChild);
            _nodeService.Insert(newChild);
        }
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
        if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(NodeViewModel<,>))
        {
            return false;
        }

        return true;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type parentType = typeToConvert.GetGenericArguments()[0];
        Type childType = typeToConvert.GetGenericArguments()[1];

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(GenericNodeViewModelValueConverter<,>).MakeGenericType(
                new Type[] { parentType, childType }),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: new object[] { options },
            culture: null)!;

        return converter;
    }

    private class GenericNodeViewModelValueConverter<TParent, TChild> : JsonConverter<NodeViewModel<TParent, TChild>>
        where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
        where TChild : Node
    {
        private readonly JsonConverter<TChild> _childConverter;


        public GenericNodeViewModelValueConverter(JsonSerializerOptions options)
        {
            _childConverter = (JsonConverter<TChild>)options.GetConverter(typeof(TChild));
        }

        public override NodeViewModel<TParent, TChild> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var tree = new NodeViewModel<TParent, TChild>(-1, null, null, null);
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
            NodeViewModel<TParent, TChild> tree,
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