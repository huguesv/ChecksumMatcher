// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal;

using System;
using System.Collections.Generic;
using System.IO;
using Woohoo.IO.AbstractFileSystem;

internal sealed class FolderContainer : IContainer
{
    string IContainer.FileExtension => throw new NotSupportedException();

    FileInformation[] IContainer.GetAllFiles(string containerFilePath, SearchOption searchOption)
    {
        Requires.NotNullOrEmpty(containerFilePath);

        var files = new List<FileInformation>();

        var folderInfo = new DirectoryInfo(containerFilePath);
        foreach (var fileInfo in folderInfo.GetFiles("*.*", searchOption))
        {
            var compressedContainer = ContainerExtensionProvider.GetContainer(fileInfo.FullName);
            if (compressedContainer == null)
            {
                var file = new FileInformation
                {
                    ContainerIsFolder = true,
                    ContainerAbsolutePath = Path.GetDirectoryName(fileInfo.FullName) ?? string.Empty,
                    FileRelativePath = Path.GetFileName(fileInfo.FullName),
                    Size = fileInfo.Length,
                };
                file.DataBlockSize = file.Size;

                files.Add(file);
            }
            else
            {
                files.AddRange(compressedContainer.GetAllFiles(fileInfo.FullName, searchOption));
            }
        }

        return files.ToArray();
    }

    void IContainer.CalculateChecksums(FileInformation file)
    {
        Requires.NotNull(file);

        var filePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        if (File.Exists(filePath))
        {
            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using (stream)
            {
                ChecksumService.CalculateAll(file, stream, stream.Length);
            }
        }
    }

    bool IContainer.Exists(FileInformation file)
    {
        Requires.NotNull(file);

        var sourceFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        return File.Exists(sourceFilePath);
    }

    void IContainer.Copy(FileInformation file, string targetFilePath)
    {
        Requires.NotNull(file);
        Requires.NotNullOrEmpty(targetFilePath);

        var sourceFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        File.Copy(sourceFilePath, targetFilePath);
    }

    void IContainer.Move(FileInformation file, string targetFilePath)
    {
        Requires.NotNull(file);
        Requires.NotNullOrEmpty(targetFilePath);

        var sourceFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        File.Move(sourceFilePath, targetFilePath);
    }

    void IContainer.Remove(FileInformation file)
    {
        Requires.NotNull(file);

        var sourceFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        FileUtility.SafeDelete(sourceFilePath);
    }
}
