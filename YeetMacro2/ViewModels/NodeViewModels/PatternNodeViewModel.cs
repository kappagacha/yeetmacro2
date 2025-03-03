using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Specialized;
using System.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class PatternNodeViewModel : PatternNode
{
    readonly Dictionary<string, PatternNodeViewModel> _nodeCache;
    public override IList<PatternNode> Nodes
    {
        get => base.Nodes;
        set {
            var nodes = new NodeObservableCollection<PatternNodeViewModel, PatternNode>(value);
            nodes.CollectionChanged += Nodes_CollectionChanged;
            base.Nodes = nodes;
            Nodes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, nodes));
            OnPropertyChanged();
        }
    }

    public override IList<Pattern> Patterns
    {
        get => base.Patterns;
        set
        {
            base.Patterns = new NodeObservableCollection<PatternViewModel, Pattern>(value);
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

    public override bool IsMultiPattern
    {
        get => base.IsMultiPattern;
        set
        {
            var original = base.IsMultiPattern;
            base.IsMultiPattern = value;
            OnPropertyChanged();

            if (original != value)
            {
                WeakReferenceMessenger.Default.Send(this);
            }
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

    public PatternNodeViewModel()
    {
        var nodes = new NodeObservableCollection<PatternNodeViewModel, PatternNode>();
        nodes.CollectionChanged += Nodes_CollectionChanged;
        base.Nodes = nodes;
        base.Patterns = new NodeObservableCollection<PatternViewModel, Pattern>();
        _nodeCache = new Dictionary<string, PatternNodeViewModel>();
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

    public PatternNodeViewModel this[string key]
    {
        get
        {
            if (!_nodeCache.ContainsKey(key))
            {
                var child = base.Nodes.FirstOrDefault(n => n.Name == key) ?? throw new ArgumentException($"Invalid key: {key}");
                _nodeCache.Add(key, child as PatternNodeViewModel);
            }
            else if (!base.Nodes.Contains(_nodeCache[key]))
            {
                _nodeCache.Remove(key);
                var child = base.Nodes.FirstOrDefault(n => n.Name == key) ?? throw new ArgumentException($"Invalid key: {key}");
                _nodeCache.Add(key, child as PatternNodeViewModel);
            }
            
            return _nodeCache[key];
        }
    }

    public void ResetDictionary()
    {
        _nodeCache.Clear();
        foreach (PatternNodeViewModel child in Nodes)
        {
            child.ResetDictionary();
        }
    }
}

[ObservableObject]
public partial class PatternViewModel : Pattern
{
    static readonly IMapper _mapper;

    public override Rect Rect
    {
        get => base.Rect;
        set
        {
            base.Rect = value;
            OnPropertyChanged();
        }
    }

    public override Size Resolution
    {
        get => base.Resolution;
        set
        {
            base.Resolution = value;
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

    public override bool IsBoundsPattern
    {
        get => base.IsBoundsPattern;
        set
        {
            base.IsBoundsPattern = value;
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

    public override double VariancePct
    {
        get => base.VariancePct;
        set
        {
            base.VariancePct = value;
            OnPropertyChanged();
        }
    }

    public override TextMatchProperties TextMatch
    {
        get => base.TextMatch;
        set
        {
            base.TextMatch = _mapper.Map<TextMatchPropertiesViewModel>(value);
            OnPropertyChanged();
        }
    }

    public override ColorThresholdProperties ColorThreshold
    {
        get => base.ColorThreshold;
        set
        {
            base.ColorThreshold = _mapper.Map<ColorThresholdPropertiesViewModel>(value);
            OnPropertyChanged();
        }
    }

    public override OffsetCalcType OffsetCalcType
    {
        get => base.OffsetCalcType;
        set
        {
            base.OffsetCalcType = value;
            OnPropertyChanged();
        }
    }

    static PatternViewModel()
    {
        _mapper = ServiceHelper.GetService<IMapper>();
    }
}

[ObservableObject]
public partial class TextMatchPropertiesViewModel: TextMatchProperties
{
    public override bool IsActive
    {
        get => base.IsActive;
        set
        {
            base.IsActive = value;
            OnPropertyChanged();
        }
    }

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            OnPropertyChanged();
        }
    }

    public override string WhiteList
    {
        get => base.WhiteList;
        set
        {
            base.WhiteList = value;
            OnPropertyChanged();
        }
    }
}

[ObservableObject]
public partial class ColorThresholdPropertiesViewModel : ColorThresholdProperties
{
    public override bool IsActive
    {
        get => base.IsActive;
        set
        {
            base.IsActive = value;
            OnPropertyChanged();
        }
    }

    public override double VariancePct
    {
        get => base.VariancePct;
        set
        {
            base.VariancePct = value;
            OnPropertyChanged();
        }
    }

    public override string Color
    {
        get => base.Color;
        set
        {
            base.Color = value;
            OnPropertyChanged();
        }
    }
}