// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.TorrentSevenZip;

using System;
using System.IO;
using Woohoo.IO.AbstractFileSystem.Internal;
using Woohoo.IO.Compression.TorrentSevenZip;

internal class FolderSingleFileToTorrentSevenZipCopier : IFileCopier
{
    public int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        Requires.NotNull(file);
        Requires.NotNullOrEmpty(targetContainerType);

        if (Directory.Exists(file.ContainerAbsolutePath))
        {
            if (targetContainerType == "torrent7z")
            {
                if (expectedTargetFiles.Length == 1)
                {
                    return 15;
                }
            }
        }

        return 0;
    }

    public bool Copy(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles)
    {
        Requires.NotNull(file);
        Requires.NotNullOrEmpty(targetFolderPath);
        Requires.NotNullOrEmpty(containerName);
        Requires.NotNullOrEmpty(fileName);

        var targetFilePath = this.GetTargetContainerPath(targetFolderPath, containerName);
        var originalFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        var tempFolderPath = Path.Combine(Path.GetDirectoryName(originalFilePath) ?? throw new NotSupportedException(), Path.GetRandomFileName());
        var tempFilePath = Path.Combine(tempFolderPath, fileName);

        try
        {
            _ = Directory.CreateDirectory(tempFolderPath);
            File.Move(originalFilePath, tempFilePath);

            removeSource &= TorrentSevenZipper.Create7z(targetFilePath, tempFilePath);
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

        return true;
    }

    public string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        return Path.Combine(targetFolderPath, containerName + ".7z");
    }
}
