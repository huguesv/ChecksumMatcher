// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online;

using System;
using System.Collections.Generic;
using System.Linq;

public class OnlineDirectoryInfo : IDirectoryInfo
{
    private readonly DirectoryInfo innerInfo;

    public OnlineDirectoryInfo(DirectoryInfo innerInfo)
    {
        this.innerInfo = innerInfo;
    }

    public IDirectoryInfo? Parent
    {
        get
        {
            var parentInfo = this.innerInfo.Parent;
            return parentInfo is not null ? new OnlineDirectoryInfo(parentInfo) : null;
        }
    }

    public IDirectoryInfo Root
    {
        get
        {
            var rootInfo = this.innerInfo.Root;
            return new OnlineDirectoryInfo(rootInfo);
        }
    }

    public DateTime CreationTime { get => this.innerInfo.CreationTime; set => this.innerInfo.CreationTime = value; }

    public DateTime CreationTimeUtc { get => this.innerInfo.CreationTimeUtc; set => this.innerInfo.CreationTimeUtc = value; }

    public bool Exists => this.innerInfo.Exists;

    public string Extension => this.innerInfo.Extension;

    public string FullName => this.innerInfo.FullName;

    public DateTime LastAccessTime { get => this.innerInfo.LastAccessTime; set => this.innerInfo.LastAccessTime = value; }

    public DateTime LastAccessTimeUtc { get => this.innerInfo.LastAccessTimeUtc; set => this.innerInfo.LastAccessTimeUtc = value; }

    public DateTime LastWriteTime { get => this.innerInfo.LastWriteTime; set => this.innerInfo.LastWriteTime = value; }

    public DateTime LastWriteTimeUtc { get => this.innerInfo.LastWriteTimeUtc; set => this.innerInfo.LastWriteTimeUtc = value; }

    public string Name => this.innerInfo.Name;

    public void Create()
    {
        this.innerInfo.Create();
    }

    public IDirectoryInfo CreateSubdirectory(string path)
    {
        var result = this.innerInfo.CreateSubdirectory(path);
        return new OnlineDirectoryInfo(result);
    }

    public void Delete(bool recursive)
    {
        this.innerInfo.Delete(recursive);
    }

    public void Delete()
    {
        this.innerInfo.Delete();
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
    {
        foreach (var dir in this.innerInfo.EnumerateDirectories(searchPattern, searchOption))
        {
            yield return new OnlineDirectoryInfo(dir);
        }
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, EnumerationOptions enumerationOptions)
    {
        foreach (var dir in this.innerInfo.EnumerateDirectories(searchPattern, enumerationOptions))
        {
            yield return new OnlineDirectoryInfo(dir);
        }
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern)
    {
        foreach (var dir in this.innerInfo.EnumerateDirectories(searchPattern))
        {
            yield return new OnlineDirectoryInfo(dir);
        }
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories()
    {
        foreach (var dir in this.innerInfo.EnumerateDirectories())
        {
            yield return new OnlineDirectoryInfo(dir);
        }
    }

    public IEnumerable<IFileInfo> EnumerateFiles()
    {
        foreach (var file in this.innerInfo.EnumerateFiles())
        {
            yield return new OnlineFileInfo(file);
        }
    }

    public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
    {
        foreach (var file in this.innerInfo.EnumerateFiles(searchPattern))
        {
            yield return new OnlineFileInfo(file);
        }
    }

    public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, EnumerationOptions enumerationOptions)
    {
        foreach (var file in this.innerInfo.EnumerateFiles(searchPattern, enumerationOptions))
        {
            yield return new OnlineFileInfo(file);
        }
    }

    public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
    {
        foreach (var file in this.innerInfo.EnumerateFiles(searchPattern, searchOption))
        {
            yield return new OnlineFileInfo(file);
        }
    }

    public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
    {
        foreach (var info in this.innerInfo.EnumerateFileSystemInfos(searchPattern, searchOption))
        {
            yield return OnlineInfoFactory.CreateFileSystemInfo(info);
        }
    }

    public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
    {
        foreach (var info in this.innerInfo.EnumerateFileSystemInfos())
        {
            yield return OnlineInfoFactory.CreateFileSystemInfo(info);
        }
    }

    public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
    {
        foreach (var info in this.innerInfo.EnumerateFileSystemInfos(searchPattern))
        {
            yield return OnlineInfoFactory.CreateFileSystemInfo(info);
        }
    }

    public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
    {
        throw new NotImplementedException();
    }

    public IDirectoryInfo[] GetDirectories()
    {
        return this.innerInfo.GetDirectories()
            .Select(info => new OnlineDirectoryInfo(info))
            .ToArray();
    }

    public IDirectoryInfo[] GetDirectories(string searchPattern)
    {
        return this.innerInfo.GetDirectories(searchPattern)
            .Select(info => new OnlineDirectoryInfo(info))
            .ToArray();
    }

    public IDirectoryInfo[] GetDirectories(string searchPattern, EnumerationOptions enumerationOptions)
    {
        return this.innerInfo.GetDirectories(searchPattern, enumerationOptions)
            .Select(info => new OnlineDirectoryInfo(info))
            .ToArray();
    }

    public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
    {
        return this.innerInfo.GetDirectories(searchPattern, searchOption)
            .Select(info => new OnlineDirectoryInfo(info))
            .ToArray();
    }

    public IFileInfo[] GetFiles(string searchPattern, EnumerationOptions enumerationOptions)
    {
        return this.innerInfo.GetFiles(searchPattern, enumerationOptions)
            .Select(info => new OnlineFileInfo(info))
            .ToArray();
    }

    public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
    {
        return this.innerInfo.GetFiles(searchPattern, searchOption)
            .Select(info => new OnlineFileInfo(info))
            .ToArray();
    }

    public IFileInfo[] GetFiles()
    {
        return this.innerInfo.GetFiles()
            .Select(info => new OnlineFileInfo(info))
            .ToArray();
    }

    public IFileInfo[] GetFiles(string searchPattern)
    {
        return this.innerInfo.GetFiles(searchPattern)
            .Select(info => new OnlineFileInfo(info))
            .ToArray();
    }

    public IFileSystemInfo[] GetFileSystemInfos()
    {
        return this.innerInfo.GetFileSystemInfos()
            .Select(info => OnlineInfoFactory.CreateFileSystemInfo(info))
            .ToArray();
    }

    public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
    {
        return this.innerInfo.GetFileSystemInfos(searchPattern)
            .Select(info => OnlineInfoFactory.CreateFileSystemInfo(info))
            .ToArray();
    }

    public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
    {
        return this.innerInfo.GetFileSystemInfos(searchPattern, enumerationOptions)
            .Select(info => OnlineInfoFactory.CreateFileSystemInfo(info))
            .ToArray();
    }

    public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
    {
        return this.innerInfo.GetFileSystemInfos(searchPattern, searchOption)
            .Select(info => OnlineInfoFactory.CreateFileSystemInfo(info))
            .ToArray();
    }

    public void MoveTo(string destDirName)
    {
        this.innerInfo.MoveTo(destDirName);
    }

    public void Refresh()
    {
        this.innerInfo.Refresh();
    }
}
