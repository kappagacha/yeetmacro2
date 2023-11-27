using AutoMapper;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

public class SortedNodeObservableCollection<TViewModel, T> : SortedObservableCollection<T>
    where TViewModel : T
{
    static IMapper _mapper;
    static SortedNodeObservableCollection()
    {
        _mapper = ServiceHelper.GetService<IMapper>();
    }

    public SortedNodeObservableCollection(Comparison<T> comparer) : base(comparer)
    {
    }

    public SortedNodeObservableCollection(IEnumerable<T> values, Comparison<T> comparer) : this(comparer)
    {
        if (values is null) return;
        foreach (var val in values)
        {
            this.Add(val);
        }
    }

    protected override void InsertItem(int index, T item)
    {
        if (item is not TViewModel)
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