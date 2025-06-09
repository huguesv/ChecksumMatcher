// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Folder;

using System;
using System.Collections.Generic;
using System.IO;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;

internal sealed class FolderContainer : IContainer
{
    string IContainer.FileExtension => throw new NotSupportedException();

    async Task<FileInformation[]> IContainer.GetAllFilesAsync(string containerFilePath, SearchOption searchOption, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(containerFilePath);

        ct.ThrowIfCancellationRequested();

        if (!Directory.Exists(containerFilePath))
        {
            return [];
        }

        var files = new List<FileInformation>();

        var folderInfo = new DirectoryInfo(containerFilePath);
        foreach (var fileInfo in folderInfo.GetFiles("*.*", searchOption))
        {
            ct.ThrowIfCancellationRequested();

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
                var children = await compressedContainer.GetAllFilesAsync(fileInfo.FullName, searchOption, ct);
                files.AddRange(children);
            }
        }

        return files.ToArray();
    }

    async Task IContainer.CalculateChecksumsAsync(FileInformation file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        var filePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        if (File.Exists(filePath))
        {
            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using (stream)
            {
                await ChecksumService.CalculateAllAsync(file, stream, stream.Length, ct);
            }
        }
    }

    Task<bool> IContainer.ExistsAsync(FileInformation file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        var sourceFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        return Task.FromResult(File.Exists(sourceFilePath));
    }

    Task IContainer.CopyAsync(FileInformation file, string targetFilePath, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        var sourceFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        File.Copy(sourceFilePath, targetFilePath);

        return Task.CompletedTask;
    }

    Task IContainer.MoveAsync(FileInformation file, string targetFilePath, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        var sourceFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        File.Move(sourceFilePath, targetFilePath);

        return Task.CompletedTask;
    }

    Task IContainer.RemoveAsync(FileInformation file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        var sourceFilePath = Path.Combine(file.ContainerAbsolutePath, file.FileRelativePath);
        FileUtility.SafeDelete(sourceFilePath);

        return Task.CompletedTask;
    }

    public Task RemoveContainerAsync(string containerFilePath, CancellationToken ct)
    {
        FileUtility.SafeDeleteFolder(containerFilePath);

        return Task.CompletedTask;
    }
}
