namespace LR4.Core.Collections;

public sealed class BinarySearchTree<T> where T : notnull
{
    private readonly IComparer<T> _comparer;
    private Node? _root;

    private sealed class Node
    {
        public T Value;
        public Node? Left;
        public Node? Right;

        public Node(T value) => Value = value;
    }

    public BinarySearchTree(IComparer<T>? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public void Insert(T value) => _root = Insert(_root, value);

    public bool Contains(T value) => Find(_root, value) is not null;

    public IEnumerable<T> InOrder()
    {
        foreach (var v in InOrder(_root))
            yield return v;
    }

    private Node? Insert(Node? node, T value)
    {
        if (node is null)
            return new Node(value);

        var cmp = _comparer.Compare(value, node.Value);
        if (cmp < 0)
            node.Left = Insert(node.Left, value);
        else if (cmp > 0)
            node.Right = Insert(node.Right, value);

        return node;
    }

    private Node? Find(Node? node, T value)
    {
        while (node is not null)
        {
            var cmp = _comparer.Compare(value, node.Value);
            if (cmp == 0)
                return node;
            node = cmp < 0 ? node.Left : node.Right;
        }

        return null;
    }

    private static IEnumerable<T> InOrder(Node? node)
    {
        if (node is null)
            yield break;
        foreach (var v in InOrder(node.Left))
            yield return v;
        yield return node.Value;
        foreach (var v in InOrder(node.Right))
            yield return v;
    }
}
