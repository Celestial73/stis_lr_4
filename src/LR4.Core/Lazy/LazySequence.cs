using System.Linq;
using LR4.Core.Numeration;
using LR4.Core.Collections;
using LR4.Core.Sequences;

namespace LR4.Core.Lazy;

public sealed class LazySequence<T> : IEnumerable<T>
{
    private readonly LazySequence<T>? _parent;
    private readonly Sequence<T>? _finiteSource;
    private readonly IReadOnlyList<T>? _initialElements;
    private readonly Func<IReadOnlyList<T>, T>? _recurrentRule;
    private readonly int _recurrentOrder;
    private readonly Cardinal _length;
    private readonly bool _isInfinite;
    private readonly List<DeferredOperation<T>> _deferredOps = new();
    private readonly List<LazySequence<T>> _concatTail = new();

    private readonly List<T> _memo = new();
    private readonly Generator<T> _generator;
    private readonly FixedRingBuffer<T>? _recurrentWindow;
    private int _materializedCount;
    private int _opSequenceCounter;

    internal bool IsInfinite => _isInfinite;

    public LazySequence()
    {
        _length = Cardinal.FromInt(0);
        _generator = new Generator<T>(this);
    }

    public LazySequence(T[] items) : this(new ArraySequence<T>(items)) { }

    public LazySequence(Sequence<T> seq)
    {
        _finiteSource = seq;
        _length = seq.GetLength();
        _generator = new Generator<T>(this);
    }

    public LazySequence(Func<IReadOnlyList<T>, T> rule, IReadOnlyList<T> initialElements)
    {
        _recurrentRule = rule ?? throw new ArgumentNullException(nameof(rule));
        _initialElements = initialElements ?? throw new ArgumentNullException(nameof(initialElements));
        _recurrentOrder = Math.Max(1, initialElements.Count);
        _recurrentWindow = new FixedRingBuffer<T>(_recurrentOrder);
        foreach (var e in initialElements)
            _recurrentWindow.Add(e);

        _isInfinite = true;
        _length = Cardinal.Infinity;
        _generator = new Generator<T>(this);
    }

    private LazySequence(LazySequence<T> parent)
    {
        _parent = parent;
        _finiteSource = parent._finiteSource;
        _initialElements = parent._initialElements;
        _recurrentRule = parent._recurrentRule;
        _recurrentOrder = parent._recurrentOrder;
        _length = parent._length;
        _isInfinite = parent._isInfinite;
        _recurrentWindow = parent._recurrentWindow;
        _generator = parent._generator;
    }

    public Generator<T> Generator => _generator;

    public Cardinal GetLength() => _length;

    public int GetMaterializedCount() => _materializedCount;

    public T GetFirst()
    {
        if (_length.IsFinite && _length.FiniteValue == 0)
            throw new IndexOutOfRangeException("Sequence is empty.");
        return Get(0);
    }

    public T GetLast()
    {
        if (_isInfinite)
            throw new InvalidOperationException("Cannot get last element of infinite lazy sequence.");
        var len = GetLength().ToInt();
        if (len == 0)
            throw new IndexOutOfRangeException("Sequence is empty.");
        return Get(len - 1);
    }

    public T Get(int index)
    {
        if (index < 0)
            throw new IndexOutOfRangeException($"Index {index} is negative.");
        if (!_isInfinite && index >= GetLength().ToInt())
            throw new IndexOutOfRangeException($"Index {index} is out of range.");

        return MaterializeAt(index);
    }

    internal T MaterializeAt(int index)
    {
        while (_memo.Count <= index)
        {
            var nextIndex = _memo.Count;
            _memo.Add(ComputeRawAt(nextIndex));
            _materializedCount++;
        }

        return ApplyDeferredAt(index, _memo[index]);
    }

    private T ComputeRawAt(int index)
    {
        if (_finiteSource is not null)
            return _finiteSource.Get(index);

        if (_recurrentRule is not null)
        {
            if (_initialElements is not null && index < _initialElements.Count)
                return _initialElements[index];

            if (_recurrentWindow is null)
                throw new InvalidOperationException("Recurrent window is not configured.");

            var value = _recurrentRule(_recurrentWindow.Snapshot());
            _recurrentWindow.Add(value);
            return value;
        }

        if (_parent is not null)
            return _parent.MaterializeAt(index);

        throw new IndexOutOfRangeException("Cannot compute element.");
    }

