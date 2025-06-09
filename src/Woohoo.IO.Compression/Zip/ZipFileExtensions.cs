// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Zip;

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

        using var sourceStream = zipFile.GetInputStream(entry);
        using var targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write);

        sourceStream.CopyTo(targetStream);
    }

    public static async Task ExtractAsync(this ZipFile zipFile, ZipEntry entry, string targetFilePath, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(zipFile);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        using var sourceStream = zipFile.GetInputStream(entry);
        using var targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write);

        await sourceStream.CopyToAsync(targetStream, ct);
    }
}
