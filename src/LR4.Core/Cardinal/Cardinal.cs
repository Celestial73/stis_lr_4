namespace LR4.Core.Numeration;

public readonly struct Cardinal : IEquatable<Cardinal>, IComparable<Cardinal>
{
    public static Cardinal Infinity { get; } = new(true, 0);

    private readonly bool _isInfinite;
    private readonly ulong _value;

    public Cardinal(ulong value)
    {
        _isInfinite = false;
        _value = value;
    }

    private Cardinal(bool isInfinite, ulong value)
    {
        _isInfinite = isInfinite;
        _value = value;
    }

    public bool IsInfinite => _isInfinite;
    public bool IsFinite => !_isInfinite;

    public ulong FiniteValue
    {
        get
        {
            if (_isInfinite)
                throw new InvalidOperationException("Cardinal is infinite.");
            return _value;
        }
    }

    public static Cardinal FromInt(int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));
        return new Cardinal((ulong)length);
    }

    public int ToInt()
    {
        if (_isInfinite)
            throw new InvalidOperationException("Cannot convert infinite cardinal to int.");
        if (_value > int.MaxValue)
            throw new OverflowException();
        return (int)_value;
    }

    public Cardinal AddOne()
    {
        if (_isInfinite)
            return Infinity;
        return new Cardinal(_value + 1);
    }

    public int CompareTo(Cardinal other)
    {
        if (_isInfinite && other._isInfinite) return 0;
        if (_isInfinite) return 1;
        if (other._isInfinite) return -1;
        return _value.CompareTo(other._value);
    }

    public bool Equals(Cardinal other) =>
        _isInfinite == other._isInfinite && _value == other._value;

    public override bool Equals(object? obj) => obj is Cardinal c && Equals(c);

    public override int GetHashCode() => HashCode.Combine(_isInfinite, _value);

    public override string ToString() => _isInfinite ? "∞" : _value.ToString();

    public static bool operator ==(Cardinal a, Cardinal b) => a.Equals(b);
    public static bool operator !=(Cardinal a, Cardinal b) => !a.Equals(b);
    public static bool operator <(Cardinal a, Cardinal b) => a.CompareTo(b) < 0;
    public static bool operator >(Cardinal a, Cardinal b) => a.CompareTo(b) > 0;
}
