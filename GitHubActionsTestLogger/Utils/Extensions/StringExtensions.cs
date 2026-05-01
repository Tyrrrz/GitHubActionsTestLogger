using System;
using System.Text;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class StringExtensions
{
    extension(string value)
    {
        public string Truncate(int byteLimit, Encoding encoding)
        {
            if (byteLimit <= 0)
                return string.Empty;

            if (encoding.GetByteCount(value) <= byteLimit)
                return value;

            var bytes = encoding.GetBytes(value);
            var limit = Math.Min(byteLimit, bytes.Length);

            // Decode only complete characters that fit within the byte limit.
            // Using flush: false ensures incomplete multi-byte sequences at the
            // boundary are not included in the output.
            var chars = new char[encoding.GetMaxCharCount(limit)];
            var charsUsed = encoding.GetDecoder().GetChars(bytes, 0, limit, chars, 0, flush: false);

            return new string(chars, 0, charsUsed);
        }
    }
}
