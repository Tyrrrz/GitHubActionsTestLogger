using System;

namespace GitHubActionsTestLogger.Utils;

internal static class PathEx
{
    // This method exists on .NET5+ but it's impossible to polyfill static
    // members, so we'll just use this one on all targets.
    public static string GetRelativePath(string basePath, string path)
    {
        // Without using System
        var basePathUri = new Uri(basePath);
        var pathUri = new Uri(path);

        return Uri.UnescapeDataString(
            basePathUri
                .MakeRelativeUri(pathUri)
                .ToString()
        );
    }
}