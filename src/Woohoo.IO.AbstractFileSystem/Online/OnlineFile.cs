// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class OnlineFile : IFile
{
    public void AppendAllBytes(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(bytes);

#if NET9_0_OR_GREATER
        File.AppendAllBytes(path, bytes);
#else
        throw new NotImplementedException();
#endif
    }

    public void AppendAllBytes(string path, ReadOnlySpan<byte> bytes)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        File.AppendAllBytes(path, bytes);
#else
        throw new NotImplementedException();
#endif
    }

    public Task AppendAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(bytes);

#if NET9_0_OR_GREATER
        return File.AppendAllBytesAsync(path, bytes, cancellationToken);
#else
        return Task.FromException(new NotImplementedException());
#endif
    }

    public Task AppendAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        return File.AppendAllBytesAsync(path, bytes, cancellationToken);
#else
        return Task.FromException(new NotImplementedException());
#endif
    }

    public void AppendAllLines(string path, IEnumerable<string> contents)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        File.AppendAllLines(path, contents);
    }

    public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        File.AppendAllLines(path, contents, encoding);
    }

    public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        return File.AppendAllLinesAsync(path, contents, cancellationToken);
    }

    public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        return File.AppendAllLinesAsync(path, contents, encoding, cancellationToken);
    }

    public void AppendAllText(string path, string? contents, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.AppendAllText(path, contents, encoding);
    }

    public void AppendAllText(string path, string? contents)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.AppendAllText(path, contents);
    }

    public void AppendAllText(string path, ReadOnlySpan<char> contents)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        File.AppendAllText(path, contents);
#else
        throw new NotImplementedException();
#endif
    }

    public void AppendAllText(string path, ReadOnlySpan<char> contents, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        File.AppendAllText(path, contents, encoding);
#else
        throw new NotImplementedException();
#endif
    }

    public Task AppendAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.AppendAllTextAsync(path, contents, encoding, cancellationToken);
    }

    public Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        return File.AppendAllTextAsync(path, contents, encoding, cancellationToken);
#else
        return Task.FromException(new NotImplementedException());
#endif
    }

    public Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        return File.AppendAllTextAsync(path, contents, cancellationToken);
#else
        return Task.FromException(new NotImplementedException());
