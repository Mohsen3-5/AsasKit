using Asas.Core.Exceptions;

namespace Asas.Core.Guard;
public static class Guard
{
    public static T NotNull<T>(T? value, string name) where T : class =>
        value ?? throw AsasException.BadRequest($"{name} is required.", "Required");

    public static string NotNullOrWhiteSpace(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw AsasException.BadRequest($"{name} is required.", "Required")
            : value;

    public static int InRange(int value, int min, int max, string name) =>
        value < min || value > max
            ? throw AsasException.BadRequest($"{name} must be between {min} and {max}.", "OutOfRange")
            : value;
}