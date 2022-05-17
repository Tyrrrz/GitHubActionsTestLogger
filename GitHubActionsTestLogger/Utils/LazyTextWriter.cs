using System;
using System.IO;
using System.Text;

namespace GitHubActionsTestLogger.Utils;

internal class LazyTextWriter : TextWriter
{
    private readonly Func<TextWriter> _createInnerWriter;

    private readonly StringBuilder _buffer = new();

    public override Encoding Encoding => throw new NotSupportedException();

    public LazyTextWriter(Func<TextWriter> createInnerWriter) =>
        _createInnerWriter = createInnerWriter;

    public override void Write(char value) =>
        _buffer.Append(value);

    public override void Flush()
    {
        using var writer = _createInnerWriter();

        for (var i = 0; i < _buffer.Length; i++)
            writer.Write(_buffer[i]);

        _buffer.Clear();

        writer.Flush();
        base.Flush();
    }
}