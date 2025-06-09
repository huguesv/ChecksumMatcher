// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.TorrentSevenZip;

using System;
using System.IO;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.IO.Compression.TorrentSevenZip;

internal class FolderSingleFileToTorrentSevenZipCopier : IFileCopier
{
    public int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        if (Directory.Exists(file.ContainerAbsolutePath))
        {
            if (targetContainerType == KnownContainerTypes.TorrentSevenZip)
            {
                if (expectedTargetFiles.Length == 1)
                {
                    return 15;
                }
            }
        }

        return 0;
    }

    public async Task CopyAsync(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        var targetFilePath = this.GetTargetContainerPath(targetFolderPath, containerName);
        var originalFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        var tempFolderPath = Path.Combine(Path.GetDirectoryName(originalFilePath) ?? throw new NotSupportedException(), Path.GetRandomFileName());
        var tempFilePath = Path.Combine(tempFolderPath, fileName);

        try
        {
            _ = Directory.CreateDirectory(tempFolderPath);
            File.Move(originalFilePath, tempFilePath);

            await TorrentSevenZipper.Create7zAsync(targetFilePath, tempFilePath, ct);
        }
        finally
        {
            File.Move(tempFilePath, originalFilePath);
            if (Directory.Exists(tempFolderPath))
            {
                try
                {
                    Directory.Delete(tempFolderPath);
                }
                catch (IOException)
                {
                }
            }
        }

        if (removeSource)
        {
            if (File.Exists(originalFilePath))
            {
                FileUtility.SafeDelete(originalFilePath);
            }
        }
    }

    public string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        return Path.Combine(targetFolderPath, containerName + ".7z");
    }
}
