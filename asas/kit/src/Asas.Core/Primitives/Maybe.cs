namespace Asas.Core.Primitives;
public readonly struct Maybe<T>
{
    public static readonly Maybe<T> None = new(default, false);
    public T? Value { get; }
    public bool HasValue { get; }

    private Maybe(T? value, bool hasValue) => (Value, HasValue) = (value, hasValue);
    public static Maybe<T> Some(T value) => new(value, true);
}
