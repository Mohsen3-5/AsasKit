using System.Text.RegularExpressions;

namespace AsasKit.Cli.Utils;

internal static class TextUtil
{
    public static string ToPascalCase(string raw)
    {
        var parts = Regex.Split(raw, @"[^A-Za-z0-9]+").Where(s => s.Length > 0);
        var pascal = string.Concat(parts.Select(s => char.ToUpper(s[0]) + s[1..]));
        if (string.IsNullOrEmpty(pascal)) pascal = "App";
        if (char.IsDigit(pascal[0])) pascal = "_" + pascal;
        return pascal;
    }

    public static string ToSafeDbName(string raw)
    {
        var s = Regex.Replace(raw, "[^A-Za-z0-9_]", "_");
        if (s.Length == 0) s = "asaskit";
        if (char.IsDigit(s[0])) s = "_" + s;
        return s.ToLowerInvariant();
    }

    public static string MaskConnectionString(string cs)
    {
        if (string.IsNullOrWhiteSpace(cs)) return cs;
        var masked = Regex.Replace(cs, "(?i)(password\\s*=\\s*)([^;]+)", "$1****");
        masked = Regex.Replace(masked, "(?i)(pwd\\s*=\\s*)([^;]+)", "$1****");
        return masked;
    }
}