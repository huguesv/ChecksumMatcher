// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System.Diagnostics.CodeAnalysis;

public interface IFile
{
    void AppendAllBytes(string path, byte[] bytes);

    void AppendAllBytes(string path, ReadOnlySpan<byte> bytes);

    System.Threading.Tasks.Task AppendAllBytesAsync(string path, byte[] bytes, System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task AppendAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, System.Threading.CancellationToken cancellationToken = default);

    void AppendAllLines(string path, System.Collections.Generic.IEnumerable<string> contents);

    void AppendAllLines(string path, System.Collections.Generic.IEnumerable<string> contents, System.Text.Encoding encoding);

    System.Threading.Tasks.Task AppendAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> contents, System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task AppendAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> contents, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default);

    void AppendAllText(string path, ReadOnlySpan<char> contents);

    void AppendAllText(string path, ReadOnlySpan<char> contents, System.Text.Encoding encoding);

    void AppendAllText(string path, string? contents);

    void AppendAllText(string path, string? contents, System.Text.Encoding encoding);

    System.Threading.Tasks.Task AppendAllTextAsync(string path, string? contents, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task AppendAllTextAsync(string path, ReadOnlyMemory<char> contents, System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task AppendAllTextAsync(string path, string? contents, System.Threading.CancellationToken cancellationToken = default);

    System.IO.StreamWriter AppendText(string path);

    void Copy(string sourceFileName, string destFileName, bool overwrite);

    void Copy(string sourceFileName, string destFileName);

    System.IO.Stream Create(string path, int bufferSize, System.IO.FileOptions options);

    System.IO.Stream Create(string path, int bufferSize);

    System.IO.Stream Create(string path);

    System.IO.StreamWriter CreateText(string path);

    void Decrypt(string path);

    void Delete(string path);

    void Encrypt(string path);

    bool Exists([NotNullWhen(true)] string? path);

    System.IO.FileAttributes GetAttributes(string path);

    System.DateTime GetCreationTime(string path);

    System.DateTime GetCreationTimeUtc(string path);

    System.DateTime GetLastAccessTime(string path);

    System.DateTime GetLastAccessTimeUtc(string path);

    System.DateTime GetLastWriteTime(string path);

    System.DateTime GetLastWriteTimeUtc(string path);

    void Move(string sourceFileName, string destFileName);

    void Move(string sourceFileName, string destFileName, bool overwrite);

    System.IO.Stream Open(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share);

    System.IO.Stream Open(string path, System.IO.FileMode mode, System.IO.FileAccess access);

    System.IO.Stream Open(string path, System.IO.FileMode mode);

    System.IO.Stream Open(string path, System.IO.FileStreamOptions options);

    System.IO.Stream OpenRead(string path);

    System.IO.StreamReader OpenText(string path);

    System.IO.Stream OpenWrite(string path);

    byte[] ReadAllBytes(string path);

    System.Threading.Tasks.Task<byte[]> ReadAllBytesAsync(string path, System.Threading.CancellationToken cancellationToken = default);

    string[] ReadAllLines(string path, System.Text.Encoding encoding);

    string[] ReadAllLines(string path);

    System.Threading.Tasks.Task<string[]> ReadAllLinesAsync(string path, System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task<string[]> ReadAllLinesAsync(string path, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default);

    string ReadAllText(string path, System.Text.Encoding encoding);

    string ReadAllText(string path);

    System.Threading.Tasks.Task<string> ReadAllTextAsync(string path, System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task<string> ReadAllTextAsync(string path, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default);

    void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors);

    void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName);

    void SetAttributes(string path, System.IO.FileAttributes fileAttributes);

    void SetCreationTime(string path, System.DateTime creationTime);

    void SetCreationTimeUtc(string path, System.DateTime creationTimeUtc);

    void SetLastAccessTime(string path, System.DateTime lastAccessTime);

    void SetLastAccessTimeUtc(string path, System.DateTime lastAccessTimeUtc);

    void SetLastWriteTime(string path, System.DateTime lastWriteTime);

    void SetLastWriteTimeUtc(string path, System.DateTime lastWriteTimeUtc);

    void WriteAllBytes(string path, byte[] bytes);

    void WriteAllBytes(string path, ReadOnlySpan<byte> bytes);

    System.Threading.Tasks.Task WriteAllBytesAsync(string path, byte[] bytes, System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> bytes, System.Threading.CancellationToken cancellationToken = default);

    void WriteAllLines(string path, string[] contents, System.Text.Encoding encoding);

    void WriteAllLines(string path, string[] contents);

    void WriteAllLines(string path, System.Collections.Generic.IEnumerable<string> contents, System.Text.Encoding encoding);

    void WriteAllLines(string path, System.Collections.Generic.IEnumerable<string> contents);

    System.Threading.Tasks.Task WriteAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> contents, System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task WriteAllLinesAsync(string path, System.Collections.Generic.IEnumerable<string> contents, System.Text.Encoding encoding, System.Threading.CancellationToken cancellationToken = default);

    void WriteAllText(string path, string? contents);

    void WriteAllText(string path, string? contents, System.Text.Encoding encoding);

    void WriteAllText(string path, ReadOnlySpan<char> contents);

    void WriteAllText(string path, ReadOnlySpan<char> contents, System.Text.Encoding encoding);
}
