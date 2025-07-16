// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline;

using System;
using System.Collections.Generic;
using System.IO;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public class OfflineDirectoryInfo : IDirectoryInfo
{
    private readonly OfflineConfiguration configuration;
    private readonly OfflineEnumerator enumerator;
    private readonly OfflineItem? item;
    private readonly string path;

    public OfflineDirectoryInfo(OfflineConfiguration configuration, OfflineItem item)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(item);

        this.configuration = configuration;
        this.enumerator = new OfflineEnumerator(configuration);
        this.item = item;
        this.path = item.Path;

        if (this.path.EndsWith(Path.DirectorySeparatorChar) || this.path.EndsWith(Path.AltDirectorySeparatorChar))
        {
            this.path = Path.GetFileName(this.path.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar));
        }
    }

    public OfflineDirectoryInfo(OfflineConfiguration configuration, string path)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(path);

        this.configuration = configuration;
        this.enumerator = new OfflineEnumerator(configuration);
        this.path = path;

        if (this.path.EndsWith(Path.DirectorySeparatorChar) || this.path.EndsWith(Path.AltDirectorySeparatorChar))
        {
            this.path = Path.GetFileName(this.path.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar));
        }
    }

    public IDirectoryInfo? Parent
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

    public IDirectoryInfo Root
        => new OfflineDirectoryInfo(this.configuration, Path.GetPathRoot(Path.GetFullPath(this.path)) ?? string.Empty);

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
        get => this.item?.Modified ?? OfflineDates.NotFoundDateTimeUtc;
        set => throw new NotSupportedException();
    }

    public string Name => Path.GetFileName(this.path);

    public void Create()
    {
        throw new NotSupportedException();
    }

    public IDirectoryInfo CreateSubdirectory(string path)
    {
        throw new NotSupportedException();
    }

    public void Delete(bool recursive)
    {
        throw new NotSupportedException();
    }

    public void Delete()
    {
        throw new NotSupportedException();
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return this.enumerator
            .EnumerateItems(this.path, searchOption, item => item.Kind == OfflineItemKind.Folder && OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(item => OfflineEnumerator.ToDirectoryInfo(this.configuration, item));
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return this.enumerator
            .EnumerateItems(this.path, enumerationOptions, item => item.Kind == OfflineItemKind.Folder && OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(item => OfflineEnumerator.ToDirectoryInfo(this.configuration, item));
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return this.enumerator
            .EnumerateItems(this.path, item => item.Kind == OfflineItemKind.Folder && OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(item => OfflineEnumerator.ToDirectoryInfo(this.configuration, item));
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories()
    {
        return this.enumerator
            .EnumerateItems(this.path, item => item.Kind == OfflineItemKind.Folder)
            .Select(item => OfflineEnumerator.ToDirectoryInfo(this.configuration, item));
    }

    public IEnumerable<IFileInfo> EnumerateFiles()
    {
        return this.enumerator
            .EnumerateItems(this.path, item => item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile)
            .Select(item => OfflineEnumerator.ToFileInfo(this.configuration, item));
    }

    public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return this.enumerator
            .EnumerateItems(this.path, item => (item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile) && OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(item => OfflineEnumerator.ToFileInfo(this.configuration, item));
    }

    public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return this.enumerator
            .EnumerateItems(this.path, enumerationOptions, item => (item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile) && OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(item => OfflineEnumerator.ToFileInfo(this.configuration, item));
    }

    public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
    {
        return this.enumerator
            .EnumerateItems(this.path, searchOption, item => (item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile) && OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(item => OfflineEnumerator.ToFileInfo(this.configuration, item));
    }

    public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return this.enumerator
            .EnumerateItems(this.path, searchOption, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(item => OfflineEnumerator.ToInfo(this.configuration, item));
    }

    public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
    {
        return this.enumerator
            .EnumerateItems(this.path)
            .Select(item => OfflineEnumerator.ToInfo(this.configuration, item));
    }

    public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return this.enumerator
            .EnumerateItems(this.path, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(item => OfflineEnumerator.ToInfo(this.configuration, item));
    }

    public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return this.enumerator
            .EnumerateItems(this.path, enumerationOptions, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(item => OfflineEnumerator.ToInfo(this.configuration, item));
    }

    public IDirectoryInfo[] GetDirectories()
    {
        return [.. this.EnumerateDirectories()];
    }

    public IDirectoryInfo[] GetDirectories(string searchPattern)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return [.. this.EnumerateDirectories(searchPattern)];
    }

    public IDirectoryInfo[] GetDirectories(string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return [.. this.EnumerateDirectories(searchPattern, enumerationOptions)];
    }

    public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return [.. this.EnumerateDirectories(searchPattern, searchOption)];
    }

    public IFileInfo[] GetFiles(string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return [.. this.EnumerateFiles(searchPattern, enumerationOptions)];
    }

    public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return [.. this.EnumerateFiles(searchPattern, searchOption)];
    }

    public IFileInfo[] GetFiles()
    {
        return [.. this.EnumerateFiles()];
    }

    public IFileInfo[] GetFiles(string searchPattern)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return [.. this.EnumerateFiles(searchPattern)];
    }

    public IFileSystemInfo[] GetFileSystemInfos()
    {
        return [.. this.EnumerateFileSystemInfos()];
    }

    public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return [.. this.EnumerateFileSystemInfos(searchPattern)];
    }

    public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return [.. this.EnumerateFileSystemInfos(searchPattern, enumerationOptions)];
    }

    public IFileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
    {
        ArgumentNullException.ThrowIfNull(searchPattern);

        return [.. this.EnumerateFileSystemInfos(searchPattern, searchOption)];
    }

    public void MoveTo(string destDirName)
    {
        throw new NotSupportedException();
    }

    public void Refresh()
    {
    }
}
