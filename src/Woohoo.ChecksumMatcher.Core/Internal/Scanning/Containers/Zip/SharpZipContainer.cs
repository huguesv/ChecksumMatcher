// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Zip;

using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;
using Woohoo.IO.AbstractFileSystem;
using Woohoo.IO.Compression.Zip;

internal sealed class SharpZipContainer : IContainer
{
    string IContainer.FileExtension => ".zip";

    public static Task<bool> IsCompleteAsync(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        var isComplete = false;

        using (var targetZipFile = new ZipFile(targetArchiveFilePath))
        {
            isComplete = targetZipFile.Count == expectedTargetFiles.Length;
        }

        return Task.FromResult(isComplete);
    }

    Task<FileInformation[]> IContainer.GetAllFilesAsync(string containerFilePath, SearchOption searchOption, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(containerFilePath);

        ct.ThrowIfCancellationRequested();

        var files = new List<FileInformation>();

        try
        {
            using (var archive = new ZipFile(containerFilePath))
            {
                foreach (ZipEntry entry in archive)
                {
                    var file = new FileInformation
                    {
                        ContainerIsFolder = false,
                        ContainerAbsolutePath = containerFilePath,
                        FileRelativePath = entry.Name.Replace('/', '\\'),
                        Size = entry.Size,
                        CreationTime = null,
                        LastWriteTime = entry.DateTime,
                        IsDirectory = entry.IsDirectory,
                        CompressionMethod = entry.CompressionMethod.ToString(),
                    };

                    if (entry.HasCrc)
                    {
                        file.ReportedCRC32 = ChecksumConversion.ToByteArray((uint)entry.Crc);
                    }

                    files.Add(file);
                }
            }
        }
        catch (Exception)
        {
        }

        FileInformation[] result = [.. files];
        return Task.FromResult(result);
    }

    async Task IContainer.CalculateChecksumsAsync(FileInformation file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (File.Exists(file.ContainerAbsolutePath))
        {
            try
            {
                using (var zipArchive = new ZipFile(file.ContainerAbsolutePath))
                {
                    var entryIndex = zipArchive.FindEntry(file.FileRelativePath, true);
                    if (entryIndex != -1)
                    {
                        var entry = zipArchive[entryIndex];
                        if (entry != null)
                        {
                            using (var stream = zipArchive.GetInputStream(entry))
                            {
                                await ChecksumService.CalculateAllAsync(file, stream, entry.Size, ct);
                            }
                        }
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
            using (var sourceZipFile = new ZipFile(file.ContainerAbsolutePath))
            {
                var entry = sourceZipFile.GetEntry(file.FileRelativePath);
                return Task.FromResult(entry != null);
            }
        }

        return Task.FromResult(false);
    }

    async Task IContainer.CopyAsync(FileInformation file, string targetFilePath, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        using (var sourceZipFile = new ZipFile(file.ContainerAbsolutePath))
        {
            var entry = sourceZipFile.GetEntry(file.FileRelativePath);
            await sourceZipFile.ExtractAsync(entry, targetFilePath, ct);
        }
    }

    async Task IContainer.MoveAsync(FileInformation file, string targetFilePath, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        await (this as IContainer).CopyAsync(file, targetFilePath, ct);
        await (this as IContainer).RemoveAsync(file, ct);
    }

    Task IContainer.RemoveAsync(FileInformation file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        var shouldDeleteArchive = false;

        using (var sourceZipFile = new ZipFile(file.ContainerAbsolutePath))
        {
            var entry = sourceZipFile.GetEntry(file.FileRelativePath);
            if (entry != null)
            {
                if (sourceZipFile.Count == 1)
                {
                    // It's faster to just delete the archive
                    shouldDeleteArchive = true;
                }
                else
                {
                    sourceZipFile.BeginUpdate();
                    sourceZipFile.Delete(entry);
                    sourceZipFile.CommitUpdate();
                }
            }
        }

        if (shouldDeleteArchive)
        {
            FileUtility.SafeDelete(file.ContainerAbsolutePath);
        }

        return Task.CompletedTask;
    }

    public Task RemoveContainerAsync(string containerFilePath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(containerFilePath);

        FileUtility.SafeDelete(containerFilePath);

        return Task.CompletedTask;
    }
}
