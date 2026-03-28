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

        public static byte[] ReadAllBytes(string path, int offset)
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            stream.Seek(offset, SeekOrigin.Begin);

            var buffer = new byte[stream.Length - offset];
            stream.ReadExactly(buffer);

            return buffer;
        }
    }
}
