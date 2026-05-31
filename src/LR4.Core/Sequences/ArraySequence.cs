using LR4.Core.Numeration;

namespace LR4.Core.Sequences;

public sealed class ArraySequence<T> : Sequence<T>
{
    private readonly List<T> _items;

    public ArraySequence()
    {
        _items = new List<T>();
    }

    public ArraySequence(IEnumerable<T> items)
    {
        _items = new List<T>(items);
    }

    public ArraySequence(T[] items) : this(items.AsEnumerable()) { }

    private ArraySequence(List<T> items)
    {
        _items = items;
    }

    public override Cardinal GetLength() => Cardinal.FromInt(_items.Count);

    public override T Get(int index)
    {
        ValidateIndex(index, GetLength());
        return _items[index];
    }

    public override Sequence<T> Set(int index, T item)
    {
        ValidateIndex(index, GetLength());
        var copy = new List<T>(_items) { [index] = item };
        return new ArraySequence<T>(copy);
    }

    public override Sequence<T> Append(T item)
    {
        var copy = new List<T>(_items) { item };
        return new ArraySequence<T>(copy);
    }

    public override Sequence<T> Prepend(T item)
    {
        var copy = new List<T>(_items);
        copy.Insert(0, item);
        return new ArraySequence<T>(copy);
    }

    public override Sequence<T> InsertAt(T item, int index)
    {
        ValidateIndex(index, Cardinal.FromInt(_items.Count + 1));
        var copy = new List<T>(_items);
        copy.Insert(index, item);
        return new ArraySequence<T>(copy);
    }

    public override Sequence<T> Concat(Sequence<T> other)
    {
        var result = new List<T>(_items);
        if (other is ArraySequence<T> arr)
        {
            result.AddRange(arr._items);
            return new ArraySequence<T>(result);
        }

        if (other.GetLength().IsFinite)
        {
            var n = other.GetLength().ToInt();
            for (var i = 0; i < n; i++)
                result.Add(other.Get(i));
        }
        else
        {
            throw new InvalidOperationException("Cannot concat with infinite sequence into ArraySequence.");
        }

        return new ArraySequence<T>(result);
    }

    public override Sequence<T> GetSubsequence(int startIndex, int endIndex)
    {
        ValidateRange(startIndex, endIndex, GetLength());
        return new ArraySequence<T>(_items.GetRange(startIndex, endIndex - startIndex + 1));
    }

    public IReadOnlyList<T> AsReadOnly() => _items;
}
