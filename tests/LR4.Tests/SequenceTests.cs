using LR4.Core.Numeration;
using LR4.Core.Sequences;

namespace LR4.Tests;

public class SequenceTests
{
    [Fact]
    public void Append_And_Get_Works()
    {
        Sequence<int> s = new ArraySequence<int>();
        s = s.Append(1).Append(2);
        Assert.Equal(2, s.GetLength().ToInt());
        Assert.Equal(1, s.Get(0));
        Assert.Equal(2, s.GetLast());
    }

    [Fact]
    public void Get_OutOfRange_Throws()
    {
        var s = new ArraySequence<int>(new[] { 1 });
        Assert.Throws<IndexOutOfRangeException>(() => s.Get(5));
    }

    [Fact]
    public void InsertAt_InMiddle_Works()
    {
        Sequence<int> s = new ArraySequence<int>(new[] { 1, 3 });
        s = s.InsertAt(2, 1);
        Assert.Equal(new[] { 1, 2, 3 }, ((ArraySequence<int>)s).AsReadOnly());
    }
}
