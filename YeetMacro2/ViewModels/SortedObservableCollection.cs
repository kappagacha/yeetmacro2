using System.Collections.ObjectModel;

namespace YeetMacro2.ViewModels;

// From chagGPT question: Can we make an observable collection that will sort itself?
public class SortedObservableCollection<T> : ObservableCollection<T>
{
    private readonly IComparer<T> _comparer;

    public SortedObservableCollection(Comparison<T> comparer)
    {
        _comparer = Comparer<T>.Create(comparer);
    }

    protected override void InsertItem(int index, T item)
    {
        index = GetSortedIndex(item);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, T item)
    {
        RemoveAt(index);
        index = GetSortedIndex(item);
        base.InsertItem(index, item);
    }

    private int GetSortedIndex(T item)
    {
        int index = 0;
        while (index < Count && _comparer.Compare(item, this[index]) > 0)
        {
            index++;
        }
        return index;
    }
}