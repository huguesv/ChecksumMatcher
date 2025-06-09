// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.SevenZip;

public class SevenZipEntry
{
    public int Index { get; internal set; }

    public string Name { get; internal set; } = string.Empty;

    public ulong Size { get; internal set; }

    public long CRC32 { get; internal set; }

    public DateTime LastWriteTime { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime LastAccessTime { get; set; }

    public bool IsDirectory { get; set; }

    public bool IsEncrypted { get; set; }

    public string Comment { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;
}
