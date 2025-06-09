// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System;
using System.IO;

public class FileInformation
{
    private string containerAbsolutePath;

    private string fileRelativePath;

    private byte[] reportedCrc32;

    private byte[] crc32;

    private byte[] md5;

    private byte[] sha1;

    private byte[] sha256;

    public FileInformation()
    {
        this.IsFromCache = false;
        this.containerAbsolutePath = string.Empty;
        this.fileRelativePath = string.Empty;
        this.reportedCrc32 = new byte[0];
        this.crc32 = new byte[0];
        this.md5 = new byte[0];
        this.sha1 = new byte[0];
        this.sha256 = new byte[0];
    }

    public bool IsFromCache { get; set; }

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
    public string ContainerAbsolutePath
    {
        get => this.containerAbsolutePath;

        set => this.containerAbsolutePath = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the relative path to the file inside its container.
    /// </summary>
    public string FileRelativePath
    {
        get => this.fileRelativePath;

        set => this.fileRelativePath = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the CRC32 that is reported by the archive. If the archive has been
    /// damaged since its creation, this may not be the actual CRC32.
    /// </summary>
    public byte[] ReportedCRC32
    {
        get => this.reportedCrc32;

        set => this.reportedCrc32 = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the CRC32 that was calculated.
    /// </summary>
    public byte[] CRC32
    {
        get => this.crc32;

        set => this.crc32 = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the MD5 that was calculated.
    /// </summary>
    public byte[] MD5
    {
        get => this.md5;

        set => this.md5 = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the SHA1 that was calculated.
    /// </summary>
    public byte[] SHA1
    {
        get => this.sha1;

        set => this.sha1 = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the SHA256 that was calculated.
    /// </summary>
    public byte[] SHA256
    {
        get => this.sha256;

        set => this.sha256 = value ?? throw new ArgumentNullException(nameof(value));
    }

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
