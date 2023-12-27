using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

public class SettingNodeViewModelMetadataProvider : INodeMetadataProvider<ParentSettingViewModel>
{
    public Expression<Func<ParentSettingViewModel, object>> CollectionPropertiesExpression => null;
    public Expression<Func<ParentSettingViewModel, object>> ProxyPropertiesExpression => null;

    public Type[] NodeTypes => new Type[] { 
        typeof(ParentSettingViewModel), typeof(BooleanSettingViewModel), typeof(OptionSettingViewModel), 
        typeof(StringSettingViewModel), typeof(IntegerSettingViewModel), typeof(EnabledIntegerSettingViewModel), 
        typeof(PatternSettingViewModel) 
    };
}

[ObservableObject]
[NodeMetadata(NodeMetadataProvider = typeof(SettingNodeViewModelMetadataProvider))]
public partial class ParentSettingViewModel : ParentSetting
{
    static IMapper _mapper;
    Dictionary<string, SettingNode> _nodeCache;

    public override ICollection<SettingNode> Nodes
    {
        get => base.Nodes;
        set {
            if (value is null)
            {
                base.Nodes = new ObservableCollection<SettingNode>();
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLeaf));
                return;
            }
            base.Nodes = new ObservableCollection<SettingNode>();
            foreach (var val in value)
            {
                if (val is ParentSetting)
                {
                    base.Nodes.Add(_mapper.Map<ParentSettingViewModel>(val));
                }
                else if (val is BooleanSetting)
                {
                    base.Nodes.Add(_mapper.Map<BooleanSettingViewModel>(val));
                }
                else if (val is OptionSetting)
                {
                    base.Nodes.Add(_mapper.Map<OptionSettingViewModel>(val));
                }
                else if (val is StringSetting)
                {
                    base.Nodes.Add(_mapper.Map<StringSettingViewModel>(val));
                }
                else if (val is EnabledIntegerSetting)
                {
                    base.Nodes.Add(_mapper.Map<EnabledIntegerSettingViewModel>(val));
                }
                else if (val is IntegerSetting)
                {
                    base.Nodes.Add(_mapper.Map<IntegerSettingViewModel>(val));
                }
                else if (val is PatternSetting)
                {
                    base.Nodes.Add(_mapper.Map<PatternSettingViewModel>(val));
                }
                else
                {
                    throw new Exception($"Unhandled view model type mapping: {val.GetType().Name}");
                }
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
        _mapper = ServiceHelper.GetService<IMapper>();
    }

    public ParentSettingViewModel()
    {
        base.Nodes = new ObservableCollection<SettingNode>();
        _nodeCache = new Dictionary<string, SettingNode>();
    }

    public override SettingNode this[string key]
    {
        get
        {
            // Note: cache does not automatically invalidate
            if (!_nodeCache.ContainsKey(key))
            {
                var child = base.Nodes.FirstOrDefault(n => n.Name == key);
                if (child is null) throw new ArgumentException($"Invalid key: {key}");
                _nodeCache.Add(key, child);
            }

            return _nodeCache[key];
        }
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
public partial class PatternSettingViewModel : PatternSetting
{
    static IMapper _mapper;

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

