// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online;

using System;
using System.IO;

internal sealed class OfflineArchiveEntry : IArchiveEntry
{
    public required string ArchiveFilePath { get; init; }

    public required string Name { get; init; }

    public required string FullPath { get; init; }

    public ArchiveEntryCapabilities Capabilities { get; init; }

    public long Size { get; init; }

    public long? CompressedSize { get; init; }

    public DateTime? LastModifiedUtc { get; init; }

    public bool IsDirectory { get; init; }

    public bool? IsCompressed { get; init; }

    public bool? IsEncrypted { get; init; }

    public string? CompressionMethod { get; init; }

    public string? EncryptionMethod { get; init; }

    public byte[]? ReportedCRC32 { get; init; }

    public byte[]? CRC32 { get; init; }

    public byte[]? MD5 { get; init; }

    public byte[]? SHA1 { get; init; }

    public byte[]? SHA256 { get; init; }

    public byte[]? SHA512 { get; init; }

    public void Extract(string destinationPath)
    {
        throw new NotSupportedException();
    }

    public Stream OpenRead()
    {
        throw new NotSupportedException();
    }

    public void Delete()
    {
        throw new NotSupportedException();
    }
}
