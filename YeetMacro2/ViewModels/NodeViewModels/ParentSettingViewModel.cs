using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

public class SettingNodeViewModelMetadataProvider : INodeMetadataProvider<ParentSettingViewModel>
{
    public Expression<Func<ParentSettingViewModel, object>> CollectionPropertiesExpression => null;
    public Expression<Func<ParentSettingViewModel, object>> ProxyPropertiesExpression => null;

    public Type[] NodeTypes => new Type[] { typeof(ParentSettingViewModel), typeof(BooleanSettingViewModel), typeof(OptionSettingViewModel), typeof(StringSettingViewModel), typeof(PatternSettingViewModel) };
}

[ObservableObject]
[NodeMetadata(NodeMetadataProvider = typeof(SettingNodeViewModelMetadataProvider))]
public partial class ParentSettingViewModel : ParentSetting
{
    static IMapper _mapper;
    ObservableCollection<SettingNode> _nodes;

    public override ICollection<SettingNode> Nodes
    {
        get => _nodes;
        set {
            if (value is null)
            {
                _nodes = new ObservableCollection<SettingNode>();
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLeaf));
                return;
            }
            _nodes = new ObservableCollection<SettingNode>();
            foreach (var val in value)
            {
                if (val is ParentSetting)
                {
                    _nodes.Add(_mapper.Map<ParentSettingViewModel>(val));
                }
                else if (val is BooleanSetting)
                {
                    _nodes.Add(_mapper.Map<BooleanSettingViewModel>(val));
                }
                else if (val is OptionSetting)
                {
                    _nodes.Add(_mapper.Map<OptionSettingViewModel>(val));
                }
                else if (val is StringSetting)
                {
                    _nodes.Add(_mapper.Map<StringSettingViewModel>(val));
                }
                else if (val is PatternSetting)
                {
                    _nodes.Add(_mapper.Map<PatternSettingViewModel>(val));
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
        get => _nodes.Count == 0;
    }

    public ObservableCollection<SettingNode> Children
    {
        get => _nodes;
    }

    static ParentSettingViewModel()
    {
        _mapper = ServiceHelper.GetService<IMapper>();
    }

    public ParentSettingViewModel()
    {
        _nodes= new ObservableCollection<SettingNode>();
    }
}

[ObservableObject]
public partial class BooleanSettingViewModel : BooleanSetting
{
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
            base.Value = value;
            OnPropertyChanged();
        }
    }
}

[ObservableObject]
public partial class OptionSettingViewModel : OptionSetting
{
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
            base.Value = value;
            OnPropertyChanged();
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
            base.Value = value;
            OnPropertyChanged();
        }
    }
}

[ObservableObject]
public partial class PatternSettingViewModel : PatternSetting
{
    static IMapper _mapper;

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

    static PatternSettingViewModel()
    {
        _mapper = ServiceHelper.GetService<IMapper>();
    }
}