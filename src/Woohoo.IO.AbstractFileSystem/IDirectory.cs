// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

public interface IDirectory
{
    IDirectoryInfo CreateDirectory(string path);

    void Delete(string path, bool recursive);

    void Delete(string path);

    System.Collections.Generic.IEnumerable<string> EnumerateDirectories(string path);

    System.Collections.Generic.IEnumerable<string> EnumerateDirectories(string path, string searchPattern);

    System.Collections.Generic.IEnumerable<string> EnumerateDirectories(string path, string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    System.Collections.Generic.IEnumerable<string> EnumerateDirectories(string path, string searchPattern, System.IO.SearchOption searchOption);

    System.Collections.Generic.IEnumerable<string> EnumerateFiles(string path, string searchPattern, System.IO.SearchOption searchOption);

    System.Collections.Generic.IEnumerable<string> EnumerateFiles(string path, string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    System.Collections.Generic.IEnumerable<string> EnumerateFiles(string path);

    System.Collections.Generic.IEnumerable<string> EnumerateFiles(string path, string searchPattern);

    System.Collections.Generic.IEnumerable<string> EnumerateFileSystemEntries(string path);

    System.Collections.Generic.IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern);

    System.Collections.Generic.IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    System.Collections.Generic.IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, System.IO.SearchOption searchOption);

    bool Exists(string path);

    System.DateTime GetCreationTime(string path);

    System.DateTime GetCreationTimeUtc(string path);

    string GetCurrentDirectory();

    string[] GetDirectories(string path);

    string[] GetDirectories(string path, string searchPattern);

    string[] GetDirectories(string path, string searchPattern, System.IO.SearchOption searchOption);

    string[] GetDirectories(string path, string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    string GetDirectoryRoot(string path);

    string[] GetFiles(string path);

    string[] GetFiles(string path, string searchPattern);

    string[] GetFiles(string path, string searchPattern, System.IO.SearchOption searchOption);

    string[] GetFiles(string path, string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    string[] GetFileSystemEntries(string path);

    string[] GetFileSystemEntries(string path, string searchPattern);

    string[] GetFileSystemEntries(string path, string searchPattern, System.IO.SearchOption searchOption);

    string[] GetFileSystemEntries(string path, string searchPattern, System.IO.EnumerationOptions enumerationOptions);

    System.DateTime GetLastAccessTime(string path);

    System.DateTime GetLastAccessTimeUtc(string path);

    System.DateTime GetLastWriteTime(string path);

    System.DateTime GetLastWriteTimeUtc(string path);

    string[] GetLogicalDrives();

    IDirectoryInfo? GetParent(string path);

    void Move(string sourceDirName, string destDirName);

    void SetCreationTime(string path, System.DateTime creationTime);

    void SetCreationTimeUtc(string path, System.DateTime creationTimeUtc);

    void SetCurrentDirectory(string path);

    void SetLastAccessTime(string path, System.DateTime lastAccessTime);

    void SetLastAccessTimeUtc(string path, System.DateTime lastAccessTimeUtc);

    void SetLastWriteTime(string path, System.DateTime lastWriteTime);

    void SetLastWriteTimeUtc(string path, System.DateTime lastWriteTimeUtc);
}
