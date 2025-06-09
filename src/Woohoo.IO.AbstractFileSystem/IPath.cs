// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System.Diagnostics.CodeAnalysis;

public interface IPath
{
    char AltDirectorySeparatorChar { get; }

    char DirectorySeparatorChar { get; }

    char PathSeparator { get; }

    char VolumeSeparatorChar { get; }

    [return: NotNullIfNotNull(nameof(path))]
    string? ChangeExtension(string? path, string? extension);

    string Combine(scoped ReadOnlySpan<string> paths);

    string Combine(params string[] paths);

    string Combine(string path1, string path2);

    string Combine(string path1, string path2, string path3);

    string Combine(string path1, string path2, string path3, string path4);

    bool EndsInDirectorySeparator(ReadOnlySpan<char> path);

    bool EndsInDirectorySeparator([NotNullWhen(true)] string? path);

    bool Exists([NotNullWhen(true)] string? path);

    string? GetDirectoryName(string? path);

    ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path);

    [return: NotNullIfNotNull(nameof(path))]
    string? GetExtension(string? path);

    ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path);

    [return: NotNullIfNotNull(nameof(path))]
    string? GetFileName(string? path);

    ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path);

    [return: NotNullIfNotNull(nameof(path))]
    string? GetFileNameWithoutExtension(string? path);

    ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path);

    string GetFullPath(string path);

    string GetFullPath(string path, string basePath);

    char[] GetInvalidFileNameChars();

    char[] GetInvalidPathChars();

    string? GetPathRoot(string? path);

    ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path);

    string GetRandomFileName();

    string GetRelativePath(string relativeTo, string path);

    string GetTempFileName();

    string GetTempPath();

    bool HasExtension([NotNullWhen(true)] string? path);

    bool HasExtension(ReadOnlySpan<char> path);

    bool IsPathFullyQualified(string path);

    bool IsPathFullyQualified(ReadOnlySpan<char> path);

    bool IsPathRooted([NotNullWhen(true)] string? path);

    bool IsPathRooted(ReadOnlySpan<char> path);

    string Join(string? path1, string? path2, string? path3, string? path4);

    string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4);

    string Join(string? path1, string? path2, string? path3);

    string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3);

    string Join(scoped ReadOnlySpan<string?> paths);

    string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2);

    string Join(params string?[] paths);

    string Join(string? path1, string? path2);

    ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path);

    string TrimEndingDirectorySeparator(string path);

    bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination, out int charsWritten);

    bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten);
}
