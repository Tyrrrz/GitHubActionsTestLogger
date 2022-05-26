using System;

namespace GitHubActionsTestLogger.Utils;

internal static class PathEx
{
    // This method exists on .NET5+ but it's impossible to polyfill static
    // members, so we'll just use this one on all targets.
    public static string GetRelativePath(string basePath, string path) =>
        Uri.UnescapeDataString(new Uri(new Uri(basePath), path).ToString());
}