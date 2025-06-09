// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.SevenZip;

public sealed class SevenZipEntry
{
    public int Index { get; init; }

    public required string Name { get; init; }

    public ulong Size { get; init; }

    public long CRC32 { get; init; }

    public DateTime LastWriteTime { get; init; }

    public DateTime CreationTime { get; init; }

    public DateTime LastAccessTime { get; init; }

    public bool IsDirectory { get; init; }

    public bool IsEncrypted { get; init; }

    public required string Comment { get; init; }

    public required string Method { get; init; }
}
