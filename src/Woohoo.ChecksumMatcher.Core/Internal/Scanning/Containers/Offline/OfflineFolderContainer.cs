// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Offline;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Woohoo.IO.AbstractFileSystem;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

internal sealed class OfflineFolderContainer : IContainer
{
    private readonly OfflineDisk offlineDisk;

    public OfflineFolderContainer(OfflineDisk offlineDisk)
    {
        this.offlineDisk = offlineDisk;
    }

    public string FileExtension => throw new NotImplementedException();

    public FileInformation[] GetAllFiles(string containerFilePath, SearchOption searchOption, CancellationToken ct)
    {
        var indexedFolder = this.offlineDisk.GetItemByPath(containerFilePath);
        if (indexedFolder is not null)
        {
            var folderFiles = indexedFolder
                .Items
                .Where(ii => ii.Kind == OfflineItemKind.Folder)
                .SelectMany(containerItem =>
                    containerItem.Items
                        .Where(ii => ii.Kind == OfflineItemKind.File)
                        .Select(fileItem => CreateFileInfo(containerIsFolder: true, containerItem, fileItem)));

            var archiveFiles = indexedFolder
                .Items
                .Where(ii => ii.Kind == OfflineItemKind.ArchiveFile)
                .SelectMany(containerItem =>
                    containerItem.Items
                        .Where(ii => ii.Kind == OfflineItemKind.File)
                        .Select(fileItem => CreateFileInfo(containerIsFolder: false, containerItem, fileItem)));

            return folderFiles.Concat(archiveFiles).ToArray();
        }

        return [];

        static FileInformation CreateFileInfo(bool containerIsFolder, OfflineItem containerItem, OfflineItem fileItem)
        {
            return new FileInformation
            {
                ContainerIsFolder = containerIsFolder,
                ContainerAbsolutePath = containerItem.Path,
                FileRelativePath = fileItem.Name,
                Size = fileItem.Size ?? 0,
                DataBlockSize = fileItem.Size ?? 0,
                ReportedCRC32 = ChecksumConversion.ToByteArray(fileItem.ReportedCRC32),
                CRC32 = ChecksumConversion.ToByteArray(fileItem.CRC32),
                MD5 = ChecksumConversion.ToByteArray(fileItem.MD5),
                SHA1 = ChecksumConversion.ToByteArray(fileItem.SHA1),
                SHA256 = ChecksumConversion.ToByteArray(fileItem.SHA256),
            };
        }
    }

    public void CalculateChecksums(FileInformation file, CancellationToken ct)
    {
        throw new NotSupportedException();
    }

    public bool Exists(FileInformation file)
    {
        return this.offlineDisk.GetItemByPath(Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath)) is not null;
    }

    public void Copy(FileInformation file, string targetFilePath)
    {
        throw new NotSupportedException();
    }

    public void Move(FileInformation file, string targetFilePath)
    {
        throw new NotSupportedException();
    }

    public void Remove(FileInformation file)
    {
        throw new NotSupportedException();
    }

    public void RemoveContainer(string containerFilePath)
    {
        throw new NotSupportedException();
    }
}