#endif
    }

    public Task AppendAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.AppendAllTextAsync(path, contents, cancellationToken);
    }

    public StreamWriter AppendText(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.AppendText(path);
    }

    public void Copy(string sourceFileName, string destFileName, bool overwrite)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentException.ThrowIfNullOrEmpty(destFileName);

        File.Copy(sourceFileName, destFileName, overwrite);
    }

    public void Copy(string sourceFileName, string destFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentException.ThrowIfNullOrEmpty(destFileName);

        File.Copy(sourceFileName, destFileName);
    }

    public Stream Create(string path, int bufferSize, FileOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.Create(path, bufferSize, options);
    }

    public Stream Create(string path, int bufferSize)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.Create(path, bufferSize);
    }

    public Stream Create(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.Create(path);
    }

    public StreamWriter CreateText(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.CreateText(path);
    }

    public void Decrypt(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.Decrypt(path);
    }

    public void Delete(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.Delete(path);
    }

    public void Encrypt(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.Encrypt(path);
    }

    public bool Exists([NotNullWhen(true)] string? path)
    {
        return File.Exists(path);
    }

    public FileAttributes GetAttributes(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.GetAttributes(path);
    }

    public DateTime GetCreationTime(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.GetCreationTime(path);
    }

    public DateTime GetCreationTimeUtc(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.GetCreationTimeUtc(path);
    }

    public DateTime GetLastAccessTime(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.GetLastAccessTime(path);
    }

    public DateTime GetLastAccessTimeUtc(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.GetLastAccessTimeUtc(path);
    }

    public DateTime GetLastWriteTime(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.GetLastWriteTime(path);
    }

    public DateTime GetLastWriteTimeUtc(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.GetLastWriteTimeUtc(path);
    }

    public void Move(string sourceFileName, string destFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentException.ThrowIfNullOrEmpty(destFileName);

        File.Move(sourceFileName, destFileName);
    }

    public void Move(string sourceFileName, string destFileName, bool overwrite)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentException.ThrowIfNullOrEmpty(destFileName);

        File.Move(sourceFileName, destFileName, overwrite);
    }

    public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.Open(path, mode, access, share);
    }

    public Stream Open(string path, FileMode mode, FileAccess access)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.Open(path, mode, access);
    }

    public Stream Open(string path, FileMode mode)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.Open(path, mode);
    }

    public Stream Open(string path, FileStreamOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.Open(path, options);
    }

    public Stream OpenRead(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.OpenRead(path);
    }

    public StreamReader OpenText(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.OpenText(path);
    }

    public Stream OpenWrite(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.OpenWrite(path);
    }

    public byte[] ReadAllBytes(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllBytes(path);
    }

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllBytesAsync(path, cancellationToken);
    }

    public string[] ReadAllLines(string path, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllLines(path, encoding);
    }

    public string[] ReadAllLines(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllLines(path);
    }

    public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllLinesAsync(path, cancellationToken);
    }

    public Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllLinesAsync(path, encoding, cancellationToken);
    }

    public string ReadAllText(string path, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllText(path, encoding);
    }

    public string ReadAllText(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllText(path);
    }

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllTextAsync(path, cancellationToken);
    }

    public Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return File.ReadAllTextAsync(path, encoding, cancellationToken);
    }

    public void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentException.ThrowIfNullOrEmpty(destinationFileName);

        File.Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
    }

    public void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);
        ArgumentException.ThrowIfNullOrEmpty(destinationFileName);

        File.Replace(sourceFileName, destinationFileName, destinationBackupFileName);
    }

    public void SetAttributes(string path, FileAttributes fileAttributes)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.SetAttributes(path, fileAttributes);
    }

    public void SetCreationTime(string path, DateTime creationTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.SetCreationTime(path, creationTime);
    }

    public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.SetCreationTimeUtc(path, creationTimeUtc);
    }

    public void SetLastAccessTime(string path, DateTime lastAccessTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.SetLastAccessTime(path, lastAccessTime);
    }

    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
    }

    public void SetLastWriteTime(string path, DateTime lastWriteTime)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.SetLastWriteTime(path, lastWriteTime);
    }

    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
    }

    public void WriteAllBytes(string path, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(bytes);

        File.WriteAllBytes(path, bytes);
    }

    public void WriteAllBytes(string path, ReadOnlySpan<byte> bytes)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        File.WriteAllBytes(path, bytes);
#else
        throw new NotImplementedException();
#endif
    }

    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(bytes);

        return File.WriteAllBytesAsync(path, bytes, cancellationToken);
    }

    public Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        return File.WriteAllBytesAsync(path, bytes, cancellationToken);
#else
        return Task.FromException(new NotImplementedException());
#endif
    }

    public void WriteAllLines(string path, string[] contents, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        File.WriteAllLines(path, contents, encoding);
    }

    public void WriteAllLines(string path, string[] contents)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        File.WriteAllLines(path, contents);
    }

    public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        File.WriteAllLines(path, contents, encoding);
    }

    public void WriteAllLines(string path, IEnumerable<string> contents)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        File.WriteAllLines(path, contents);
    }

    public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        return File.WriteAllLinesAsync(path, contents, cancellationToken);
    }

    public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(contents);

        return File.WriteAllLinesAsync(path, contents, encoding, cancellationToken);
    }

    public void WriteAllText(string path, string? contents, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.WriteAllText(path, contents, encoding);
    }

    public void WriteAllText(string path, string? contents)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        File.WriteAllText(path, contents);
    }

    public void WriteAllText(string path, ReadOnlySpan<char> contents)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        File.WriteAllText(path, contents);
#else
        throw new NotImplementedException();
#endif
    }

    public void WriteAllText(string path, ReadOnlySpan<char> contents, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

#if NET9_0_OR_GREATER
        File.WriteAllText(path, contents, encoding);
#else
        throw new NotImplementedException();
#endif
    }
}
