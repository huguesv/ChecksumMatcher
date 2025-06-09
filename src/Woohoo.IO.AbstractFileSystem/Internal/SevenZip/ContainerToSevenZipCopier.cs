// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.SevenZip;

using System.IO;
using System.Reflection;
using global::SevenZip;
using Woohoo.IO.Compression.SevenZip;

internal class ContainerToSevenZipCopier : IFileCopier
{
    static ContainerToSevenZipCopier()
    {
        SevenZipLibrary.Initialize();
    }

    protected virtual bool Compress => true;

    public virtual int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);

        return targetContainerType == "7z" ? 1 : 0;
    }

    public virtual bool Copy(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        var targetArchiveFilePath = Path.Combine(targetFolderPath, containerName + ".7z");
        this.CopyFromContainerToSevenZipArchive(file, fileName, targetArchiveFilePath);

        // if (!_cancel)
        return this.PostProcess(targetArchiveFilePath, expectedTargetFiles);
    }

    public virtual string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        return Path.Combine(targetFolderPath, containerName + ".7z");
    }

    protected virtual bool PostProcess(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        return SevenZipContainer.IsComplete(targetArchiveFilePath, expectedTargetFiles);
    }

    protected virtual void CopyFromContainerToSevenZipArchive(FileInformation file, string targetFile, string targetArchiveFilePath)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFile);
        ArgumentNullException.ThrowIfNull(targetArchiveFilePath);

        var sourceContainer = ContainerExtensionProvider.GetContainer(file.ContainerAbsolutePath);
        if (sourceContainer != null)
        {
            var tempFilePath = Path.GetTempFileName();
            sourceContainer.Copy(file, tempFilePath);
            try
            {
                FolderToSevenZipCopier.BuildArchive(tempFilePath, targetArchiveFilePath, targetFile);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
    }
}
