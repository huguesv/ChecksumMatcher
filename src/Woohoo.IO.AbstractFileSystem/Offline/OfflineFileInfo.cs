// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline;

using System;
using System.IO;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public class OfflineFileInfo : IFileInfo
{
    private readonly OfflineConfiguration configuration;
    private readonly OfflineItem? item;
    private readonly string path;

    public OfflineFileInfo(OfflineConfiguration configuration, OfflineItem item)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(item);

        this.configuration = configuration;
        this.item = item;
        this.path = item.Path;
    }

    public OfflineFileInfo(OfflineConfiguration configuration, string path)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(path);

        this.configuration = configuration;
        this.path = path;
    }

    public IDirectoryInfo? Directory
    {
        get
        {
            if (this.item is not null)
            {
                var parentItem = this.item.ParentItem;
                if (parentItem is not null && parentItem.Kind == OfflineItemKind.Folder)
                {
                    return new OfflineDirectoryInfo(this.configuration, parentItem);
                }
            }

            var parentPath = Path.GetDirectoryName(this.path);
            if (string.IsNullOrEmpty(parentPath) || parentPath == Path.GetPathRoot(Path.GetFullPath(this.path)))
            {
                return null; // No parent directory
            }

            return new OfflineDirectoryInfo(this.configuration, parentPath);
        }
    }

    public string? DirectoryName => Path.GetDirectoryName(this.path);

    public bool IsReadOnly
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public long Length =>
        this.item is not null
        ? this.item.Size ?? 0
        : throw new FileNotFoundException($"Could not find file '{this.path}'");

    public DateTime CreationTime
    {
        get => this.item?.Created?.ToLocalTime() ?? OfflineDates.NotFoundDateTimeUtc.ToLocalTime();
        set => throw new NotSupportedException();
    }

    public DateTime CreationTimeUtc
    {
        get => this.item?.Created ?? OfflineDates.NotFoundDateTimeUtc;
        set => throw new NotSupportedException();
    }

    public bool Exists => this.item is not null;

    public string Extension => Path.GetExtension(this.path);

    public string FullName => Path.GetFullPath(this.path);

    public DateTime LastAccessTime
    {
        get => OfflineDates.NotFoundDateTimeUtc.ToLocalTime();
        set => throw new NotSupportedException();
    }

    public DateTime LastAccessTimeUtc
    {
        get => OfflineDates.NotFoundDateTimeUtc;
        set => throw new NotSupportedException();
    }

    public DateTime LastWriteTime
    {
        get => this.item?.Modified?.ToLocalTime() ?? OfflineDates.NotFoundDateTimeUtc.ToLocalTime();
        set => throw new NotSupportedException();
    }

    public DateTime LastWriteTimeUtc
    {
        get => this.item?.Modified?.ToLocalTime() ?? OfflineDates.NotFoundDateTimeUtc;
        set => throw new NotSupportedException();
    }

    public string Name => Path.GetFileName(this.path);

    public StreamWriter AppendText()
    {
        throw new NotSupportedException();
    }

    public IFileInfo CopyTo(string destFileName)
    {
        throw new NotSupportedException();
    }

    public Stream Create()
    {
        throw new NotSupportedException();
    }

    public StreamWriter CreateText()
    {
        throw new NotSupportedException();
    }

    public void Decrypt()
    {
        throw new NotSupportedException();
    }

    public void Delete()
    {
        throw new NotSupportedException();
    }

    public void Encrypt()
    {
        throw new NotSupportedException();
    }

    public void MoveTo(string destFileName)
    {
        throw new NotSupportedException();
    }

    public void MoveTo(string destFileName, bool overwrite)
    {
        throw new NotSupportedException();
    }

    public Stream Open(FileMode mode)
    {
        throw new NotSupportedException();
    }

    public Stream Open(FileStreamOptions options)
    {
        throw new NotSupportedException();
    }

    public Stream Open(FileMode mode, FileAccess access)
    {
        throw new NotSupportedException();
    }

    public Stream Open(FileMode mode, FileAccess access, FileShare share)
    {
        throw new NotSupportedException();
    }

    public Stream OpenRead()
    {
        throw new NotSupportedException();
    }

    public StreamReader OpenText()
    {
        throw new NotSupportedException();
    }

    public FileStream OpenWrite()
    {
        throw new NotSupportedException();
    }

    public void Refresh()
    {
    }

    public IFileInfo Replace(string destinationFileName, string? destinationBackupFileName)
    {
        throw new NotSupportedException();
    }

    public IFileInfo Replace(string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors)
    {
        throw new NotSupportedException();
    }
}
