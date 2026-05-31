using LR4.Core.Lazy;
using LR4.Core.Sequences;

namespace LR4.Tests;

public class LazySequenceTests
{
    [Fact]
    public void Fibonacci_FirstValues_AreCorrect()
    {
        var fib = LazySequence<int>.Fibonacci();
        Assert.Equal(0, fib.Get(0));
        Assert.Equal(1, fib.Get(1));
        Assert.Equal(1, fib.Get(2));
        Assert.Equal(2, fib.Get(3));
        Assert.Equal(3, fib.Get(4));
    }

    [Fact]
    public void Get_Memoizes_Elements()
    {
        var fib = LazySequence<int>.Fibonacci();
        _ = fib.Get(10);
        Assert.True(fib.GetMaterializedCount() >= 11);
        Assert.Equal(55, fib.Get(10));
    }

    [Fact]
    public void FromSequence_ReadsElements()
    {
        var seq = new ArraySequence<string>(new[] { "a", "b", "c" });
        var lazy = new LazySequence<string>(seq);
        Assert.Equal("b", lazy.Get(1));
        Assert.Equal(3, lazy.GetLength().ToInt());
    }

    [Fact]
    public void Generator_ReadsUntilEnd()
    {
        var lazy = new LazySequence<int>(new ArraySequence<int>(new[] { 1, 2 }));
        var gen = lazy.Generator;
        gen.Reset();
        Assert.True(gen.HasNext());
        Assert.Equal(1, gen.GetNext());
        Assert.Equal(2, gen.GetNext());
        Assert.False(gen.HasNext());
    }
}
