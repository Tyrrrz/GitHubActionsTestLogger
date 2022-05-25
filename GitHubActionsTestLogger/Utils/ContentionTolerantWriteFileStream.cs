﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

namespace GitHubActionsTestLogger.Utils;

internal class ContentionTolerantWriteFileStream : Stream
{
    private readonly string _filePath;
    private readonly FileMode _fileMode;

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

    public ContentionTolerantWriteFileStream(string filePath, FileMode fileMode)
    {
        _filePath = filePath;
        _fileMode = fileMode;
    }

    // Backoff and retry if the file is locked
    private FileStream CreateInnerStream()
    {
        for (var retry = 0;; retry++)
        {
            try
            {
                return new FileStream(_filePath, _fileMode);
            }
            catch (IOException) when (retry < 10)
            {
                // Variance in delay to avoid overlapping back-offs
                Thread.Sleep(RandomEx.Shared.Next(200, 1000));
            }
        }
    }

    public override void Write(byte[] buffer, int offset, int count) =>
        _buffer.AddRange(buffer.Skip(offset).Take(count));

    public override void Flush()
    {
        using var stream = CreateInnerStream();
        stream.Write(_buffer.ToArray(), 0, _buffer.Count);
    }

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
    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public override void SetLength(long value) =>
        throw new NotSupportedException();
}