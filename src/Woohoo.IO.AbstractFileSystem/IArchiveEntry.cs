// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System;

public interface IArchiveEntry
{
    string ArchiveFilePath { get; }

    string Name { get; }

    ArchiveEntryCapabilities Capabilities { get; }

    long Size { get; }

    long? CompressedSize { get; }

    DateTime? LastModifiedUtc { get; }

    bool IsDirectory { get; }

    bool? IsCompressed { get; }

    bool? IsEncrypted { get; }

    string? CompressionMethod { get; }

    string? EncryptionMethod { get; }

    byte[]? ReportedCRC32 { get; }

    byte[]? CRC32 { get; }

    byte[]? MD5 { get; }

    byte[]? SHA1 { get; }

    byte[]? SHA256 { get; }

    byte[]? SHA512 { get; }

    void Extract(string destinationPath);

    Stream OpenRead();

    void Delete();
}
