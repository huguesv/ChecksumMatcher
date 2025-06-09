// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Zip;

using System.Diagnostics;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

public static class ZipFileExtensions
{
    public static Stream GetStream(this ZipFile zipFile, ZipEntry entry)
    {
        ArgumentNullException.ThrowIfNull(zipFile);
        ArgumentNullException.ThrowIfNull(entry);

        return new ZipEntryInputStream(zipFile, entry);
    }

    public static void Extract(this ZipFile zipFile, ZipEntry entry, string targetFilePath)
    {
        ArgumentNullException.ThrowIfNull(zipFile);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        using (var stream = zipFile.GetInputStream(entry))
        {
            var targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write);
            using (targetStream)
            {
                const int BlockLength = 16384;
                var remain = entry.Size;
                var count = remain < BlockLength ? (int)remain : BlockLength;
                var offset = 0;

                while (count > 0)
                {
                    var data = new byte[count];
                    if (count == stream.Read(data, 0, count))
                    {
                        targetStream.Write(data, 0, count);

                        offset += count;
                        remain = entry.Size - offset;
                        count = remain < BlockLength ? (int)remain : BlockLength;
                    }
                    else
                    {
                        Debug.Assert(false, "Could not read requested bytes.");
                    }
                }
            }
        }
    }
}
