using System;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class MtpExtensions
{
    public static string? GetOptionArgumentOrDefault(
        this ICommandLineOptions options,
        string optionName,
        string? defaultValue = null
    )
    {
        if (options.TryGetOptionArgumentList(optionName, out var arguments) && arguments.Length > 0)
        {
            return arguments[0];
        }

        return defaultValue;
    }

    public static bool? GetOptionSwitchValue(this ICommandLineOptions options, string optionName)
    {
        // If set, accept either "true" or "false" or no argument (which implies "true")
        if (options.IsOptionSet(optionName))
            return bool.Parse(options.GetOptionArgumentOrDefault(optionName) ?? "true");

        return null;
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
