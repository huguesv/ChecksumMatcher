// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.IO.AbstractFileSystem;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public static class FileSystem
{
    private static readonly IContainer FolderContainer = ContainerExtensionProvider.GetFolderContainer();

    public static FileInformation[] GetAllFiles(string folderPath, SearchOption option)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderPath);

        var files = new List<FileInformation>();

        files.AddRange(FolderContainer.GetAllFiles(folderPath, option));

        return files.ToArray();
    }

    public static IEnumerable<FileInformation> GetAllFiles(OfflineStorage indexedStorage, SearchOption option)
    {
        var files = new List<FileInformation>();

        var indexedFolder = indexedStorage.Disk.GetItemByPath(indexedStorage.FolderPath);
        if (indexedFolder is not null)
        {
            foreach (var containerItem in indexedFolder.Items.Where(ii => ii.Kind == OfflineItemKind.Folder))
            {
                foreach (var fileItem in containerItem.Items.Where(ii => ii.Kind == OfflineItemKind.File))
                {
                    var file = new FileInformation
                    {
                        ContainerIsFolder = true,
                        ContainerAbsolutePath = containerItem.Path,
                        FileRelativePath = fileItem.Name,
                        Size = fileItem.Size ?? 0,
                        DataBlockSize = fileItem.Size ?? 0,
                        ReportedCRC32 = ByteArrayUtility.HexToByteArray(fileItem.ReportedCRC32),
                        CRC32 = ByteArrayUtility.HexToByteArray(fileItem.CRC32),
                        MD5 = ByteArrayUtility.HexToByteArray(fileItem.MD5),
                        SHA1 = ByteArrayUtility.HexToByteArray(fileItem.SHA1),
                        SHA256 = ByteArrayUtility.HexToByteArray(fileItem.SHA256),
                    };
                    files.Add(file);
                }
            }

            foreach (var containerItem in indexedFolder.Items.Where(ii => ii.Kind == OfflineItemKind.ArchiveFile))
            {
                foreach (var fileItem in containerItem.Items.Where(ii => ii.Kind == OfflineItemKind.File))
                {
                    var file = new FileInformation
                    {
                        ContainerIsFolder = false,
                        ContainerAbsolutePath = containerItem.Path,
                        FileRelativePath = fileItem.Name,
                        Size = fileItem.Size ?? 0,
                        DataBlockSize = fileItem.Size ?? 0,
                        ReportedCRC32 = ByteArrayUtility.HexToByteArray(fileItem.ReportedCRC32),
                        CRC32 = ByteArrayUtility.HexToByteArray(fileItem.CRC32),
                        MD5 = ByteArrayUtility.HexToByteArray(fileItem.MD5),
                        SHA1 = ByteArrayUtility.HexToByteArray(fileItem.SHA1),
                        SHA256 = ByteArrayUtility.HexToByteArray(fileItem.SHA256),
                    };
                    files.Add(file);
                }
            }
        }

        return files.ToArray();
    }
}
