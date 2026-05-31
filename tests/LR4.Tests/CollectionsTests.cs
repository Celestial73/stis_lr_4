using LR4.Core.Collections;

namespace LR4.Tests;

public class CollectionsTests
{
    [Fact]
    public void BinaryHeap_ReturnsMinFirst()
    {
        var heap = new BinaryHeap<int>();
        heap.Enqueue(5);
        heap.Enqueue(1);
        heap.Enqueue(3);
        Assert.Equal(1, heap.Dequeue());
        Assert.Equal(3, heap.Dequeue());
    }

    [Fact]
    public void Bst_InOrder_IsSorted()
    {
        var tree = new BinarySearchTree<int>();
        tree.Insert(3);
        tree.Insert(1);
        tree.Insert(2);
        Assert.Equal(new[] { 1, 2, 3 }, tree.InOrder().ToArray());
        Assert.True(tree.Contains(2));
    }
}
