using Microsoft.Maui.Adapters;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using static System.Collections.Specialized.BitVector32;

namespace YeetMacro2.ViewModels;

// From chagGPT question: Can we make an observable collection that will sort itself?
public class SortedObservableCollection<T> : ObservableCollection<T>, IVirtualListViewAdapter
//public class SortedObservableCollection<T>(Comparison<T> comparer) : ObservableCollection<T>
{
    private readonly Comparer<T> _comparer;
    private bool disposedValue;

    public event EventHandler OnDataInvalidated;
    public SortedObservableCollection(Comparison<T> comparer)
    {
        _comparer = Comparer<T>.Create(comparer);
        this.CollectionChanged += My_CollectionChanged;
    }

    private void My_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        ((IVirtualListViewAdapter)this).InvalidateData();
    }

    public int GetNumberOfSections()
    {
        return (GetNumberOfItemsInSection(0) > 0) ? 1 : 0;
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

    public void OnCollectionReset()
    {
        var children = this.ToArray();
        this.ClearItems();
        foreach (var child in children)
        {
            this.Add(child);
        }
    }

    public virtual object GetSection(int sectionIndex)
    {
        return default(object);
    }

    object IVirtualListViewAdapter.GetItem(int sectionIndex, int itemIndex)
    {
        return GetItem(sectionIndex, itemIndex);
    }

    object IVirtualListViewAdapter.GetSection(int sectionIndex)
    {
        return GetSection(sectionIndex);
    }

    public int GetNumberOfItemsInSection(int sectionIndex)
    {
        return Items.Count;
    }

    public object GetItem(int sectionIndex, int itemIndex)
    {
        return Items[itemIndex];
    }

    public void InvalidateData()
    {
        this.OnDataInvalidated?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                this.CollectionChanged -= My_CollectionChanged;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}