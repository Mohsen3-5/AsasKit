using System.Diagnostics;

namespace AsasKit.Cli.Utils;

internal static class ProcessRunner
{
    public static async Task Exec(
        string file, string args,
        Dictionary<string,string>? env = null,
        string? cwd = null,
        bool ignoreExitCode = false)
    {
        var psi = NewPsi(file, args, env, cwd);
        using var p = Process.Start(psi)!;
        var stdout = await p.StandardOutput.ReadToEndAsync();
        var stderr = await p.StandardError.ReadToEndAsync();
        p.WaitForExit();
        if (!string.IsNullOrWhiteSpace(stdout)) Console.WriteLine(stdout.TrimEnd());
        if (!string.IsNullOrWhiteSpace(stderr)) Console.Error.WriteLine(stderr.TrimEnd());
        if (p.ExitCode != 0 && !ignoreExitCode) throw new Exception($"{file} {args} exited {p.ExitCode}");
    }

    public static async Task<(string stdout,string stderr,int code)> ExecCapture(
        string file, string args,
        Dictionary<string,string>? env = null,
        string? cwd = null,
        bool ignoreExitCode = false)
    {
        var psi = NewPsi(file, args, env, cwd);
        using var p = Process.Start(psi)!;
        var stdout = await p.StandardOutput.ReadToEndAsync();
        var stderr = await p.StandardError.ReadToEndAsync();
        p.WaitForExit();
        if (p.ExitCode != 0 && !ignoreExitCode) Console.Error.WriteLine(stderr.TrimEnd());
        return (stdout, stderr, p.ExitCode);
    }

    private static ProcessStartInfo NewPsi(string file, string args, Dictionary<string,string>? env, string? cwd)
    {
        var psi = new ProcessStartInfo(file, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            WorkingDirectory       = cwd ?? Environment.CurrentDirectory
        };
        if (env != null) foreach (var kv in env) psi.Environment[kv.Key] = kv.Value;
        return psi;
    }
}