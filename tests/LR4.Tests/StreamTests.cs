using LR4.Core.Events;
using LR4.Core.Lazy;
using LR4.Core.Sequences;
using LR4.Core.Streams;

namespace LR4.Tests;

public class StreamTests
{
    [Fact]
    public void SequenceStream_ReadsAll()
    {
        var seq = new ArraySequence<int>(new[] { 10, 20 });
        var stream = new SequenceReadOnlyStream<int>(seq);
        stream.Open();
        Assert.Equal(10, stream.Read());
        Assert.Equal(20, stream.Read());
        Assert.True(stream.IsEndOfStream());
        stream.Close();
    }

    [Fact]
    public void FileStream_RoundTrip_EventJson()
    {
        var ser = new EventJsonSerializer();
        var path = Path.Combine(Path.GetTempPath(), $"lr4_evt_{Guid.NewGuid():N}.jsonl");
        try
        {
            var write = new FileWriteOnlyStream<EventRecord>(path, ser);
            write.Open();
            write.Write(new EventRecord("Tick", null, DateTime.UtcNow));
            write.Close();

            var read = new FileReadOnlyStream<EventRecord>(path, ser);
            read.Open();
            var ev = read.Read();
            read.Close();
            Assert.Equal("Tick", ev.Type);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void LazyStream_EndsOnFiniteSequence()
    {
        var lazy = new LazySequence<int>(new ArraySequence<int>(new[] { 1 }));
        var stream = new LazySequenceReadOnlyStream<int>(lazy);
        stream.Open();
        Assert.Equal(1, stream.Read());
        Assert.True(stream.IsEndOfStream());
        stream.Close();
    }
}
