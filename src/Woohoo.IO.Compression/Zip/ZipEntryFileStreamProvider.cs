// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Zip;

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

public class ZipEntryFileStreamProvider : IStaticDataSource, IDisposable
{
    private readonly FileStream stream;

    public ZipEntryFileStreamProvider(string filePath)
    {
        this.stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    ~ZipEntryFileStreamProvider()
    {
        this.Dispose(false);
    }

    public System.IO.Stream GetSource()
    {
        return this.stream;
    }

    public void Dispose()
    {
        this.Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.stream.Dispose();
        }
    }
}
