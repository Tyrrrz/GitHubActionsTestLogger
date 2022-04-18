using System;

namespace GitHubActionsTestLogger.Tests.Utils;

internal static class EnvironmentVariable
{
    public static IDisposable Set(string name, string? value)
    {
        Environment.SetEnvironmentVariable(name, value);
        return Disposable.Create(() => Environment.SetEnvironmentVariable(name, null));
    }
}