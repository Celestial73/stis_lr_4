namespace LR4.Core.Lazy;

public enum DeferredOperationKind
{
    InsertOne,
    InsertMany,
    RemoveOne,
    RemoveMany
}

public sealed class DeferredOperation<T>
{
    public int Index { get; }
    public DeferredOperationKind Kind { get; }
    public T? SingleItem { get; }
    public IReadOnlyList<T>? ManyItems { get; }
    public int SequenceId { get; }

    public DeferredOperation(int index, DeferredOperationKind kind, T? single, IReadOnlyList<T>? many, int sequenceId)
    {
        Index = index;
        Kind = kind;
        SingleItem = single;
        ManyItems = many;
        SequenceId = sequenceId;
    }

    public static int CompareByIndex(DeferredOperation<T> a, DeferredOperation<T> b) =>
        a.Index.CompareTo(b.Index);
}