    private T ApplyDeferredAt(int index, T value)
    {
        var heap = new BinaryHeap<DeferredOperation<T>>(DeferredOperation<T>.CompareByIndex);
        foreach (var op in CollectDeferredOps())
            heap.Enqueue(op);

        var ops = new List<DeferredOperation<T>>();
        while (!heap.IsEmpty)
            ops.Add(heap.Dequeue());

        foreach (var op in ops.Where(o => o.Index == index).OrderBy(o => o.SequenceId))
        {
            value = op.Kind switch
            {
                DeferredOperationKind.InsertOne => op.SingleItem!,
                DeferredOperationKind.RemoveOne => throw new InvalidOperationException("Element was removed."),
                _ => value
            };
        }

        return value;
    }

    private IEnumerable<DeferredOperation<T>> CollectDeferredOps()
    {
        if (_parent is not null)
        {
            foreach (var op in _parent.CollectDeferredOps())
                yield return op;
        }

        foreach (var op in _deferredOps)
            yield return op;
    }

    public LazySequence<T> Append(T item)
    {
        var child = new LazySequence<T>(this);
        var idx = _isInfinite ? int.MaxValue : GetLength().ToInt();
        child._deferredOps.Add(new DeferredOperation<T>(idx, DeferredOperationKind.InsertOne, item, null, ++_opSequenceCounter));
        return child;
    }

    public LazySequence<T> Prepend(T item)
    {
        var child = new LazySequence<T>(this);
        child._deferredOps.Add(new DeferredOperation<T>(0, DeferredOperationKind.InsertOne, item, null, ++_opSequenceCounter));
        return child;
    }

    public LazySequence<T> InsertAt(T item, int index)
    {
        if (index < 0)
            throw new IndexOutOfRangeException($"Index {index} is negative.");
        if (!_isInfinite && index > GetLength().ToInt())
            throw new IndexOutOfRangeException($"Index {index} is out of range.");

        var child = new LazySequence<T>(this);
        child._deferredOps.Add(new DeferredOperation<T>(index, DeferredOperationKind.InsertOne, item, null, ++_opSequenceCounter));
        return child;
    }

    public LazySequence<T> Concat(LazySequence<T> other)
    {
        var child = new LazySequence<T>(this);
        child._concatTail.Add(other);
        return child;
    }

    public LazySequence<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        var materialized = new ArraySequence<TResult>();
        foreach (var item in EnumerateWithLimit(10000))
            materialized = (ArraySequence<TResult>)materialized.Append(mapper(item));

        if (_isInfinite)
        {
            var seed = materialized.AsReadOnly().ToList();
            return new LazySequence<TResult>((prev) => mapper(Get(prev.Count)), seed);
        }

        return new LazySequence<TResult>(materialized);
    }

    public LazySequence<T> Where(Func<T, bool> predicate)
    {
        var result = new ArraySequence<T>();
        foreach (var item in EnumerateWithLimit(10000))
        {
            if (predicate(item))
                result = (ArraySequence<T>)result.Append(item);
        }

        return new LazySequence<T>(result);
    }

    public LazySequence<T> Zip(Sequence<T> other)
    {
        var len = Math.Min(
            _isInfinite ? int.MaxValue : GetLength().ToInt(),
            other.GetLength().IsFinite ? other.GetLength().ToInt() : int.MaxValue);

        var pairs = new ArraySequence<T>();
        for (var i = 0; i < len && i < 10000; i++)
            pairs = (ArraySequence<T>)pairs.Append(Get(i));

        return new LazySequence<T>(pairs);
    }

    public LazySequence<T> GetSubsequence(int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex < startIndex)
            throw new IndexOutOfRangeException("Invalid subsequence range.");

        var seq = new ArraySequence<T>();
        for (var i = startIndex; i <= endIndex; i++)
            seq = (ArraySequence<T>)seq.Append(Get(i));
        return new LazySequence<T>(seq);
    }

    public IEnumerator<T> GetEnumerator() => EnumerateWithLimit(int.MaxValue).GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private IEnumerable<T> EnumerateWithLimit(int max)
    {
        var count = 0;
        while (HasNextEnumerated(count) && count < max)
        {
            yield return Get(count);
            count++;
        }
    }

    private bool HasNextEnumerated(int index)
    {
        if (_isInfinite)
            return true;
        return index < GetLength().ToInt();
    }

    public static LazySequence<int> Fibonacci()
    {
        return new LazySequence<int>((prev) => prev.Count < 2 ? 1 : prev[^1] + prev[^2], new[] { 0, 1 });
    }
}
