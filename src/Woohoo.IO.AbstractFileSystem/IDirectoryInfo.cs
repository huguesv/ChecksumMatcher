// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

public interface IDirectoryInfo : IFileSystemInfo
{
    IDirectoryInfo? Parent { get; }

    IDirectoryInfo Root { get; }

    void Create();

    IDirectoryInfo CreateSubdirectory(string path);

    void Delete(bool recursive);

    IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, System.IO.SearchOption searchOption);

    IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern);

    IEnumerable<IDirectoryInfo> EnumerateDirectories();

    IEnumerable<IFileInfo> EnumerateFiles();

    IEnumerable<IFileInfo> EnumerateFiles(string searchPattern);

    IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, System.IO.SearchOption searchOption);

    IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, System.IO.SearchOption searchOption);

    IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos();

    IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern);

    IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    IDirectoryInfo[] GetDirectories();

    IDirectoryInfo[] GetDirectories(string searchPattern);

    IDirectoryInfo[] GetDirectories(string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    IDirectoryInfo[] GetDirectories(string searchPattern, System.IO.SearchOption searchOption);

    IFileInfo[] GetFiles(string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    IFileInfo[] GetFiles(string searchPattern, System.IO.SearchOption searchOption);

    IFileInfo[] GetFiles();

    IFileInfo[] GetFiles(string searchPattern);

    IFileSystemInfo[] GetFileSystemInfos();

    IFileSystemInfo[] GetFileSystemInfos(string searchPattern);

    IFileSystemInfo[] GetFileSystemInfos(string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    IFileSystemInfo[] GetFileSystemInfos(string searchPattern, System.IO.SearchOption searchOption);

    void MoveTo(string destDirName);
}
