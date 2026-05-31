namespace LR4.Core.Collections;

public sealed class BinaryHeap<T>
{
    private readonly List<T> _data = new();
    private readonly Comparison<T> _comparison;

    public BinaryHeap(Comparison<T>? comparison = null)
    {
        _comparison = comparison ?? Comparer<T>.Default.Compare;
    }

    public int Count => _data.Count;
    public bool IsEmpty => _data.Count == 0;

    public void Enqueue(T item)
    {
        _data.Add(item);
        SiftUp(_data.Count - 1);
    }

    public T Dequeue()
    {
        if (_data.Count == 0)
            throw new InvalidOperationException("Heap is empty.");

        var root = _data[0];
        var last = _data[^1];
        _data.RemoveAt(_data.Count - 1);
        if (_data.Count > 0)
        {
            _data[0] = last;
            SiftDown(0);
        }

        return root;
    }

    public T Peek()
    {
        if (_data.Count == 0)
            throw new InvalidOperationException("Heap is empty.");
        return _data[0];
    }

    public void Clear() => _data.Clear();

    private void SiftUp(int index)
    {
        while (index > 0)
        {
            var parent = (index - 1) / 2;
            if (_comparison(_data[index], _data[parent]) >= 0)
                break;
            (_data[index], _data[parent]) = (_data[parent], _data[index]);
            index = parent;
        }
    }

    private void SiftDown(int index)
    {
        while (true)
        {
            var left = index * 2 + 1;
            var right = index * 2 + 2;
            var smallest = index;

            if (left < _data.Count && _comparison(_data[left], _data[smallest]) < 0)
                smallest = left;
            if (right < _data.Count && _comparison(_data[right], _data[smallest]) < 0)
                smallest = right;

            if (smallest == index)
                break;

            (_data[index], _data[smallest]) = (_data[smallest], _data[index]);
            index = smallest;
        }
    }
}
