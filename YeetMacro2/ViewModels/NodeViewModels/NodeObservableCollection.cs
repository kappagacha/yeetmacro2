using AutoMapper;
using System.Collections.ObjectModel;
using YeetMacro2.Services;

namespace YeetMacro2.ViewModels.NodeViewModels;

public class NodeObservableCollection<TViewModel, T> : ObservableCollection<T>
    where TViewModel : T
{
    static IMapper _mapper;
    static NodeObservableCollection()
    {
        _mapper = ServiceHelper.GetService<IMapper>();
    }

    public NodeObservableCollection()
    {
    }

    public NodeObservableCollection(IEnumerable<T> values)
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