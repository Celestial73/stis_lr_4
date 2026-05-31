namespace LR4.Core.Lazy;

internal sealed class FixedRingBuffer<T>
{
    private readonly T[] _buffer;
    private int _count;
    private int _start;

    public FixedRingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        _buffer = new T[capacity];
    }

    public int Count => _count;
    public int Capacity => _buffer.Length;

    public void Add(T item)
    {
        if (_count < _buffer.Length)
        {
            _buffer[_count] = item;
            _count++;
        }
        else
        {
            _buffer[_start] = item;
            _start = (_start + 1) % _buffer.Length;
        }
    }

    public IReadOnlyList<T> Snapshot()
    {
        var list = new List<T>(_count);
        for (var i = 0; i < _count; i++)
        {
            var idx = (_start + i) % _buffer.Length;
            list.Add(_buffer[idx]);
        }

        return list;
    }
}
