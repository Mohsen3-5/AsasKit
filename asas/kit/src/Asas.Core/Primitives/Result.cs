namespace Asas.Core.Primitives;
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error Error { get; }

    private Result(bool ok, T? value, Error error) => (IsSuccess, Value, Error) = (ok, value, error);

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(string code, string message) => new(false, default, new Error(code, message));

    public T Unwrap() =>
        IsSuccess ? Value! : throw new InvalidOperationException($"Failure: {Error.Code} - {Error.Message}");
}
