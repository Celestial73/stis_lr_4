using LR4.Core.Sequences;

namespace LR4.Core.Streams;

public abstract class WriteOnlyStream<T>
{
    public abstract void Open();
    public abstract void Close();
    public abstract int Write(T item);
    public abstract int GetPosition();

    public bool IsOpen { get; protected set; }

    protected void EnsureOpen()
    {
        if (!IsOpen)
            throw new InvalidOperationException("Stream is not open.");
    }
}

public sealed class SequenceWriteOnlyStream<T> : WriteOnlyStream<T>
{
    private ArraySequence<T> _sequence = new();

    public Sequence<T> BuiltSequence => _sequence;

    public override void Open() => IsOpen = true;
    public override void Close() => IsOpen = false;

    public override int Write(T item)
    {
        EnsureOpen();
        _sequence = (ArraySequence<T>)_sequence.Append(item);
        return _sequence.GetLength().ToInt();
    }

    public override int GetPosition()
    {
        EnsureOpen();
        return _sequence.GetLength().ToInt();
    }
}

public sealed class FileWriteOnlyStream<T> : WriteOnlyStream<T>
{
    private readonly string _path;
    private readonly ISerializer<T> _serializer;
    private StreamWriter? _writer;
    private int _position;

    public FileWriteOnlyStream(string path, ISerializer<T> serializer)
    {
        _path = path;
        _serializer = serializer;
    }

    public override void Open()
    {
        _writer = new StreamWriter(_path, append: false);
        _position = 0;
        IsOpen = true;
    }

    public override void Close()
    {
        _writer?.Dispose();
        _writer = null;
        IsOpen = false;
    }

    public override int Write(T item)
    {
        EnsureOpen();
        _writer!.WriteLine(_serializer.Serialize(item));
        _position++;
        return _position;
    }

    public override int GetPosition()
    {
        EnsureOpen();
        return _position;
    }
}
