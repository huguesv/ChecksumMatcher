// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.SevenZip;

using System;
using System.Collections.Generic;
using System.IO;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.IO.AbstractFileSystem;
using Woohoo.IO.Compression.SevenZip;

internal sealed class SevenZipContainer : IContainer
{
    string IContainer.FileExtension => ".7z";

    public static Task<bool> IsCompleteAsync(string targetArchiveFilePath, string[] expectedTargetFiles, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        var targetZipFile = new SevenZipFile(targetArchiveFilePath);
        var isComplete = targetZipFile.Entries.Count == expectedTargetFiles.Length;

        return Task.FromResult(isComplete);
    }

    Task<FileInformation[]> IContainer.GetAllFilesAsync(string containerFilePath, SearchOption searchOption, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(containerFilePath);

        ct.ThrowIfCancellationRequested();

        var files = new List<FileInformation>();

        try
        {
            var archive = new SevenZipFile(containerFilePath);
            foreach (var entry in archive.Entries)
            {
                var file = new FileInformation
                {
                    ContainerIsFolder = false,
                    ContainerAbsolutePath = containerFilePath,
                    FileRelativePath = entry.Name,
                    Size = (long)entry.Size,
                    ReportedCRC32 = ChecksumConversion.ToByteArray((uint)entry.CRC32),
                    LastWriteTime = entry.LastWriteTime,
                    CreationTime = entry.CreationTime,
                    IsDirectory = entry.IsDirectory,
                    CompressionMethod = entry.Method,
                };

                files.Add(file);
            }
        }
        catch (Exception)
        {
        }

        return Task.FromResult(files.ToArray());
    }

    async Task IContainer.CalculateChecksumsAsync(FileInformation file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (File.Exists(file.ContainerAbsolutePath))
        {
            try
            {
                var archive = new SevenZipFile(file.ContainerAbsolutePath);

                var entry = archive.GetEntry(file.FileRelativePath);
                if (entry is not null)
                {
                    var tempFilePath = Path.GetTempFileName();
                    try
                    {
                        archive.Extract(entry, tempFilePath);
                        using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                        {
                            await ChecksumService.CalculateAllAsync(file, stream, stream.Length, ct);
                        }
                    }
                    finally
                    {
                        File.Delete(tempFilePath);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }

    Task<bool> IContainer.ExistsAsync(FileInformation file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (File.Exists(file.ContainerAbsolutePath))
        {
            var archive = new SevenZipFile(file.ContainerAbsolutePath);
            var entry = archive.GetEntry(file.FileRelativePath);
            return Task.FromResult(entry is not null);
        }

        return Task.FromResult(false);
    }

    Task IContainer.CopyAsync(FileInformation file, string targetFilePath, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (string.IsNullOrEmpty(targetFilePath))
        {
            throw new ArgumentNullException(nameof(targetFilePath));
        }

        var archive = new SevenZipFile(file.ContainerAbsolutePath);
        var entry = archive.GetEntry(file.FileRelativePath);
        if (entry is not null)
        {
            archive.Extract(entry, targetFilePath);
        }

        return Task.CompletedTask;
    }

    async Task IContainer.MoveAsync(FileInformation file, string targetFilePath, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        await (this as IContainer).CopyAsync(file, targetFilePath, ct);
        await (this as IContainer).RemoveAsync(file, ct);
    }

    async Task IContainer.RemoveAsync(FileInformation file, CancellationToken ct)
    {
        // If the archive contains only the file we want to delete, we'll delete the archive
        var files = (await (this as IContainer).GetAllFilesAsync(file.ContainerAbsolutePath, SearchOption.AllDirectories, ct)).ToArray();
        if (files.Length == 1 && files[0].FileRelativePath == file.FileRelativePath)
        {
            FileUtility.SafeDelete(file.ContainerAbsolutePath);
        }

        // Otherwise, we cannot remove the item from the archive
    }

    public Task RemoveContainerAsync(string containerFilePath, CancellationToken ct)
    {
        FileUtility.SafeDelete(containerFilePath);

        return Task.CompletedTask;
    }
}
