using LR4.Core.Sequences;

namespace LR4.Core.Lazy;

public sealed class Generator<T>
{
    private readonly LazySequence<T> _owner;
    private int _nextIndex;
    private bool _finished;

    internal Generator(LazySequence<T> owner)
    {
        _owner = owner;
    }

    public bool HasNext()
    {
        if (_finished)
            return false;
        if (_owner.IsInfinite)
            return true;
        return _nextIndex < _owner.GetLength().ToInt();
    }

    public T GetNext()
    {
        if (!HasNext())
            throw new IndexOutOfRangeException("No more elements in sequence.");
        return _owner.MaterializeAt(_nextIndex++);
    }

    public bool TryGetNext(out T value)
    {
        if (!HasNext())
        {
            value = default!;
            return false;
        }

        value = GetNext();
        return true;
    }

    public void Reset()
    {
        _nextIndex = 0;
        _finished = false;
    }

    internal void MarkFinished() => _finished = true;
}
