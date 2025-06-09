// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Zip;

using ICSharpCode.SharpZipLib.Zip;

public class ZipEntryZipStreamProvider : IStaticDataSource
{
    private readonly ZipFile file;
    private readonly ZipEntry entry;

    public ZipEntryZipStreamProvider(ZipFile file, ZipEntry entry)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(entry);

        this.file = file;
        this.entry = entry;
    }

    public System.IO.Stream GetSource()
    {
        return new ZipEntryInputStream(this.file, this.entry);
    }
}
