namespace AsasKit.Core.Exceptions;

/// <summary>Throw with field errors; map to 400 in your web layer.</summary>
public sealed class ValidationProblemException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }
    public ValidationProblemException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.") => Errors = errors;
}
