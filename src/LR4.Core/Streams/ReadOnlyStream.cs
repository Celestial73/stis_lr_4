using LR4.Core.Lazy;
using LR4.Core.Sequences;

namespace LR4.Core.Streams;

public abstract class ReadOnlyStream<T>
{
    public abstract void Open();
    public abstract void Close();
    public abstract bool IsEndOfStream();
    public abstract T Read();
    public abstract int GetPosition();
    public abstract bool IsCanSeek();
    public abstract bool IsCanGoBack();
    public abstract int Seek(int index);

    public bool IsOpen { get; protected set; }

    protected void EnsureOpen()
    {
        if (!IsOpen)
            throw new InvalidOperationException("Stream is not open.");
    }
}

public sealed class SequenceReadOnlyStream<T> : ReadOnlyStream<T>
{
    private readonly Sequence<T> _sequence;
    private int _position;

    public SequenceReadOnlyStream(Sequence<T> sequence)
    {
        _sequence = sequence;
    }

    public override void Open() => IsOpen = true;
    public override void Close() => IsOpen = false;

    public override bool IsEndOfStream()
    {
        EnsureOpen();
        return _position >= _sequence.GetLength().ToInt();
    }

    public override T Read()
    {
        EnsureOpen();
        if (IsEndOfStream())
            throw new EndOfStreamException();
        return _sequence.Get(_position++);
    }

    public override int GetPosition()
    {
        EnsureOpen();
        return _position;
    }

    public override bool IsCanSeek() => true;
    public override bool IsCanGoBack() => true;

    public override int Seek(int index)
    {
        EnsureOpen();
        if (index < 0 || index > _sequence.GetLength().ToInt())
            throw new IndexOutOfRangeException($"Seek index {index} is invalid.");
        _position = index;
        return _position;
    }
}

public sealed class LazySequenceReadOnlyStream<T> : ReadOnlyStream<T>
{
    private readonly LazySequence<T> _sequence;
    private readonly Generator<T> _generator;

    public LazySequenceReadOnlyStream(LazySequence<T> sequence)
    {
        _sequence = sequence;
        _generator = sequence.Generator;
    }

    public override void Open()
    {
        IsOpen = true;
        _generator.Reset();
    }

    public override void Close() => IsOpen = false;

    public override bool IsEndOfStream()
    {
        EnsureOpen();
        return !_generator.HasNext();
    }

    public override T Read()
    {
        EnsureOpen();
        if (IsEndOfStream())
            throw new EndOfStreamException();
        return _generator.GetNext();
    }

    public override int GetPosition()
    {
        EnsureOpen();
        return _sequence.GetMaterializedCount();
    }

    public override bool IsCanSeek() => false;
    public override bool IsCanGoBack() => false;

    public override int Seek(int index) =>
        throw new NotSupportedException("Cannot seek lazy sequence stream.");
}

public sealed class FileReadOnlyStream<T> : ReadOnlyStream<T>
{
    private readonly string _path;
    private readonly IDeserializer<T> _deserializer;
    private StreamReader? _reader;
    private int _position;

    public FileReadOnlyStream(string path, IDeserializer<T> deserializer)
    {
        _path = path;
        _deserializer = deserializer;
    }

    public override void Open()
    {
        _reader = new StreamReader(_path);
        _position = 0;
        IsOpen = true;
    }

    public override void Close()
    {
        _reader?.Dispose();
        _reader = null;
        IsOpen = false;
    }

    public override bool IsEndOfStream()
    {
        EnsureOpen();
        return _reader!.EndOfStream;
    }

    public override T Read()
    {
        EnsureOpen();
        if (IsEndOfStream())
            throw new EndOfStreamException();
        var line = _reader!.ReadLine() ?? throw new EndOfStreamException();
        _position++;
        return _deserializer.Deserialize(line);
    }

    public override int GetPosition()
    {
        EnsureOpen();
        return _position;
    }

    public override bool IsCanSeek() => false;
    public override bool IsCanGoBack() => false;
    public override int Seek(int index) =>
        throw new NotSupportedException("Cannot seek file stream.");
}

public sealed class StringReadOnlyStream<T> : ReadOnlyStream<T>
{
    private readonly Queue<string> _lines;
    private readonly IDeserializer<T> _deserializer;
    private int _position;

    public StringReadOnlyStream(string content, IDeserializer<T> deserializer)
    {
        _deserializer = deserializer;
        _lines = new Queue<string>(
            content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
    }

    public override void Open() => IsOpen = true;
    public override void Close() => IsOpen = false;

    public override bool IsEndOfStream()
    {
        EnsureOpen();
        return _lines.Count == 0;
    }

    public override T Read()
    {
        EnsureOpen();
        if (IsEndOfStream())
            throw new EndOfStreamException();
        _position++;
        return _deserializer.Deserialize(_lines.Dequeue());
    }

    public override int GetPosition()
    {
        EnsureOpen();
        return _position;
    }

    public override bool IsCanSeek() => false;
    public override bool IsCanGoBack() => false;
    public override int Seek(int index) =>
        throw new NotSupportedException("Cannot seek string stream.");
}
