// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;

using System;
using System.IO;

internal sealed class FileInformation
{
    public FileInformation()
    {
        this.IsFromOfflineStorage = false;
        this.ContainerAbsolutePath = string.Empty;
        this.FileRelativePath = string.Empty;
        this.ReportedCRC32 = [];
        this.CRC32 = [];
        this.MD5 = [];
        this.SHA1 = [];
        this.SHA256 = [];
    }

    public bool IsFromOfflineStorage { get; set; }

    public bool AllChecksumsCalculated => this.CRC32.Length > 0 && this.MD5.Length > 0 && this.SHA1.Length > 0 && this.SHA256.Length > 0;

    public string ContainerName => this.ContainerIsFolder
                ? Path.GetFileName(this.ContainerAbsolutePath)
                : Path.GetFileNameWithoutExtension(this.ContainerAbsolutePath);

    /// <summary>
    /// Gets or sets a value indicating whether the container is a folder, or an archive file.
    /// </summary>
    public bool ContainerIsFolder { get; set; }

    /// <summary>
    /// Gets or sets the full path to the container (may be an archive file or folder).
    /// </summary>
    public string ContainerAbsolutePath { get; set; }

    /// <summary>
    /// Gets or sets the relative path to the file inside its container.
    /// </summary>
    public string FileRelativePath { get; set; }

    /// <summary>
    /// Gets or sets the CRC32 that is reported by the archive. If the archive has been
    /// damaged since its creation, this may not be the actual CRC32.
    /// </summary>
    public byte[] ReportedCRC32 { get; set; }

    /// <summary>
    /// Gets or sets the CRC32 that was calculated.
    /// </summary>
    public byte[] CRC32 { get; set; }

    /// <summary>
    /// Gets or sets the MD5 that was calculated.
    /// </summary>
    public byte[] MD5 { get; set; }

    /// <summary>
    /// Gets or sets the SHA1 that was calculated.
    /// </summary>
    public byte[] SHA1 { get; set; }

    /// <summary>
    /// Gets or sets the SHA256 that was calculated.
    /// </summary>
    public byte[] SHA256 { get; set; }

    /// <summary>
    /// Gets or sets the file size.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the data block size. This is different from the file size
    /// if the file has a header/footer which isn't used in calculating the checksums.
    /// </summary>
    public long DataBlockSize { get; set; }

    public DateTime? LastWriteTime { get; set; }

    public DateTime? CreationTime { get; set; }

    public bool IsDirectory { get; set; }

    public string? CompressionMethod { get; set; }
}
