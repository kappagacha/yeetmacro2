﻿using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

[ObservableObject]
public partial class PatternNodeViewModel : PatternNode
{
    Dictionary<string, PatternNodeViewModel> _nodeCache;
    public override ICollection<PatternNode> Nodes
    {
        get => base.Nodes;
        set {
            base.Nodes = new NodeObservableCollection<PatternNodeViewModel, PatternNode>(value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLeaf));
        }
    }

    public override ICollection<Pattern> Patterns
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
            base.IsMultiPattern = value;
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

    public ICollection<PatternNode> Children
    {
        get => base.Nodes;
    }

    public PatternNodeViewModel()
    {
        base.Nodes = new NodeObservableCollection<PatternNodeViewModel, PatternNode>();
        base.Patterns = new NodeObservableCollection<PatternViewModel, Pattern>();
        _nodeCache = new Dictionary<string, PatternNodeViewModel>();
    }

    public PatternNodeViewModel this[string key]
    {
        get
        {
            // Note: cache does not automatically invalidate
            if (!_nodeCache.ContainsKey(key))
            {
                var child = base.Nodes.FirstOrDefault(n => n.Name == key);
                if (child is null) throw new ArgumentException($"Invalid key: {key}");
                _nodeCache.Add(key, child as PatternNodeViewModel);
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
public partial class PatternViewModel : Pattern
{
    static IMapper _mapper;

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

    public bool IsLeaf
    {
        get => true;
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