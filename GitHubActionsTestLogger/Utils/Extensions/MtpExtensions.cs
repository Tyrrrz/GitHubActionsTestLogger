using System;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class MtpExtensions
{
    public static string? GetOptionArgumentOrDefault(
        this ICommandLineOptions options,
        string argumentName,
        string? defaultValue = null
    )
    {
        if (
            options.TryGetOptionArgumentList(argumentName, out var arguments)
            && arguments.Length > 0
        )
        {
            return arguments[0];
        }

        return defaultValue;
    }

    public static Exception? TryGetException(this TestNodeStateProperty state) =>
        state switch
        {
            FailedTestNodeStateProperty failedState => failedState.Exception,
            ErrorTestNodeStateProperty errorState => errorState.Exception,
            TimeoutTestNodeStateProperty timeoutState => timeoutState.Exception,
            _ => null,
        };
}
