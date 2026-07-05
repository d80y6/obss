namespace Obss.ProductCatalog.Application.Abstractions;

public readonly struct Optional<T>
{
    private readonly T _value;
    private readonly bool _hasValue;

    private Optional(T value)
    {
        _value = value;
        _hasValue = true;
    }

    public bool HasValue => _hasValue;
    public T Value => _hasValue ? _value! : throw new InvalidOperationException("Optional has no value.");

    public static implicit operator Optional<T>(T value) => new(value);
}
