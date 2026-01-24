using System;
using System.IO;

namespace VRCX.App.Platform.Windows;

public class BurnAfterReadStream(Stream stream) : Stream
{
    public override bool CanRead => stream.CanRead;

    public override bool CanSeek => stream.CanSeek;

    public override bool CanWrite => stream.CanWrite;

    public override long Length => stream.Length;

    public override long Position { get => stream.Position; set => stream.Position = value; }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read;
        try
        {
            read = stream.Read(buffer, offset, count);
            if (read == 0)
            {
                stream.Dispose();
            }
        } 
        catch
        {
            stream.Dispose();
            throw;
        }
        return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}