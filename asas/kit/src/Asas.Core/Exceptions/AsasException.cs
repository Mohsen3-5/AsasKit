namespace Asas.Core.Exceptions;

public sealed class AsasException : Exception
{
    public string CodeName { get; }
    public int StatusCode { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public AsasException(
        string message,
        string codeName = "Error",
        int statusCode = 400,
        IReadOnlyDictionary<string, string[]>? errors = null,
        Exception? inner = null) : base(message, inner)
    {
        CodeName = codeName;
        StatusCode = statusCode;
        Errors = errors;
    }

    public static AsasException NotFound(string message, string codeName = "NotFound")
        => new(message, codeName, 404);

    public static AsasException Forbidden(string message, string codeName = "Forbidden")
        => new(message, codeName, 403);

    public static AsasException BadRequest(string message, string codeName = "BadRequest",
                                           IReadOnlyDictionary<string, string[]>? errors = null)
        => new(message, codeName, 400, errors);
}
