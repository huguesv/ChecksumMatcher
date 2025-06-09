// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Folder;

using System.IO;

internal class ContainerToFolderCopier : IFileCopier
{
    public virtual int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);

        return targetContainerType == "folder" ? 1 : 0;
    }

    public virtual async Task CopyAsync(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        var sourceContainer = ContainerExtensionProvider.GetContainer(file.ContainerAbsolutePath);
        if (sourceContainer != null)
        {
            var targetGameFolderPath = Path.Combine(targetFolderPath, containerName);
            _ = Directory.CreateDirectory(targetGameFolderPath);

            var targetRomFilePath = Path.Combine(targetGameFolderPath, fileName);
            if (!File.Exists(targetRomFilePath))
            {
                if (allowContainerMove && removeSource)
                {
                    // Move is faster than a copy/delete, but this is only possible if the rom is not shared by multiple games
                    await sourceContainer.MoveAsync(file, targetRomFilePath, ct);
                }
                else
                {
                    await sourceContainer.CopyAsync(file, targetRomFilePath, ct);
                }
            }
            else
            {
                // Target file already exists.  This may be because the same rom is shared by many games.
            }
        }
    }

    public string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        return Path.Combine(targetFolderPath, containerName);
    }
}
