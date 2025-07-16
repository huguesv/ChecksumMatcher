// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online.Archiving;

using System;
using System.Collections.Generic;
using System.IO;
using Woohoo.IO.Compression.SevenZip;

internal class OnlineArchiveEngineSevenZip : IOnlineArchiveEngine
{
    public int Priority => 100;

    public bool IsSupportedExtension(string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        // TODO: this is only supported on Windows x86/x64/arm64
        return string.Equals(extension, ".7z", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, "7z", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<IArchiveEntry> EnumerateEntries(string archiveFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(archiveFilePath);

        if (!File.Exists(archiveFilePath))
        {
            throw ArchiveNotFound(archiveFilePath);
        }

        SevenZipFile? archive = null;
        try
        {
            archive = new SevenZipFile(archiveFilePath);
        }
        catch
        {
        }

        if (archive is null)
        {
            yield break;
        }

        foreach (var entry in archive.Entries)
        {
            // Note: Example entry names:
            // folder
            // folder\data1.txt
            var name = entry.Name;
            yield return new OnlineArchiveEntry(this)
            {
                ArchiveFilePath = archiveFilePath,
                Name = name,
                OriginalPath = entry.Name,
                Capabilities = ArchiveEntryCapabilities.CanListContents | ArchiveEntryCapabilities.CanExtract,
                Size = (long)entry.Size,
                CompressedSize = null,
                CompressionMethod = entry.Method,
                IsDirectory = entry.IsDirectory,
                LastModifiedUtc = entry.LastWriteTime, // TODO: check if this is UTC
                ReportedCRC32 = entry.IsDirectory ? null : ChecksumConversion.ToByteArray((uint)entry.CRC32),
            };
        }
    }

    public void Extract(OnlineArchiveEntry entry, string destinationPath)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrEmpty(destinationPath);

        if (!File.Exists(entry.ArchiveFilePath))
        {
            throw ArchiveNotFound(entry);
        }

        var archive = new SevenZipFile(entry.ArchiveFilePath);
        var current = archive.GetEntry(entry.OriginalPath) ?? throw EntryNotFound(entry);

        archive.Extract(current, destinationPath);
    }

    public Stream OpenRead(OnlineArchiveEntry entry)
    {
        throw new NotSupportedException();
    }

    public void Delete(OnlineArchiveEntry entry)
    {
        throw new NotSupportedException();
    }

    private static FileNotFoundException ArchiveNotFound(string archiveFilePath)
    {
        return new FileNotFoundException($"Archive file '{archiveFilePath}' does not exist.");
    }

    private static FileNotFoundException ArchiveNotFound(OnlineArchiveEntry entry)
    {
        return new FileNotFoundException($"Archive file '{entry.ArchiveFilePath}' does not exist.");
    }

    private static FileNotFoundException EntryNotFound(OnlineArchiveEntry entry)
    {
        return new FileNotFoundException($"Entry '{entry.Name}' not found in archive '{entry.ArchiveFilePath}'.");
    }
}
