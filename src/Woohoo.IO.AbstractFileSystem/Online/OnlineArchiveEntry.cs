// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online;

using System;
using System.IO;
using Woohoo.IO.AbstractFileSystem.Online.Archiving;

internal class OnlineArchiveEntry : IArchiveEntry
{
    private readonly IOnlineArchiveEngine engine;

    public OnlineArchiveEntry(IOnlineArchiveEngine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);

        this.engine = engine;
    }

    public required string ArchiveFilePath { get; init; }

    public required string Name { get; init; }

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

    internal required string OriginalPath { get; init; }

    public void Extract(string destinationPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(destinationPath);

        this.engine.Extract(this, destinationPath);
    }

    public Stream OpenRead()
    {
        return this.engine.OpenRead(this);
    }

    public void Delete()
    {
        this.engine.Delete(this);
    }
}
