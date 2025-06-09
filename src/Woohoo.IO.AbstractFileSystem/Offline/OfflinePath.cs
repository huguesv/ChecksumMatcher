// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline;

using System;
using System.Diagnostics.CodeAnalysis;

public sealed class OfflinePath : IPath
{
    public char AltDirectorySeparatorChar => throw new NotImplementedException();

    public char DirectorySeparatorChar => throw new NotImplementedException();

    public char PathSeparator => throw new NotImplementedException();

    public char VolumeSeparatorChar => throw new NotImplementedException();

    [return: NotNullIfNotNull(nameof(path))]
    public string? ChangeExtension(string? path, string? extension)
    {
        throw new NotImplementedException();
    }

    public string Combine(scoped ReadOnlySpan<string> paths)
    {
        throw new NotImplementedException();
    }

    public string Combine(params string[] paths)
    {
        throw new NotImplementedException();
    }

    public string Combine(string path1, string path2)
    {
        throw new NotImplementedException();
    }

    public string Combine(string path1, string path2, string path3)
    {
        throw new NotImplementedException();
    }

    public string Combine(string path1, string path2, string path3, string path4)
    {
        throw new NotImplementedException();
    }

    public bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    public bool EndsInDirectorySeparator([NotNullWhen(true)] string? path)
    {
        throw new NotImplementedException();
    }

    public bool Exists([NotNullWhen(true)] string? path)
    {
        throw new NotImplementedException();
    }

    public string? GetDirectoryName(string? path)
    {
        throw new NotImplementedException();
    }

    public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    [return: NotNullIfNotNull(nameof(path))]
    public string? GetExtension(string? path)
    {
        throw new NotImplementedException();
    }

    public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    [return: NotNullIfNotNull(nameof(path))]
    public string? GetFileName(string? path)
    {
        throw new NotImplementedException();
    }

    public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    [return: NotNullIfNotNull(nameof(path))]
    public string? GetFileNameWithoutExtension(string? path)
    {
        throw new NotImplementedException();
    }

    public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    public string GetFullPath(string path)
    {
        throw new NotImplementedException();
    }

    public string GetFullPath(string path, string basePath)
    {
        throw new NotImplementedException();
    }

    public char[] GetInvalidFileNameChars()
    {
        throw new NotImplementedException();
    }

    public char[] GetInvalidPathChars()
    {
        throw new NotImplementedException();
    }

    public string? GetPathRoot(string? path)
    {
        throw new NotImplementedException();
    }

    public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    public string GetRandomFileName()
    {
        throw new NotImplementedException();
    }

    public string GetRelativePath(string relativeTo, string path)
    {
        throw new NotImplementedException();
    }

    public string GetTempFileName()
    {
        throw new NotImplementedException();
    }

    public string GetTempPath()
    {
        throw new NotImplementedException();
    }

    public bool HasExtension([NotNullWhen(true)] string? path)
    {
        throw new NotImplementedException();
    }

    public bool HasExtension(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    public bool IsPathFullyQualified(string path)
    {
        throw new NotImplementedException();
    }

    public bool IsPathFullyQualified(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    public bool IsPathRooted([NotNullWhen(true)] string? path)
    {
        throw new NotImplementedException();
    }

    public bool IsPathRooted(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    public string Join(string? path1, string? path2, string? path3, string? path4)
    {
        throw new NotImplementedException();
    }

    public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4)
    {
        throw new NotImplementedException();
    }

    public string Join(string? path1, string? path2, string? path3)
    {
        throw new NotImplementedException();
    }

    public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3)
    {
        throw new NotImplementedException();
    }

    public string Join(scoped ReadOnlySpan<string?> paths)
    {
        throw new NotImplementedException();
    }

    public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
    {
        throw new NotImplementedException();
    }

    public string Join(params string?[] paths)
    {
        throw new NotImplementedException();
    }

    public string Join(string? path1, string? path2)
    {
        throw new NotImplementedException();
    }

    public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    public string TrimEndingDirectorySeparator(string path)
    {
        throw new NotImplementedException();
    }

    public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination, out int charsWritten)
    {
        throw new NotImplementedException();
    }

    public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten)
    {
        throw new NotImplementedException();
    }
}
