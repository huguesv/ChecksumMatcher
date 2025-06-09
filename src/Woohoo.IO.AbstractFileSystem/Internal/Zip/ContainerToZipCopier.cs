// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.Zip;

using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Woohoo.IO.Compression.Zip;

internal class ContainerToZipCopier : IFileCopier
{
    protected virtual bool Compress => true;

    public virtual int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);

        return targetContainerType == "zip" ? 1 : 0;
    }

    public virtual bool Copy(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles)
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
                this.CopyFromContainerToZipArchive(file, fileName, targetZipFile);
            }
        }
        else
        {
            using (var targetZipFile = ZipFile.Create(targetArchiveFilePath))
            {
                this.CopyFromContainerToZipArchive(file, fileName, targetZipFile);
            }
        }

        // if (!_cancel)
        return this.PostProcess(targetArchiveFilePath, expectedTargetFiles);
    }

    public virtual string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        return Path.Combine(targetFolderPath, containerName + ".zip");
    }

    protected virtual bool PostProcess(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        return SharpZipContainer.IsComplete(targetArchiveFilePath, expectedTargetFiles);
    }

    protected virtual void CopyFromContainerToZipArchive(FileInformation file, string targetFile, ZipFile targetZipFile)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFile);
        ArgumentNullException.ThrowIfNull(targetZipFile);

        var sourceContainer = ContainerExtensionProvider.GetContainer(file.ContainerAbsolutePath);
        if (sourceContainer != null)
        {
            var tempFilePath = Path.GetTempFileName();
            sourceContainer.Copy(file, tempFilePath);
            try
            {
                this.CopyFromFileToZipArchive(tempFilePath, targetZipFile, targetFile);
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

    protected virtual void CopyFromFileToZipArchive(string sourceFilePath, ZipFile targetZipFile, string targetFile)
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
    }
}
