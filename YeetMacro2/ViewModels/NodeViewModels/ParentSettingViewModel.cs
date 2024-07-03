using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
[NodeTypes(
    typeof(ParentSettingViewModel), typeof(BooleanSettingViewModel), typeof(OptionSettingViewModel), typeof(EnabledOptionSettingViewModel),
    typeof(StringSettingViewModel), typeof(EnabledStringSettingViewModel), typeof(IntegerSettingViewModel), typeof(EnabledIntegerSettingViewModel),
    typeof(DoubleSettingViewModel), typeof(EnabledDoubleSettingViewModel), typeof(PatternSettingViewModel), typeof(EnabledPatternSettingViewModel), typeof(TimestampSettingViewModel)
)]
[NodeTypeMapping(typeof(ParentSetting), typeof(ParentSettingViewModel))]
[NodeTypeMapping(typeof(BooleanSetting), typeof(BooleanSettingViewModel))]
[NodeTypeMapping(typeof(EnabledOptionSetting), typeof(EnabledOptionSettingViewModel))]
[NodeTypeMapping(typeof(OptionSetting), typeof(OptionSettingViewModel))]
[NodeTypeMapping(typeof(EnabledStringSetting), typeof(EnabledStringSettingViewModel))]
[NodeTypeMapping(typeof(StringSetting), typeof(StringSettingViewModel))]
[NodeTypeMapping(typeof(EnabledIntegerSetting), typeof(EnabledIntegerSettingViewModel))]
[NodeTypeMapping(typeof(IntegerSetting), typeof(IntegerSettingViewModel))]
[NodeTypeMapping(typeof(EnabledDoubleSetting), typeof(EnabledDoubleSettingViewModel))]
[NodeTypeMapping(typeof(DoubleSetting), typeof(DoubleSettingViewModel))]
[NodeTypeMapping(typeof(EnabledPatternSetting), typeof(EnabledPatternSettingViewModel))]
[NodeTypeMapping(typeof(PatternSetting), typeof(PatternSettingViewModel))]
[NodeTypeMapping(typeof(TimestampSetting), typeof(TimestampSettingViewModel))]
public partial class ParentSettingViewModel : ParentSetting
{
    readonly Dictionary<string, SettingNode> _nodeCache;

    public override IList<SettingNode> Nodes
    {
        get => base.Nodes;
        set {
            if (value is null)
            {
                base.Nodes = [];
            } 
            else
            {
                base.Nodes = new NodeObservableCollection<ParentSettingViewModel, SettingNode>(value);
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLeaf));
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

    public bool IsLeaf
    {
        get => base.Nodes.Count == 0;
        set { }
    }

    public ICollection<SettingNode> Children
    {
        get => base.Nodes;
        set { }
    }

    static ParentSettingViewModel()
    {
    }

    public ParentSettingViewModel()
    {
        base.Nodes = [];
        _nodeCache = [];
    }

    public override SettingNode this[string key]
    {
        get
        {
            // Note: cache does not automatically invalidate
            if (!_nodeCache.ContainsKey(key))
            {
                var child = base.Nodes.FirstOrDefault(n => n.Name == key) ?? throw new ArgumentException($"Invalid key: {key}");
                _nodeCache.Add(key, child);
            }

            return _nodeCache[key];
        }
    }

    public void ResetDictionary()
    {
        _nodeCache.Clear();
    }
}

[ObservableObject]
public partial class BooleanSettingViewModel : BooleanSetting
{
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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override bool Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }
}

[ObservableObject]
public partial class OptionSettingViewModel : OptionSetting
{
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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override string Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }

    public override ICollection<String> Options
    {
        get => base.Options;
        set
        {
            base.Options = value;
            OnPropertyChanged();
        }
    }
}

[ObservableObject]
public partial class EnabledOptionSettingViewModel : EnabledOptionSetting
{
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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override string Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }

    public override ICollection<String> Options
    {
        get => base.Options;
        set
        {
            base.Options = value;
            OnPropertyChanged();
        }
    }

    public override bool IsEnabled
    {
        get => base.IsEnabled;
        set
        {
            var doSave = base.IsEnabled != value;
            base.IsEnabled = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }
}

[ObservableObject]
public partial class StringSettingViewModel : StringSetting
{
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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override string Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }
}

[ObservableObject]
public partial class EnabledStringSettingViewModel : EnabledStringSetting
{
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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override string Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }

    public override bool IsEnabled
    {
        get => base.IsEnabled;
        set
        {
            var doSave = base.IsEnabled != value;
            base.IsEnabled = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }
}

[ObservableObject]
public partial class IntegerSettingViewModel : IntegerSetting
{
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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override int Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }

    public override int Increment
    {
        get => base.Increment;
        set
        {
            base.Increment = value;
            OnPropertyChanged();
        }
    }
}

[ObservableObject]
public partial class EnabledIntegerSettingViewModel : EnabledIntegerSetting
{
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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override int Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
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

    public override int Increment
    {
        get => base.Increment;
        set
        {
            base.Increment = value;
            OnPropertyChanged();
        }
    }

    public override bool IsEnabled
    {
        get => base.IsEnabled;
        set
        {
            var doSave = base.IsEnabled != value;
            base.IsEnabled = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }
}

[ObservableObject]
public partial class DoubleSettingViewModel : DoubleSetting
{
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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override double Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }

    public override double Increment
    {
        get => base.Increment;
        set
        {
            base.Increment = value;
            OnPropertyChanged();
        }
    }
}

[ObservableObject]
public partial class EnabledDoubleSettingViewModel : EnabledDoubleSetting
{
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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override double Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
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

    public override double Increment
    {
        get => base.Increment;
        set
        {
            base.Increment = value;
            OnPropertyChanged();
        }
    }

    public override bool IsEnabled
    {
        get => base.IsEnabled;
        set
        {
            var doSave = base.IsEnabled != value;
            base.IsEnabled = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }
}

[ObservableObject]
public partial class PatternSettingViewModel : PatternSetting
{
    static readonly IMapper _mapper;

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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override PatternNode Value
    {
        get => base.Value;
        set
        {
            base.Value = _mapper.Map<PatternNodeViewModel>(value);
            OnPropertyChanged();
        }
    }

    static PatternSettingViewModel()
    {
        _mapper = ServiceHelper.GetService<IMapper>();
    }
}

[ObservableObject]
public partial class EnabledPatternSettingViewModel : EnabledPatternSetting
{
    static readonly IMapper _mapper;

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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override PatternNode Value
    {
        get => base.Value;
        set
        {
            base.Value = _mapper.Map<PatternNodeViewModel>(value);
            OnPropertyChanged();
        }
    }

    static EnabledPatternSettingViewModel()
    {
        _mapper = ServiceHelper.GetService<IMapper>();
    }

    public override bool IsEnabled
    {
        get => base.IsEnabled;
        set
        {
            var doSave = base.IsEnabled != value;
            base.IsEnabled = value;
            OnPropertyChanged();
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }
}

[ObservableObject]
public partial class TimestampSettingViewModel : TimestampSetting
{

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

    public bool IsLeaf { get; set; } = true;
    public object Children { get; set; }        // I don't know why this is always binded in UraniumUI treeview

    public override DateTimeOffset Value
    {
        get => base.Value;
        set
        {
            var doSave = base.Value != value;
            base.Value = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LocalValue));
            if (doSave) WeakReferenceMessenger.Default.Send<SettingNode>(this);
        }
    }
}