using AutoMapper;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

public class NodeObservableCollection<TViewModel, T> : SortedObservableCollection<T>
    where TViewModel : T
    where T: ISortable
{
    static readonly IMapper _mapper;
    static NodeObservableCollection()
    {
        _mapper = ServiceHelper.GetService<IMapper>();
    }

    public NodeObservableCollection(): base((a, b) => a.Position - b.Position == 0 ? 1: a.Position - b.Position)
    {
    }

    public NodeObservableCollection(IEnumerable<T> values): this()
    {
        if (values is null) return;
        foreach (var val in values)
        {
            this.Add(val);
        }
    }

    public NodeObservableCollection(Comparison<T> comparer) : base(comparer)
    {
    }

    public NodeObservableCollection(IEnumerable<T> values, Comparison<T> comparer) : this(comparer)
    {
        if (values is null) return;
        foreach (var val in values)
        {
            this.Add(val);
        }
    }

    protected override void InsertItem(int index, T item)
    {
        var mappedTypes = NodeTypeMappingAttribute.GetMappedType<TViewModel>();
        if (mappedTypes is not null && mappedTypes.Count > 0)
        {
            if (mappedTypes.ContainsKey(item.GetType()))
            {
                var targetType = mappedTypes[item.GetType()];
                var mappedItem = _mapper.Map(item, item.GetType(), targetType);
                base.InsertItem(index, (T)mappedItem);
            }
            else
            {
                base.InsertItem(index, item);
            }
        }
        else if (item is not TViewModel)
        {
            var mappedItem = _mapper.Map<TViewModel>(item);
            base.InsertItem(index, mappedItem);
        } 
        else
        {
            base.InsertItem(index, item);
        }
    }
}