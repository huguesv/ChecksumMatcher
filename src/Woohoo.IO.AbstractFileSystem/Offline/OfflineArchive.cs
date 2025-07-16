// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline;

using System;
using System.Collections.Generic;
using Woohoo.IO.AbstractFileSystem.Offline.Models;
using Woohoo.IO.AbstractFileSystem.Online;

public class OfflineArchive : IArchive
{
    private readonly OfflineConfiguration configuration;
    private readonly OfflineEnumerator enumerator;

    public OfflineArchive(OfflineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        this.configuration = configuration;
        this.enumerator = new OfflineEnumerator(configuration);
    }

    public bool IsSupportedArchiveFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var extension = Path.GetExtension(path);
        return string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(extension, ".7z", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<IArchiveEntry> EnumerateEntries(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var item = this.configuration.GetItemByPath(path);
        if (item is null)
        {
            throw new FileNotFoundException($"Archive file '{path}' does not exist.");
        }

        foreach (var entry in this.enumerator.EnumerateItems(path))
        {
            // TODO: need to decide how to handle subfolders
            yield return new OfflineArchiveEntry
            {
                ArchiveFilePath = item.Path,
                Name = entry.Name,
                FullPath = entry.Path,
                Capabilities = ArchiveEntryCapabilities.CanListContents,
                Size = entry.Size ?? 0,
                CompressedSize = null,
                CompressionMethod = null,
                IsDirectory = entry.Kind == OfflineItemKind.Folder,
                LastModifiedUtc = entry.Modified,
                ReportedCRC32 = ChecksumConversion.ToByteArray(entry.ReportedCRC32),
                CRC32 = ChecksumConversion.ToByteArray(entry.CRC32),
                MD5 = ChecksumConversion.ToByteArray(entry.MD5),
                SHA1 = ChecksumConversion.ToByteArray(entry.SHA1),
                SHA256 = ChecksumConversion.ToByteArray(entry.SHA256),
                SHA512 = ChecksumConversion.ToByteArray(entry.SHA512),
            };
        }
    }

    public IArchiveEntry[] GetEntries(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return [.. this.EnumerateEntries(path)];
    }
}
