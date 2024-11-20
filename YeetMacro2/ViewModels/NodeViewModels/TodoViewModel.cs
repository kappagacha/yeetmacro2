using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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

    public override string Data
    {
        get => base.Data;
        set
        {
            base.Data = value;
            OnPropertyChanged();

            if (Data is not null && JsonViewModel is null)
            {
                JsonViewModel = TodoJsonParentViewModel.Load(Data, this);
            }
        }
    }

    public TodoViewModel()
    {
        base.Nodes = new NodeObservableCollection<TodoViewModel, TodoNode>((a, b) => b.Date > a.Date ? 1 : 0);
    }
}

public abstract partial class TodoJsonElementViewModel : ObservableObject, ISortable
{
    [ObservableProperty]
    bool _isExpanded = false, _isSelected = false;
    public TodoJsonParentViewModel Parent { get; set; }
    public TodoJsonParentViewModel Root { get => Parent?.Root ?? Parent ?? (TodoJsonParentViewModel)this; }
    public virtual TodoViewModel ViewModel
    {
        get { return Parent.ViewModel; }
        set { }
    }
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
    TodoViewModel _viewModel;
    public override TodoViewModel ViewModel 
    { 
        get { return Parent?.ViewModel ?? _viewModel; }
        set { _viewModel = value; }
    }

    public override bool IsParent => true;

    public static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new()
    {
        Converters = {
            new TodoJsonParentViewModelJsonConverter()
        },
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static TodoJsonParentViewModel Load(string jsonString, TodoViewModel node)
    {
        var jsonViewModel = (TodoJsonParentViewModel)JsonSerializer.Deserialize<TodoJsonElementViewModel>(jsonString, _defaultJsonSerializerOptions);
        jsonViewModel.ViewModel = node;
        return jsonViewModel;
    }

    public static string Export(TodoJsonParentViewModel todoJsonParentViewModel)
    {
        var jsonText = JsonSerializer.Serialize<TodoJsonElementViewModel>(todoJsonParentViewModel, _defaultJsonSerializerOptions);
        return jsonText;
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
        if (Key is null) return;

        //ViewModel.OnDataTextPropertyChanged();
        ViewModel.Data = TodoJsonParentViewModel.Export(Root);
        WeakReferenceMessenger.Default.Send(ViewModel);
    }
}

public partial class TodoJsonCountViewModel : TodoJsonElementViewModel
{
    [ObservableProperty]
    int _count;

    partial void OnCountChanged(int value)
    {
        if (Key is null) return;

        //ViewModel.OnDataTextPropertyChanged();
        ViewModel.Data = TodoJsonParentViewModel.Export(Root);
        WeakReferenceMessenger.Default.Send(ViewModel);
    }
}

public class TodoJsonParentViewModelJsonConverter() : JsonConverter<TodoJsonElementViewModel>
{
    public override TodoJsonElementViewModel Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var json = new TodoJsonParentViewModel();
        var children = new List<TodoJsonElementViewModel>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                foreach (var child in children)
                {
                    child.Parent = json;
                    json.Children.Add(child);
                }
                return json;
            }

            if (reader.TokenType == JsonTokenType.PropertyName) 
            {
                var prop = reader.GetString();
                reader.Read();

                if (reader.TokenType == JsonTokenType.False || reader.TokenType == JsonTokenType.True)
                {
                    var boolValue = reader.GetBoolean();
                    children.Add(new TodoJsonBooleanViewModel() {  IsChecked = boolValue, Key = prop });
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    var numValue = reader.GetInt32();
                    children.Add(new TodoJsonCountViewModel() { Count = numValue, Key = prop });
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var child = Read(ref reader, typeof(TodoJsonElementViewModel), options);
                    child.Key = prop;
                    children.Add(child);
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        TodoJsonElementViewModel todo,
        JsonSerializerOptions options)
    {
        if (todo is TodoJsonParentViewModel parent)
        {
            writer.WriteStartObject();
            foreach (var child in parent.Children)
            {
                writer.WritePropertyName(child.Key);
                Write(writer, child, options);
            }
            writer.WriteEndObject();
        }
        else if (todo is TodoJsonBooleanViewModel todoBool)
        {
            writer.WriteBooleanValue(todoBool.IsChecked);
        }
        else if (todo is TodoJsonCountViewModel todoCount)
        {
            writer.WriteNumberValue(todoCount.Count);
        }
    }
}