using System.IO;

namespace GitHubActionsTestLogger.Tests.Utils.Extensions;

internal static class FileExtensions
{
    extension(File)
    {
        public static void WriteAllZeroes(string path, long count)
        {
            using var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 1
            );

            stream.SetLength(count);
        }

        public static byte[] ReadAllBytes(string path, int start)
        {
            var allBytes = File.ReadAllBytes(path);
            return allBytes[start..];
        }
    }
}
