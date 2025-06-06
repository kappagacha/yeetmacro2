﻿using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Specialized;
using System.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
[NodeTypes(
    typeof(ParentSettingViewModel), typeof(EnabledParentSettingViewModel), typeof(BooleanSettingViewModel), typeof(OptionSettingViewModel), typeof(EnabledOptionSettingViewModel),
    typeof(StringSettingViewModel), typeof(EnabledStringSettingViewModel), typeof(IntegerSettingViewModel), typeof(EnabledIntegerSettingViewModel),
    typeof(DoubleSettingViewModel), typeof(EnabledDoubleSettingViewModel), typeof(PatternSettingViewModel), typeof(EnabledPatternSettingViewModel),
    typeof(TimestampSettingViewModel)
)]
[NodeTypeMapping(typeof(ParentSetting), typeof(ParentSettingViewModel))]
[NodeTypeMapping(typeof(EnabledParentSetting), typeof(EnabledParentSettingViewModel))]
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
            var nodes = value is null ? new NodeObservableCollection<ParentSettingViewModel, SettingNode>()
                : new NodeObservableCollection<ParentSettingViewModel, SettingNode>(value);
            nodes.CollectionChanged += Nodes_CollectionChanged;
            base.Nodes = nodes;
            Nodes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, nodes));
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
            OnPropertyChanged(nameof(NodesHeight));
        }
    }

    public override int Height => 20;

    public override int NodesHeight
    {
        get
        {
            if (!IsExpanded) return 0;

            return Nodes.Sum(n => n.Height + n.NodesHeight);
        }
    }

    static ParentSettingViewModel()
    {
    }

    public ParentSettingViewModel()
    {
        var nodes = new NodeObservableCollection<ParentSettingViewModel, SettingNode>();
        nodes.CollectionChanged += Nodes_CollectionChanged;
        base.Nodes = nodes;
        _nodeCache = new Dictionary<string, SettingNode>();
    }

    private void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (INotifyPropertyChanged node in e.NewItems)
            {
                node.PropertyChanged += Node_PropertyChanged;
            }
            OnPropertyChanged(nameof(NodesHeight));
        }

        if (e.OldItems is not null)
        {
            foreach (INotifyPropertyChanged node in e.OldItems)
            {
                node.PropertyChanged -= Node_PropertyChanged;
            }
            OnPropertyChanged(nameof(NodesHeight));
        }
    }

    private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodesHeight))
        {
            OnPropertyChanged(nameof(NodesHeight));
        }
    }

    public override SettingNode this[string key]
    {
        get
        {
            if (!_nodeCache.ContainsKey(key))
            {
                var child = base.Nodes.FirstOrDefault(n => n.Name == key) ?? throw new ArgumentException($"Invalid key: {key}");
                _nodeCache.Add(key, child);
            }
            else if (!base.Nodes.Contains(_nodeCache[key]))
            {
                _nodeCache.Remove(key);
                var child = base.Nodes.FirstOrDefault(n => n.Name == key) ?? throw new ArgumentException($"Invalid key: {key}");
                _nodeCache.Add(key, child);
            }

            return _nodeCache[key];
        }
    }

    public void ResetDictionary()
    {
        _nodeCache.Clear();
        foreach (var child in Nodes)
        {
            if (child is EnabledParentSettingViewModel parent)
            {
                parent.ResetDictionary();
            }
            else if (child is EnabledParentSettingViewModel enabledParent)
            {
                enabledParent.ResetDictionary();
            }
        }
    }
}

[ObservableObject]
[NodeTypes(
    typeof(ParentSettingViewModel), typeof(BooleanSettingViewModel), typeof(OptionSettingViewModel), typeof(EnabledOptionSettingViewModel),
    typeof(StringSettingViewModel), typeof(EnabledStringSettingViewModel), typeof(IntegerSettingViewModel), typeof(EnabledIntegerSettingViewModel),
    typeof(DoubleSettingViewModel), typeof(EnabledDoubleSettingViewModel), typeof(PatternSettingViewModel), typeof(EnabledPatternSettingViewModel),
    typeof(TimestampSettingViewModel), typeof(EnabledParentSettingViewModel)
)]
[NodeTypeMapping(typeof(ParentSetting), typeof(ParentSettingViewModel))]
[NodeTypeMapping(typeof(EnabledParentSetting), typeof(EnabledParentSettingViewModel))]
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
public partial class EnabledParentSettingViewModel : EnabledParentSetting
{

    readonly Dictionary<string, SettingNode> _nodeCache;

    public override IList<SettingNode> Nodes
    {
        get => base.Nodes;
        set
        {
            var nodes = value is null ? new NodeObservableCollection<ParentSettingViewModel, SettingNode>()
                : new NodeObservableCollection<ParentSettingViewModel, SettingNode>(value);
            nodes.CollectionChanged += Nodes_CollectionChanged;
            base.Nodes = nodes;
            Nodes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, nodes));
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
            OnPropertyChanged(nameof(NodesHeight));
        }
    }

    public override int Height => 25;

    public override int NodesHeight
    {
        get
        {
            if (!IsExpanded) return 0;

            return Nodes.Sum(n => n.Height + n.NodesHeight);
        }
    }

    static EnabledParentSettingViewModel()
    {
    }

    public EnabledParentSettingViewModel()
    {
        var nodes = new NodeObservableCollection<ParentSettingViewModel, SettingNode>();
        nodes.CollectionChanged += Nodes_CollectionChanged;
        base.Nodes = nodes;
        _nodeCache = new Dictionary<string, SettingNode>();
    }

    private void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (INotifyPropertyChanged node in e.NewItems)
            {
                node.PropertyChanged += Node_PropertyChanged;
            }
            OnPropertyChanged(nameof(NodesHeight));
        }

        if (e.OldItems is not null)
        {
            foreach (INotifyPropertyChanged node in e.OldItems)
            {
                node.PropertyChanged -= Node_PropertyChanged;
            }
            OnPropertyChanged(nameof(NodesHeight));
        }
    }

    private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodesHeight))
        {
            OnPropertyChanged(nameof(NodesHeight));
        }
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
        foreach (var child in Nodes)
        {
            if (child is EnabledParentSettingViewModel parent)
            {
                parent.ResetDictionary();
            }
            else if (child is EnabledParentSettingViewModel enabledParent)
            {
                enabledParent.ResetDictionary();
            }
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

    public override int Height => 25;
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
    public override int Height => 25;
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
    public override int Height => 25;
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
    public override int Height => 30;
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
    public override int Height => 30;
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
    public override int Height => 30;
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
    public override int Height => 30;
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
    public override int Height => 30;
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
    public override int Height => 30;
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

    public override PatternNode Value
    {
        get => base.Value;
        set
        {
            base.Value = _mapper.Map<PatternNodeViewModel>(value);
            OnPropertyChanged();
        }
    }

    public override int Height => 20;

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

    public override PatternNode Value
    {
        get => base.Value;
        set
        {
            base.Value = _mapper.Map<PatternNodeViewModel>(value);
            OnPropertyChanged();
        }
    }

    public override int Height => 20;

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

    public override int Height => 20;

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