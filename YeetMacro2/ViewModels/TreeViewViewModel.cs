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

[JsonConverter(typeof(TreeViewViewModelValueConverter))]
public partial class TreeViewViewModel<TParent, TChild> : ObservableObject
        where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
        where TChild : Node, new()
{
    [ObservableProperty]
    protected TParent _root;
    [ObservableProperty]
    protected TChild _selectedNode;
    protected INodeService<TParent, TChild> _nodeService;
    protected IToastService _toastService;
    protected IInputService _inputService;
    static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        TypeInfoResolver = TreeViewViewModelTypeInfoResolver.Instance,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public TreeViewViewModel(
        INodeService<TParent, TChild> nodeService,
        IInputService inputService,
        IToastService toastService)
    {
        _nodeService = nodeService;
        _inputService = inputService;
        _toastService = toastService;
    }

    //protected virtual void Init()
    //{
    //    Root = _nodeService.GetRoot();
    //    Root = ProxyViewModel.Create(Root);
    //    _nodeService.ReAttachNodes(Root);
    //}

    [RelayCommand]
    public async void AddNode()
    {
        var name = await _inputService.PromptInput("Please enter node name: ");

        if (string.IsNullOrWhiteSpace(name))
        {
            _toastService.Show("Canceled add pattern node");
            return;
        }

        var newNode = ProxyViewModel.Create(new TParent()
        {
            Name = name
        });

        if (SelectedNode != null && SelectedNode is TParent parent)
        {
            newNode.ParentId = SelectedNode.NodeId;
            newNode.RootId = SelectedNode.RootId;
            parent.Children.Add(newNode);
            SelectedNode.IsExpanded = true;
        }
        else
        {
            newNode.ParentId = Root.NodeId;
            newNode.RootId = Root.NodeId;
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

        var type = typeof(TChild).Name.Replace("Node", "") + "s";
        var json = JsonSerializer.Serialize(this, _defaultJsonSerializerOptions);

#if ANDROID
        // https://stackoverflow.com/questions/39332085/get-path-to-pictures-directory
        var targetDirctory = DeviceInfo.Current.Platform == DevicePlatform.Android ? 
            Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath :
            FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(targetDirctory, $"{name}_{type}.json");
        File.WriteAllText(targetFile, json);
#elif WINDOWS
        var targetDirctory = FileSystem.Current.AppDataDirectory;
        var targetFile = Path.Combine(targetDirctory, $"{name}_{type}.json");
        File.WriteAllText(targetFile, json);
#endif
        _toastService.Show($"Exported {type}: {name}");
    }

    [RelayCommand]
    private async Task Import()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var resourceNames = currentAssembly.GetManifestResourceNames().Where(rs => rs.StartsWith("YeetMacro2.Resources.MacroSets"));
        var regex = new Regex(@"YeetMacro2\.Resources\.MacroSets\.(?<macroSet>.+?)\.");

        // https://stackoverflow.com/questions/9436381/c-sharp-regex-string-extraction
        var macroSetGroups = resourceNames.GroupBy(rn => regex.Match(rn).Groups["macroSet"].Value);
        var selectedMacroSet = await Application.Current.MainPage.DisplayActionSheet("Import Patterns", "Cancel", null, macroSetGroups.Select(g => g.Key).ToArray());
        if (selectedMacroSet == null || selectedMacroSet == "Cancel") return;

        using (var stream = currentAssembly.GetManifestResourceStream($"YeetMacro2.Resources.MacroSets.{selectedMacroSet}.{selectedMacroSet.Replace("_", " ")}_patterns.json"))
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            //var x = JsonSerializer.Deserialize<TreeViewViewModel<TParent, TChild>>(json, _defaultJsonSerializerOptions);
            var tempTree = JsonSerializer.Deserialize<TreeViewViewModel<TParent, TChild>>(json, _defaultJsonSerializerOptions);
            var rootTemp = ProxyViewModel.Create(tempTree.Root);
            var currentChildren = Root.Children.ToList();
            foreach (var currentChild in currentChildren)
            {
                Root.Children.Remove(currentChild);
                _nodeService.Delete(currentChild);
            }

            var newChildren = rootTemp.Children;
            foreach (var newChild in newChildren)
            {
                Console.WriteLine("Before Insert: " + newChild.Name);
                newChild.RootId = Root.NodeId;
                newChild.ParentId = Root.NodeId;
                Root.Children.Add(newChild);
                _nodeService.Insert(newChild);
                Console.WriteLine("After Insert: " + newChild.Name);
            }
        }
        //_toastService.Show($"Imported Patterns: {_selectedMacroSet.Name})");
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

public class TreeViewViewModelTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public static TreeViewViewModelTypeInfoResolver Instance = new TreeViewViewModelTypeInfoResolver();
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);
        
        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (JsonPropertyInfo property in typeInfo.Properties)
            {
                switch (property.Name.ToLower())
                {
                    case "children":
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
public class TreeViewViewModelValueConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(TreeViewViewModel<,>))
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
            typeof(GenericTreeViewViewModelValueConverter<,>).MakeGenericType(
                new Type[] { parentType, childType }),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: new object[] { options },
            culture: null)!;

        return converter;
    }

    private class GenericTreeViewViewModelValueConverter<TParent, TChild> : JsonConverter<TreeViewViewModel<TParent, TChild>>
        where TParent : Node, IParentNode<TParent, TChild>, TChild, new()
        where TChild : Node, new()
    {
        private readonly JsonConverter<TChild> _childConverter;
        private readonly JsonConverter<TParent> _parentConverter;
        private readonly Type _parentType;
        private readonly Type _childType;

        public GenericTreeViewViewModelValueConverter(JsonSerializerOptions options)
        {
            _childConverter = (JsonConverter<TChild>)(options).GetConverter(typeof(TChild));
            _parentConverter = (JsonConverter<TParent>)(options).GetConverter(typeof(TParent));

            _parentType = typeof(TParent);
            _childType = typeof(TChild);
        }

        public override TreeViewViewModel<TParent, TChild> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var tree = new TreeViewViewModel<TParent, TChild>(null, null, null);
            tree.Root = new TParent();
            tree.Root.Children = new List<TChild>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return tree;
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    tree.Root.Children.Add(ReadNode(ref reader, typeToConvert, options));
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
            ICollection<TChild> children = new List<TChild>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return node;
                }

                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "isParent")
                {
                    reader.Read();
                    isParent = reader.GetBoolean();
                    Debug.WriteLine($"isParent: {isParent}");
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
                // additional properties are children
                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    Debug.WriteLine($"property: {reader.GetString()}");
                    parent.Children.Add(ReadNode(ref reader, typeToConvert, options));
                }
            }

            return node;
        }

        public override void Write(
            Utf8JsonWriter writer,
            TreeViewViewModel<TParent, TChild> tree,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (tree.Root != null)
            {
                foreach (var child in tree.Root.Children)
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
                writer.WritePropertyName("isParent");
                writer.WriteBooleanValue(true);
                writer.WritePropertyName("properties");
                _parentConverter.Write(writer, parent, options);
                foreach (var subChild in parent.Children)
                {
                    Write(writer, subChild, options);
                }
            }
            else
            {
                writer.WritePropertyName("isParent");
                writer.WriteBooleanValue(false);
                writer.WritePropertyName("properties");
                _childConverter.Write(writer, child, options);
            }

            writer.WriteEndObject();
        }
    }
}

