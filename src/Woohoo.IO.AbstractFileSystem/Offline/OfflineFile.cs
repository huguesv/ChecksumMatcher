// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public class OfflineFile : IFile
{
    private readonly OfflineConfiguration configuration;

    public OfflineFile(OfflineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        this.configuration = configuration;
    }

    public void AppendAllBytes(string path, byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public void AppendAllBytes(string path, ReadOnlySpan<byte> bytes)
    {
        throw new NotSupportedException();
    }

    public Task AppendAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task AppendAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public void AppendAllLines(string path, IEnumerable<string> contents)
    {
        throw new NotSupportedException();
    }

    public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
        throw new NotSupportedException();
    }

    public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public void AppendAllText(string path, ReadOnlySpan<char> contents)
    {
        throw new NotSupportedException();
    }

    public void AppendAllText(string path, ReadOnlySpan<char> contents, Encoding encoding)
    {
        throw new NotSupportedException();
    }

    public void AppendAllText(string path, string? contents)
    {
        throw new NotSupportedException();
    }

    public void AppendAllText(string path, string? contents, Encoding encoding)
    {
        throw new NotSupportedException();
    }

    public Task AppendAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task AppendAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public StreamWriter AppendText(string path)
    {
        throw new NotSupportedException();
    }

    public void Copy(string sourceFileName, string destFileName, bool overwrite)
    {
        throw new NotSupportedException();
    }

    public void Copy(string sourceFileName, string destFileName)
    {
        throw new NotSupportedException();
    }

    public Stream Create(string path, int bufferSize, FileOptions options)
    {
        throw new NotSupportedException();
    }

    public Stream Create(string path, int bufferSize)
    {
        throw new NotSupportedException();
    }

    public Stream Create(string path)
    {
        throw new NotSupportedException();
    }

    public StreamWriter CreateText(string path)
    {
        throw new NotSupportedException();
    }

    public void Decrypt(string path)
    {
        throw new NotSupportedException();
    }

    public void Delete(string path)
    {
        throw new NotSupportedException();
    }

    public void Encrypt(string path)
    {
        throw new NotSupportedException();
    }

    public bool Exists(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        var item = this.configuration.GetItemByPath(path);
        return item is not null && (item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile);
    }

    public FileAttributes GetAttributes(string path)
    {
        throw new NotSupportedException();
    }

    public DateTime GetCreationTime(string path)
    {
        return this.GetCreationTimeUtc(path).ToLocalTime();
    }

    public DateTime GetCreationTimeUtc(string path)
    {
        var item = this.configuration.GetItemByPath(path);
        return item?.Created ?? OfflineDates.NotFoundDateTimeUtc;
    }

    public DateTime GetLastAccessTime(string path)
    {
        return this.GetLastAccessTimeUtc(path).ToLocalTime();
    }

    public DateTime GetLastAccessTimeUtc(string path)
    {
        return OfflineDates.NotFoundDateTimeUtc; // Offline file system does not support last access time
    }

    public DateTime GetLastWriteTime(string path)
    {
        return this.GetLastWriteTimeUtc(path).ToLocalTime();
    }

    public DateTime GetLastWriteTimeUtc(string path)
    {
        var item = this.configuration.GetItemByPath(path);
        return item?.Modified ?? OfflineDates.NotFoundDateTimeUtc;
    }

    public void Move(string sourceFileName, string destFileName)
    {
        throw new NotSupportedException();
    }

    public void Move(string sourceFileName, string destFileName, bool overwrite)
    {
        throw new NotSupportedException();
    }

    public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
    {
        throw new NotSupportedException();
    }

    public Stream Open(string path, FileMode mode, FileAccess access)
    {
        throw new NotSupportedException();
    }

    public Stream Open(string path, FileMode mode)
    {
        throw new NotSupportedException();
    }

    public Stream Open(string path, FileStreamOptions options)
    {
        throw new NotSupportedException();
    }

    public Stream OpenRead(string path)
    {
        throw new NotSupportedException();
    }

    public StreamReader OpenText(string path)
    {
        throw new NotSupportedException();
    }

    public Stream OpenWrite(string path)
    {
        throw new NotSupportedException();
    }

    public byte[] ReadAllBytes(string path)
    {
        throw new NotSupportedException();
    }

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public string[] ReadAllLines(string path, Encoding encoding)
    {
        throw new NotSupportedException();
    }

    public string[] ReadAllLines(string path)
    {
        throw new NotSupportedException();
    }

    public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public string ReadAllText(string path, Encoding encoding)
    {
        throw new NotSupportedException();
    }

    public string ReadAllText(string path)
    {
        throw new NotSupportedException();
    }

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
    {
        throw new NotSupportedException();
    }

    public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
    {
        throw new NotSupportedException();
    }

    public void SetAttributes(string path, FileAttributes fileAttributes)
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

    public void WriteAllBytes(string path, byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public void WriteAllBytes(string path, ReadOnlySpan<byte> bytes)
    {
        throw new NotSupportedException();
    }

    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public void WriteAllLines(string path, string[] contents, Encoding encoding)
    {
        throw new NotSupportedException();
    }

    public void WriteAllLines(string path, string[] contents)
    {
        throw new NotSupportedException();
    }

    public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
        throw new NotSupportedException();
    }

    public void WriteAllLines(string path, IEnumerable<string> contents)
    {
        throw new NotSupportedException();
    }

    public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public void WriteAllText(string path, string? contents)
    {
        throw new NotSupportedException();
    }

    public void WriteAllText(string path, string? contents, Encoding encoding)
    {
        throw new NotSupportedException();
    }

    public void WriteAllText(string path, ReadOnlySpan<char> contents)
    {
        throw new NotSupportedException();
    }

    public void WriteAllText(string path, ReadOnlySpan<char> contents, Encoding encoding)
    {
        throw new NotSupportedException();
    }
}
