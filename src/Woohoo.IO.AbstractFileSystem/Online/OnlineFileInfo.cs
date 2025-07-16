// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online;

using System;

public class OnlineFileInfo : IFileInfo
{
    private readonly FileInfo innerInfo;

    public OnlineFileInfo(FileInfo innerInfo)
    {
        ArgumentNullException.ThrowIfNull(innerInfo);

        this.innerInfo = innerInfo;
    }

    public IDirectoryInfo? Directory
    {
        get
        {
            var parentInfo = this.innerInfo.Directory;
            return parentInfo is not null ? new OnlineDirectoryInfo(parentInfo) : null;
        }
    }

    public string? DirectoryName => this.innerInfo.DirectoryName;

    public bool IsReadOnly
    {
        get => this.innerInfo.IsReadOnly;
        set => this.innerInfo.IsReadOnly = value;
    }

    public long Length => this.innerInfo.Length;

    public DateTime CreationTime
    {
        get => this.innerInfo.CreationTime;
        set => this.innerInfo.CreationTime = value;
    }

    public DateTime CreationTimeUtc
    {
        get => this.innerInfo.CreationTimeUtc;
        set => this.innerInfo.CreationTimeUtc = value;
    }

    public bool Exists => this.innerInfo.Exists;

    public string Extension => this.innerInfo.Extension;

    public string FullName => this.innerInfo.FullName;

    public string FullPath => this.innerInfo.FullName;

    public DateTime LastAccessTime
    {
        get => this.innerInfo.LastAccessTime;
        set => this.innerInfo.LastAccessTime = value;
    }

    public DateTime LastAccessTimeUtc
    {
        get => this.innerInfo.LastAccessTimeUtc;
        set => this.innerInfo.LastAccessTimeUtc = value;
    }

    public DateTime LastWriteTime
    {
        get => this.innerInfo.LastWriteTime;
        set => this.innerInfo.LastWriteTime = value;
    }

    public DateTime LastWriteTimeUtc
    {
        get => this.innerInfo.LastWriteTimeUtc;
        set => this.innerInfo.LastWriteTimeUtc = value;
    }

    public string Name => this.innerInfo.Name;

    public StreamWriter AppendText()
    {
        return this.innerInfo.AppendText();
    }

    public IFileInfo CopyTo(string destFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(destFileName);

        return new OnlineFileInfo(this.innerInfo.CopyTo(destFileName));
    }

    public Stream Create()
    {
        return this.innerInfo.Create();
    }

    public StreamWriter CreateText()
    {
        return this.innerInfo.CreateText();
    }

    public void Decrypt()
    {
        this.innerInfo.Decrypt();
    }

    public void Delete()
    {
        this.innerInfo.Delete();
    }

    public void Encrypt()
    {
        this.innerInfo.Encrypt();
    }

    public void MoveTo(string destFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(destFileName);

        this.innerInfo.MoveTo(destFileName);
    }

    public void MoveTo(string destFileName, bool overwrite)
    {
        ArgumentException.ThrowIfNullOrEmpty(destFileName);

        this.innerInfo.MoveTo(destFileName, overwrite);
    }

    public Stream Open(FileMode mode)
    {
        return this.innerInfo.Open(mode);
    }

    public Stream Open(FileStreamOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return this.innerInfo.Open(options);
    }

    public Stream Open(FileMode mode, FileAccess access)
    {
        return this.innerInfo.Open(mode, access);
    }

    public Stream Open(FileMode mode, FileAccess access, FileShare share)
    {
        return this.innerInfo.Open(mode, access, share);
    }

    public Stream OpenRead()
    {
        return this.innerInfo.OpenRead();
    }

    public StreamReader OpenText()
    {
        return this.innerInfo.OpenText();
    }

    public FileStream OpenWrite()
    {
        return this.innerInfo.OpenWrite();
    }

    public void Refresh()
    {
        this.innerInfo.Refresh();
    }

    public IFileInfo Replace(string destinationFileName, string? destinationBackupFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(destinationFileName);

        return new OnlineFileInfo(this.innerInfo.Replace(destinationFileName, destinationBackupFileName));
    }

    public IFileInfo Replace(string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors)
    {
        ArgumentException.ThrowIfNullOrEmpty(destinationFileName);

        return new OnlineFileInfo(this.innerInfo.Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors));
    }
}
