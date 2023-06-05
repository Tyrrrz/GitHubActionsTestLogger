using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GitHubActionsTestLogger.Utils;

internal static class PathEx
{
    private static readonly StringComparison PathStringComparison =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    // This method exists on .NET 5+ but it's impossible to polyfill static
    // members, so we'll just use this one on all targets.
    public static string GetRelativePath(string basePath, string path)
    {
        var basePathSegments = basePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var pathSegments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var commonSegmentsCount = 0;
        for (var i = 0; i < basePathSegments.Length && i < pathSegments.Length; i++)
        {
            if (!string.Equals(basePathSegments[i], pathSegments[i], PathStringComparison))
                break;

            commonSegmentsCount++;
        }

        return string.Join(
            Path.DirectorySeparatorChar.ToString(),
            pathSegments.Skip(commonSegmentsCount)
        );
    }
}