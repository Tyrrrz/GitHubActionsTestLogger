using System.IO;

namespace GitHubActionsTestLogger.Tests.Utils.Extensions;

internal static class FileExtensions
{
    extension(File)
    {
        // Writes 'count' zero bytes to a file without allocating a large buffer.
        // Uses SetLength to expand the file on disk via a single system call.
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
