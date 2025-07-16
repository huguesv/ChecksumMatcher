// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online.Archiving;

using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Woohoo.IO.Compression.Zip;

internal class OnlineArchiveEngineSharpZip : IOnlineArchiveEngine
{
    public int Priority => 90;

    public bool IsSupportedExtension(string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        return string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, "zip", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<IArchiveEntry> EnumerateEntries(string archiveFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(archiveFilePath);

        if (!File.Exists(archiveFilePath))
        {
            throw ArchiveNotFound(archiveFilePath);
        }

        using var archive = new ZipFile(archiveFilePath);

        foreach (ZipEntry entry in archive)
        {
            // Note: Example entry names:
            // folder/
            // folder/file.ext
            var name = entry.Name.Replace('/', '\\');
            if (name.EndsWith('\\'))
            {
                name = name[..^1]; // Remove trailing backslash for directories
            }

            yield return new OnlineArchiveEntry(this)
            {
                ArchiveFilePath = archiveFilePath,
                Name = name,
                OriginalPath = entry.Name,
                Capabilities = ArchiveEntryCapabilities.CanListContents | ArchiveEntryCapabilities.CanRead | ArchiveEntryCapabilities.CanExtract | ArchiveEntryCapabilities.CanDelete,
                Size = entry.Size,
                CompressedSize = entry.CompressedSize,
                CompressionMethod = entry.CompressionMethod.ToString(),
                IsDirectory = entry.IsDirectory,
                LastModifiedUtc = entry.DateTime, // TODO: check if this is UTC
                ReportedCRC32 = entry.IsDirectory ? null : ByteArrayUtility.ByteArrayFromUInt32((uint)entry.Crc),
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

        using var archive = new ZipFile(entry.ArchiveFilePath);
        var current = archive.GetEntry(entry.OriginalPath) ?? throw EntryNotFound(entry);

        archive.Extract(current, destinationPath);
    }

    public Stream OpenRead(OnlineArchiveEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!File.Exists(entry.ArchiveFilePath))
        {
            throw ArchiveNotFound(entry);
        }

        // TODO:
        // Not sure of lifetime of the ZipFile
        // Will the stream keep it open?
        // Do we need a stream class that will dispose of the ZipFile when the stream is disposed?
        var archive = new ZipFile(entry.ArchiveFilePath);
        var current = archive.GetEntry(entry.OriginalPath) ?? throw EntryNotFound(entry);

        return archive.GetStream(current);
    }

    public void Delete(OnlineArchiveEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!File.Exists(entry.ArchiveFilePath))
        {
            throw ArchiveNotFound(entry);
        }

        using var archive = new ZipFile(entry.ArchiveFilePath);
        var current = archive.GetEntry(entry.OriginalPath) ?? throw EntryNotFound(entry);

        archive.BeginUpdate();
        archive.Delete(current);
        archive.CommitUpdate();
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
