// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline;

using System;
using System.Collections.Generic;
using System.IO;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public class OfflineDirectory : IDirectory
{
    private readonly OfflineConfiguration configuration;
    private readonly OfflineEnumerator enumerator;

    public OfflineDirectory(OfflineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        this.configuration = configuration;
        this.enumerator = new OfflineEnumerator(configuration);
    }

    public IDirectoryInfo CreateDirectory(string path)
    {
        throw new NotSupportedException();
    }

    public void Delete(string path, bool recursive)
    {
        throw new NotSupportedException();
    }

    public void Delete(string path)
    {
        throw new NotSupportedException();
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, item => item.Kind == OfflineItemKind.Folder)
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, item => item.Kind == OfflineItemKind.Folder, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, enumerationOptions, item => item.Kind == OfflineItemKind.Folder, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, searchOption, item => item.Kind == OfflineItemKind.Folder, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, searchOption, item => item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile)
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, enumerationOptions, item => item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile)
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateFiles(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, item => item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile)
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, item => item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path)
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, enumerationOptions, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(OfflineEnumerator.ToPath);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.enumerator
            .EnumerateItems(path, searchOption, item => OfflineEnumerator.IsPatternMatch(item, searchPattern))
            .Select(OfflineEnumerator.ToPath);
    }

    public bool Exists(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        var item = this.configuration.GetItemByPath(path);
        return item is not null && item.Kind == OfflineItemKind.Folder;
    }

    public DateTime GetCreationTime(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.GetCreationTimeUtc(path).ToLocalTime();
    }

    public DateTime GetCreationTimeUtc(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        var folder = this.configuration.GetItemByPath(path);
        if (folder is not null && folder.Kind == OfflineItemKind.Folder)
        {
            return folder.Created ?? OfflineDates.NotFoundDateTimeUtc;
        }

        return OfflineDates.NotFoundDateTimeUtc;
    }

    public string GetCurrentDirectory()
    {
        throw new NotSupportedException();
    }

    public string[] GetDirectories(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateDirectories(path).ToArray();
    }

    public string[] GetDirectories(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateDirectories(path, searchPattern).ToArray();
    }

    public string[] GetDirectories(string path, string searchPattern, System.IO.SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateDirectories(path, searchPattern, searchOption).ToArray();
    }

    public string[] GetDirectories(string path, string searchPattern, System.IO.EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateDirectories(path, searchPattern, enumerationOptions).ToArray();
    }

    public string GetDirectoryRoot(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        string fullPath = Path.GetFullPath(path);
        string root = Path.GetPathRoot(fullPath)!;

        return root;
    }

    public string[] GetFiles(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateFiles(path).ToArray();
    }

    public string[] GetFiles(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateFiles(path, searchPattern).ToArray();
    }

    public string[] GetFiles(string path, string searchPattern, System.IO.SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateFiles(path, searchPattern, searchOption).ToArray();
    }

    public string[] GetFiles(string path, string searchPattern, System.IO.EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateFiles(path, searchPattern, enumerationOptions).ToArray();
    }

    public string[] GetFileSystemEntries(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateFileSystemEntries(path).ToArray();
    }

    public string[] GetFileSystemEntries(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateFileSystemEntries(path, searchPattern).ToArray();
    }

    public string[] GetFileSystemEntries(string path, string searchPattern, System.IO.SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateFileSystemEntries(path, searchPattern, searchOption).ToArray();
    }

    public string[] GetFileSystemEntries(string path, string searchPattern, System.IO.EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions).ToArray();
    }

    public DateTime GetLastAccessTime(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.GetLastAccessTimeUtc(path).ToLocalTime();
    }

    public DateTime GetLastAccessTimeUtc(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        // Not supported, but return a default value to avoid exceptions.
        return OfflineDates.NotFoundDateTimeUtc;
    }

    public DateTime GetLastWriteTime(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return this.GetLastWriteTimeUtc(path).ToLocalTime();
    }

    public DateTime GetLastWriteTimeUtc(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        var folder = this.configuration.GetItemByPath(path);
        if (folder is not null && folder.Kind == OfflineItemKind.Folder)
        {
            return folder.Modified ?? OfflineDates.NotFoundDateTimeUtc;
        }

        return OfflineDates.NotFoundDateTimeUtc;
    }

    public string[] GetLogicalDrives()
    {
        // TODO
        throw new NotImplementedException();
    }

    public IDirectoryInfo? GetParent(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        string fullPath = Path.GetFullPath(path);

        string? parentPath = Path.GetDirectoryName(fullPath);
        if (parentPath is null)
        {
            return null;
        }

        return new OfflineDirectoryInfo(this.configuration, parentPath);
    }

    public void Move(string sourceDirName, string destDirName)
    {
        throw new NotSupportedException();
    }

    public void SetCreationTime(string path, DateTime creationTime)
    {
        throw new NotSupportedException();
    }

    public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
    {
        throw new NotSupportedException();
    }

    public void SetCurrentDirectory(string path)
    {
        throw new NotSupportedException();
    }

    public void SetLastAccessTime(string path, DateTime lastAccessTime)
    {
        throw new NotSupportedException();
    }

    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
    {
        throw new NotSupportedException();
    }

    public void SetLastWriteTime(string path, DateTime lastWriteTime)
    {
        throw new NotSupportedException();
    }

    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
    {
        throw new NotSupportedException();
    }
}
