using System.Reflection;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class AssemblyExtensions
{
    extension(Assembly assembly)
    {
        public string? TryGetVersionString() =>
            assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? assembly.GetName().Version?.ToString();
    }
}
