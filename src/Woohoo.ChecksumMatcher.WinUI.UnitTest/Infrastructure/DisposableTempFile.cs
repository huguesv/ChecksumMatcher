// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using System;

internal sealed class DisposableTempFile : IDisposable
{
    public DisposableTempFile()
    {
        this.FilePath = Path.GetTempFileName();
    }

    public string FilePath { get; }

    public string FileName => Path.GetFileName(this.FilePath) ?? string.Empty;

    public string FolderPath => Path.GetDirectoryName(this.FilePath) ?? string.Empty;

    public static DisposableTempFile WithBytes(byte[] content)
    {
        var tempFile = new DisposableTempFile();
        tempFile.WriteAllBytes(content);
        return tempFile;
    }

    public static DisposableTempFile WithText(string content)
    {
        var tempFile = new DisposableTempFile();
        tempFile.WriteAllText(content);
        return tempFile;
    }

    public void WriteAllText(string content)
    {
        File.WriteAllText(this.FilePath, content);
    }

    public void WriteAllBytes(byte[] content)
    {
        File.WriteAllBytes(this.FilePath, content);
    }

    public void Dispose()
    {
        if (File.Exists(this.FilePath))
        {
            File.Delete(this.FilePath);
        }
    }
}
