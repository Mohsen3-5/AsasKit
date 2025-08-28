namespace AsasKit.Core.Exceptions;

public sealed class AsasException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public AsasException(
        string message,
        string code = "Error",
        int statusCode = 400,
        IReadOnlyDictionary<string, string[]>? errors = null,
        Exception? inner = null) : base(message, inner)
    {
        Code = code;
        StatusCode = statusCode;
        Errors = errors;
    }

    public static AsasException NotFound(string message, string code = "NotFound")
        => new(message, code, 404);

    public static AsasException Forbidden(string message, string code = "Forbidden")
        => new(message, code, 403);

    public static AsasException BadRequest(string message, string code = "BadRequest",
        IReadOnlyDictionary<string, string[]>? errors = null)
        => new(message, code, 400, errors);
}
