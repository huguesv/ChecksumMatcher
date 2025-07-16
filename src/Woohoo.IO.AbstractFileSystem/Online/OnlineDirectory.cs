// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

public class OnlineDirectory : IDirectory
{
    public IDirectoryInfo CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return new OnlineDirectoryInfo(Directory.CreateDirectory(path));
    }

    public void Delete(string path, bool recursive)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Directory.Delete(path, recursive);
    }

    public void Delete(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Directory.Delete(path);
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.EnumerateDirectories(path);
    }

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.EnumerateDirectories(path, searchPattern);
    }

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.EnumerateDirectories(path, searchPattern, enumerationOptions);
    }

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.EnumerateDirectories(path, searchPattern, searchOption);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.EnumerateFiles(path, searchPattern, searchOption);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.EnumerateFiles(path, searchPattern, enumerationOptions);
    }

    public IEnumerable<string> EnumerateFiles(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.EnumerateFiles(path);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.EnumerateFiles(path, searchPattern);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.EnumerateFileSystemEntries(path);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.EnumerateFileSystemEntries(path, searchPattern);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);
    }

    public bool Exists([NotNullWhen(true)] string? path)
    {
        return Directory.Exists(path);
    }

    public DateTime GetCreationTime(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetCreationTime(path);
    }

    public DateTime GetCreationTimeUtc(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetCreationTimeUtc(path);
    }

    public string GetCurrentDirectory()
    {
        return Directory.GetCurrentDirectory();
    }

    public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.GetDirectories(path, searchPattern, searchOption);
    }

    public string[] GetDirectories(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.GetDirectories(path, searchPattern);
    }

    public string[] GetDirectories(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetDirectories(path);
    }

    public string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.GetDirectories(path, searchPattern, enumerationOptions);
    }

    public string GetDirectoryRoot(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetDirectoryRoot(path);
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.GetFiles(path, searchPattern, searchOption);
    }

    public string[] GetFiles(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.GetFiles(path, searchPattern);
    }

    public string[] GetFiles(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetFiles(path);
    }

    public string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.GetFiles(path, searchPattern, enumerationOptions);
    }

    public string[] GetFileSystemEntries(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.GetFileSystemEntries(path, searchPattern);
    }

    public string[] GetFileSystemEntries(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetFileSystemEntries(path);
    }

    public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.GetFileSystemEntries(path, searchPattern, searchOption);
    }

    public string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(searchPattern);

        return Directory.GetFileSystemEntries(path, searchPattern, enumerationOptions);
    }

    public DateTime GetLastAccessTime(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetLastAccessTime(path);
    }

    public DateTime GetLastAccessTimeUtc(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetLastAccessTimeUtc(path);
    }

    public DateTime GetLastWriteTime(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetLastWriteTime(path);
    }

    public DateTime GetLastWriteTimeUtc(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Directory.GetLastWriteTimeUtc(path);
    }

    public string[] GetLogicalDrives()
    {
        return Directory.GetLogicalDrives();
    }

    public IDirectoryInfo? GetParent(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var parentInfo = Directory.GetParent(path);
        return parentInfo is not null ? new OnlineDirectoryInfo(parentInfo) : null;
    }

    public void Move(string sourceDirName, string destDirName)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceDirName);
        ArgumentException.ThrowIfNullOrEmpty(destDirName);

        Directory.Move(sourceDirName, destDirName);
    }

    public void SetCreationTime(string path, DateTime creationTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Directory.SetCreationTime(path, creationTime);
    }

    public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Directory.SetCreationTimeUtc(path, creationTimeUtc);
    }

    public void SetCurrentDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Directory.SetCurrentDirectory(path);
    }

    public void SetLastAccessTime(string path, DateTime lastAccessTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Directory.SetLastAccessTime(path, lastAccessTime);
    }

    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
    }

    public void SetLastWriteTime(string path, DateTime lastWriteTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Directory.SetLastWriteTime(path, lastWriteTime);
    }

    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
    }
}
