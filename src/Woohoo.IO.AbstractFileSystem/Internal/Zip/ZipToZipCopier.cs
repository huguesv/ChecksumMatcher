// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.Zip;

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Woohoo.IO.Compression.Zip;

internal class ZipToZipCopier : IFileCopier
{
    protected virtual bool Compress => true;

    public virtual int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        if (file == null)
        {
            throw new ArgumentNullException("file");
        }

        if (string.IsNullOrEmpty(targetContainerType))
        {
            throw new ArgumentNullException("targetContainerType");
        }

        if (string.Compare(Path.GetExtension(file.ContainerAbsolutePath), ".zip", StringComparison.OrdinalIgnoreCase) == 0)
        {
            if (targetContainerType == "zip")
            {
                return 10;
            }
        }

        return 0;
    }

    public virtual bool Copy(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles)
    {
        if (file == null)
        {
            throw new ArgumentNullException("file");
        }

        if (string.IsNullOrEmpty(targetFolderPath))
        {
            throw new ArgumentNullException("targetFolderPath");
        }

        if (string.IsNullOrEmpty(containerName))
        {
            throw new ArgumentNullException("containerName");
        }

        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException("fileName");
        }

        if (expectedTargetFiles == null)
        {
            throw new ArgumentNullException("expectedTargetFiles");
        }

        var targetArchiveFilePath = Path.Combine(targetFolderPath, containerName + ".zip");
        if (File.Exists(targetArchiveFilePath))
        {
            using (var targetZipFile = new ZipFile(targetArchiveFilePath))
            {
                using (var sourceZipFile = new ZipFile(file.ContainerAbsolutePath))
                {
                    this.CopyFromZipArchiveToZipArchive(sourceZipFile, file.FileRelativePath, targetZipFile, fileName);
                }
            }
        }
        else
        {
            var shouldCopyArchive = false;
            if (file.ContainerName == containerName &&
                file.FileRelativePath == fileName)
            {
                using (var sourceZipFile = new ZipFile(file.ContainerAbsolutePath))
                {
                    if (sourceZipFile.Count == 1)
                    {
                        shouldCopyArchive = true;
                    }
                }
            }

            if (shouldCopyArchive)
            {
                if (removeSource && allowContainerMove)
                {
                    File.Move(file.ContainerAbsolutePath, targetArchiveFilePath);
                }
                else
                {
                    File.Copy(file.ContainerAbsolutePath, targetArchiveFilePath);
                }
            }
            else
            {
                using (var targetZipFile = ZipFile.Create(targetArchiveFilePath))
                {
                    using (var sourceZipFile = new ZipFile(file.ContainerAbsolutePath))
                    {
                        this.CopyFromZipArchiveToZipArchive(sourceZipFile, file.FileRelativePath, targetZipFile, fileName);
                    }
                }
            }
        }

        // if (!_cancel)
        return this.PostProcess(targetArchiveFilePath, expectedTargetFiles);
    }

    public virtual string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        return Path.Combine(targetFolderPath, containerName + ".zip");
    }

    protected virtual bool PostProcess(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        return SharpZipContainer.IsComplete(targetArchiveFilePath, expectedTargetFiles);
    }

    protected virtual void CopyFromZipArchiveToZipArchive(ZipFile sourceZipFile, string sourceFile, ZipFile targetZipFile, string targetFile)
    {
        if (sourceZipFile == null)
        {
            throw new ArgumentNullException("sourceZipFile");
        }

        if (string.IsNullOrEmpty(sourceFile))
        {
            throw new ArgumentNullException("sourceFile");
        }

        if (targetZipFile == null)
        {
            throw new ArgumentNullException("targetZipFile");
        }

        if (string.IsNullOrEmpty(targetFile))
        {
            throw new ArgumentNullException("targetFile");
        }

        var sourceEntry = sourceZipFile.GetEntry(sourceFile);
        if (sourceEntry != null)
        {
            var sourceProvider = new ZipEntryZipStreamProvider(sourceZipFile, sourceEntry);

            targetZipFile.BeginUpdate();
            targetZipFile.Add(sourceProvider, targetFile, this.Compress ? CompressionMethod.Deflated : CompressionMethod.Stored);
            targetZipFile.CommitUpdate();
        }
    }
}
