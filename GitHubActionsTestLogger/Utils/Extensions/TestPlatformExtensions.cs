using Microsoft.Testing.Platform.CommandLine;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TestPlatformExtensions
{
    public static string? TryGetOptionArgument(
        this ICommandLineOptions options,
        string argumentName
    )
    {
        if (
            options.TryGetOptionArgumentList(argumentName, out var arguments)
            && arguments.Length > 0
        )
        {
            return arguments[0];
        }

        return null;
    }
}
