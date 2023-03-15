using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using YeetMacro2.Data.Models;
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
    bool _isInitialized;
    TaskCompletionSource _initializeCompleted;
    protected INodeService<TParent, TChild> _nodeService;
    protected IToastService _toastService;
    protected IInputService _inputService;
    static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new JsonSerializerOptions()
    {
        Converters = {
            new JsonStringEnumConverter()
        },
        WriteIndented = true,
        TypeInfoResolver = NodeModelTypeInfoResolver.Instance,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
            IsInitialized = true;
            _initializeCompleted.SetResult();
        });
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

    [RelayCommand]
    public async void Export(string name)
    {
        if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted) return;

        var json = JsonSerializer.Serialize(this, _defaultJsonSerializerOptions);
#if ANDROID
        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
        var targetDirctory = DeviceInfo.Current.Platform == DevicePlatform.Android ? 
            Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath :
            FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(targetDirctory, $"{name}_{_nodeTypeName}s.json");
        File.WriteAllText(targetFile, json);
#elif WINDOWS
        var targetDirctory = FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(targetDirctory, $"{name}_{_nodeTypeName}s.json");
        File.WriteAllText(targetFile, json);
#endif
        _toastService.Show($"Exported {_nodeTypeName}: {name}");
    }

    [RelayCommand]
    public async Task Import()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var resourceNames = currentAssembly.GetManifestResourceNames().Where(rs => rs.StartsWith("YeetMacro2.Resources.MacroSets"));
        var regex = new Regex(@"YeetMacro2\.Resources\.MacroSets\.(?<macroSet>.+?)\.");

        // https://stackoverflow.com/questions/9436381/c-sharp-regex-string-extraction
        var macroSetGroups = resourceNames.GroupBy(rn => regex.Match(rn).Groups["macroSet"].Value);
        var selectedMacroSet = await Application.Current.MainPage.DisplayActionSheet($"Import {_nodeTypeName}", "Cancel", null, macroSetGroups.Select(g => g.Key).ToArray());
        if (selectedMacroSet == null || selectedMacroSet == "Cancel") return;

        using (var stream = currentAssembly.GetManifestResourceStream($"YeetMacro2.Resources.MacroSets.{selectedMacroSet}.{selectedMacroSet.Replace("_", " ")}_{_nodeTypeName}s.json"))
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            var tempTree = JsonSerializer.Deserialize<NodeViewModel<TParent, TChild>>(json, _defaultJsonSerializerOptions);
            var rootTemp = (TParent)ProxyViewModel.Create<TChild>(tempTree.Root);
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
        _toastService.Show($"Imported {_nodeTypeName}");
    }

    [RelayCommand]
    public void Save()
    {
        _nodeService.Save();
    }
}

public class NodeModelTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public static NodeModelTypeInfoResolver Instance = new NodeModelTypeInfoResolver();
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);
        
        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (JsonPropertyInfo property in typeInfo.Properties)
            {
                switch (property.Name.ToLower())
                {
                    case "nodes":
                    case "isselected":
                    case "isexpanded":
                        property.ShouldSerialize = (obj, value) => false;
                        break;
                }
            }
        }

        return typeInfo;
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
        private readonly JsonConverter<TParent> _parentConverter;
        private readonly Type _parentType;
        private readonly Type _childType;

        public GenericNodeViewModelValueConverter(JsonSerializerOptions options)
        {
            _childConverter = (JsonConverter<TChild>)(options).GetConverter(typeof(TChild));
            _parentConverter = (JsonConverter<TParent>)(options).GetConverter(typeof(TParent));

            _parentType = typeof(TParent);
            _childType = typeof(TChild);
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
                    tree.Root.Nodes.Add(ReadNode(ref reader, typeToConvert, options));
                }
            }

            throw new JsonException();
        }

        public TChild ReadNode(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var isParent = false;
            TChild node = null;
            TParent parent = null;
            ICollection<TChild> nodes = new List<TChild>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return node;
                }

                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "$isParent")
                {
                    reader.Read();
                    isParent = reader.GetBoolean();
                }

                // each property named "properties" is the properties of the node
                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "properties")
                {
                    reader.Read();
                    if (isParent)
                    {
                        parent = _parentConverter.Read(ref reader, _parentType, options);
                        node = parent;
                    }
                    else
                    {
                        node = _childConverter.Read(ref reader, _childType, options);
                    }
                    Debug.WriteLine($"nodeName: {node.Name}");
                }
                // additional properties are nodes
                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    Debug.WriteLine($"property: {reader.GetString()}");
                    parent.Nodes.Add(ReadNode(ref reader, typeToConvert, options));
                }
            }

            return node;
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
                    Write(writer, child, options);
                }
            }

            writer.WriteEndObject();
        }

        public void Write(
            Utf8JsonWriter writer,
            TChild child,
            JsonSerializerOptions options)
        {
            writer.WritePropertyName(child.Name);
            writer.WriteStartObject();
            if (child is TParent parent)
            {
                writer.WritePropertyName("$isParent");
                writer.WriteBooleanValue(true);
                writer.WritePropertyName("properties");
                _parentConverter.Write(writer, parent, options);
                foreach (var subChild in parent.Nodes)
                {
                    Write(writer, subChild, options);
                }
            }
            else
            {
                writer.WritePropertyName("$isParent");
                writer.WriteBooleanValue(false);
                writer.WritePropertyName("properties");
                _childConverter.Write(writer, child, options);
            }

            writer.WriteEndObject();
        }
    }
}

