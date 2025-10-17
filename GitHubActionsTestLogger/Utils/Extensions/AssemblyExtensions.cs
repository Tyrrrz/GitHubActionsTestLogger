using System.Reflection;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class AssemblyExtensions
{
    public static string? TryGetVersionString(this Assembly assembly) =>
        assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? assembly.GetName().Version?.ToString();
}
