using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubActionsTestLogger.Utils;

internal class ContentionTolerantWriteFileStream(string filePath, FileMode fileMode) : Stream
{
    // Random is used to introduce variance in backoff delays.
    // Fixed seed for reproducibility in tests and debugging.
    private readonly Random _random = new(1173363);

    private readonly List<byte> _buffer = new(1024);

    [ExcludeFromCodeCoverage]
    public override bool CanRead => false;

    [ExcludeFromCodeCoverage]
    public override bool CanSeek => false;

    [ExcludeFromCodeCoverage]
    public override bool CanWrite => true;

    [ExcludeFromCodeCoverage]
    public override long Length => _buffer.Count;

    [ExcludeFromCodeCoverage]
    public override long Position { get; set; }

    // Backoff and retry if the file is locked
    private FileStream CreateInnerStream()
    {
        for (var retriesRemaining = 10; ; retriesRemaining--)
        {
            try
            {
                return new FileStream(filePath, fileMode);
            }
            catch (IOException) when (retriesRemaining > 0)
            {
                // Variance in delay to avoid overlapping back-offs
                Thread.Sleep(_random.Next(200, 1000));
            }
        }
    }

    public override void Write(byte[] buffer, int offset, int count) =>
        _buffer.AddRange(buffer.Skip(offset).Take(count));

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        using var stream = CreateInnerStream();
        await stream.WriteAsync(_buffer.ToArray(), 0, _buffer.Count, cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    public override void Flush() => FlushAsync().GetAwaiter().GetResult();

    [ExcludeFromCodeCoverage]
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _buffer.Clear();
    }

    [ExcludeFromCodeCoverage]
    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public override void SetLength(long value) => throw new NotSupportedException();
}
