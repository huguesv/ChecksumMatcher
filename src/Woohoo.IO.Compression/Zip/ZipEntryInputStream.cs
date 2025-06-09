// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Zip;

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

public class ZipEntryInputStream : Stream
{
    private readonly Stream inputStream;
    private readonly long length;
    private long position;

    public ZipEntryInputStream(ZipFile file, ZipEntry entry)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(entry);

        this.inputStream = file.GetInputStream(entry);
        this.length = entry.Size;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => this.length;

    public override long Position
    {
        get => this.position;

        set
        {
            // Can only seek forward
            if (value >= this.position)
            {
                _ = this.Seek(value, SeekOrigin.Begin);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public override void Flush()
    {
        this.inputStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        // Can only seek forward
        if (offset > 0)
        {
            var expectedPos = this.Position + offset;

            if (expectedPos != this.Seek(offset, SeekOrigin.Current))
            {
                return 0;
            }
        }
        else if (offset < 0)
        {
            throw new NotSupportedException();
        }

        var actual = this.inputStream.Read(buffer, 0, count);
        this.position += actual;

        return actual;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        long offsetFromCurrent;
        if (origin == SeekOrigin.Begin)
        {
            offsetFromCurrent = offset - this.Position;
        }
        else if (origin == SeekOrigin.End)
        {
            offsetFromCurrent = this.Length + offset - this.Position;
        }
        else
        {
            offsetFromCurrent = origin == SeekOrigin.Current ? offset : throw new NotSupportedException();
        }

        // Can only seek forward
        if (offsetFromCurrent >= 0)
        {
            var data = new byte[4096];

            var remain = offsetFromCurrent;
            while (remain >= data.Length)
            {
                var actual = this.inputStream.Read(data, 0, data.Length);
                this.position += actual;

                if (actual < data.Length)
                {
                    return this.Position;
                }

                remain -= data.Length;
            }

            if (remain > 0)
            {
                var actual = this.inputStream.Read(data, 0, (int)remain);
                this.position += actual;
            }

            return this.Position;
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.inputStream.Dispose();
        }

        base.Dispose(disposing);
    }
}
