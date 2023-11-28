using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Experimental.TerrainAPI;

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}

// Note: The CompareTo method returns a priority
//       higher priority = 1
//       equal priority = 0   
//       lower priority = -1
public class Heap<T> where T : IHeapItem<T>
{
    T[] _items;
    int _itemsCount;
    public int Count => _itemsCount;

    public Heap(int maxSize)
    {
        _items = new T[maxSize];
    }

    public bool Contains(T item)
    {
        if (item.HeapIndex < _itemsCount)
            return Equals(_items[item.HeapIndex], item);
        else
            return false;
    }

    public void Clear()
    {
        _itemsCount = 0;
    }

    public void Add(T item)
    {
        // Add the item to the end of the list
        item.HeapIndex = _itemsCount;
        _items[_itemsCount] = item;
        
        SortUp(item);
        _itemsCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = _items[0];
        _itemsCount--;
        _items[0] = _items[_itemsCount];
        _items[0].HeapIndex = 0;
        SortDown(_items[0]);
        return firstItem;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    /// <summary>
    /// Swaps item with its parents until it reaches a parent with a lower priority
    /// </summary>
    /// <param name="item">The item to move up the heap</param>
    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex-1)/2;

        while(true)
        {
            T parentItem = _items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
                Swap(item, parentItem);
            else
                break;
            parentIndex = (item.HeapIndex-1)/2;
        }
    }

    /// <summary>
    /// Swaps item with its children until it reaches children with higher priority
    /// </summary>
    /// <param name="item">The item to move down the heap</param>
    void SortDown(T item)
    {
        while (true)
        {
            int childItemToSwapIndex;
            int leftChildIndex = item.HeapIndex * 2 + 1;
            int rightChildIndex = item.HeapIndex * 2 + 2;

            if (isItemExists(leftChildIndex))
            {
                childItemToSwapIndex = leftChildIndex;

                if (isItemExists(rightChildIndex))
                {
                    if (_items[leftChildIndex].CompareTo(_items[rightChildIndex]) < 0)
                        childItemToSwapIndex = rightChildIndex;
                }

                if (item.CompareTo(_items[childItemToSwapIndex]) < 0)
                    Swap(item, _items[childItemToSwapIndex]);
                else
                    return;
            }
            else
                return;
        }

        bool isItemExists(int itemIndex)
        {
            return itemIndex < _itemsCount;
        }
    }

    void Swap(T itemA, T itemB)
    {
        _items[itemA.HeapIndex] = itemB;
        _items[itemB.HeapIndex] = itemA;
    
        int auxHeapIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = auxHeapIndex;
    }
}
