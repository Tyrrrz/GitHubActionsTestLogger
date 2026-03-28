using System.IO;

namespace GitHubActionsTestLogger.Tests.Utils.Extensions;

internal static class FileExtensions
{
    extension(File)
    {
        public static void WriteAllZeroes(string path, long count)
        {
            using var fs = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 1
            );

            fs.SetLength(count);
        }
    }
}
