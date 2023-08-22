using System;

namespace GitHubActionsTestLogger.Utils;

internal static class RandomEx
{
    public static Random Shared { get; } = new();
}
