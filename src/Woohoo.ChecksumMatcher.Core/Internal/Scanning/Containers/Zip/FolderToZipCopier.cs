// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Zip;

using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.IO.Compression.Zip;

internal class FolderToZipCopier : IFileCopier
{
    protected virtual bool Compress => true;

    public virtual int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        if (Directory.Exists(file.ContainerAbsolutePath))
        {
            if (targetContainerType == KnownContainerTypes.Zip)
            {
                return 5;
            }
        }

        return 0;
    }

    public virtual async Task CopyAsync(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        var targetArchiveFilePath = Path.Combine(targetFolderPath, containerName + ".zip");

        if (File.Exists(targetArchiveFilePath))
        {
            using (var targetZipFile = new ZipFile(targetArchiveFilePath))
            {
                var entry = targetZipFile.GetEntry(fileName);
                if (entry == null)
                {
                    await this.CopyFromFileToZipArchiveAsync(Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath), targetZipFile, fileName, ct);
                }
                else
                {
                    // Target file already exists.  This may be because the same rom is shared by many games.
                }
            }
        }
        else
        {
            using (var targetZipFile = ZipFile.Create(targetArchiveFilePath))
            {
                await this.CopyFromFileToZipArchiveAsync(Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath), targetZipFile, fileName, ct);
            }
        }

        await this.PostProcessAsync(targetArchiveFilePath, expectedTargetFiles, ct);
    }

    public virtual string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        return Path.Combine(targetFolderPath, containerName + ".zip");
    }

    protected virtual Task PostProcessAsync(string targetArchiveFilePath, string[] expectedTargetFiles, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    protected virtual Task CopyFromFileToZipArchiveAsync(string sourceFilePath, ZipFile targetZipFile, string targetFile, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFilePath);
        ArgumentNullException.ThrowIfNull(targetZipFile);
        ArgumentException.ThrowIfNullOrEmpty(targetFile);

        using (var sourceProvider = new ZipEntryFileStreamProvider(sourceFilePath))
        {
            targetZipFile.BeginUpdate();
            targetZipFile.Add(sourceProvider, targetFile, this.Compress ? CompressionMethod.Deflated : CompressionMethod.Stored);
            targetZipFile.CommitUpdate();
        }

        return Task.CompletedTask;
    }
}
