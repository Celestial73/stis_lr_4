using LR4.Core.Numeration;

namespace LR4.Core.Sequences;

public abstract class Sequence<T>
{
    public abstract Cardinal GetLength();
    public abstract T Get(int index);
    public abstract Sequence<T> Set(int index, T item);
    public abstract Sequence<T> Append(T item);
    public abstract Sequence<T> Prepend(T item);
    public abstract Sequence<T> InsertAt(T item, int index);
    public abstract Sequence<T> Concat(Sequence<T> other);
    public abstract Sequence<T> GetSubsequence(int startIndex, int endIndex);

    public T GetFirst()
    {
        if (GetLength().IsFinite && GetLength().FiniteValue == 0)
            throw new IndexOutOfRangeException("Sequence is empty.");
        return Get(0);
    }

    public T GetLast()
    {
        var len = GetLength();
        if (len.IsFinite)
        {
            if (len.FiniteValue == 0)
                throw new IndexOutOfRangeException("Sequence is empty.");
            return Get((int)len.FiniteValue - 1);
        }

        throw new InvalidOperationException("Cannot get last element of an infinite sequence.");
    }

    protected static void ValidateIndex(int index, Cardinal length)
    {
        if (index < 0)
            throw new IndexOutOfRangeException($"Index {index} is negative.");
        if (length.IsFinite && index >= (int)length.FiniteValue)
            throw new IndexOutOfRangeException($"Index {index} is out of range (length {length}).");
    }

    protected static void ValidateRange(int start, int end, Cardinal length)
    {
        ValidateIndex(start, length);
        if (length.IsFinite && end >= (int)length.FiniteValue)
            throw new IndexOutOfRangeException($"End index {end} is out of range.");
        if (end < start)
            throw new ArgumentException("End index must be >= start index.");
    }
}
