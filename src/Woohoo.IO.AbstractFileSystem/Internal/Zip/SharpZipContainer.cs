// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.Zip;

using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Woohoo.IO.AbstractFileSystem;
using Woohoo.IO.AbstractFileSystem.Internal;
using Woohoo.IO.Compression.Zip;

internal sealed class SharpZipContainer : IContainer
{
    string IContainer.FileExtension => ".zip";

    public static bool IsComplete(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        Requires.NotNullOrEmpty(targetArchiveFilePath);
        Requires.NotNull(expectedTargetFiles);

        var isComplete = false;

        using (var targetZipFile = new ZipFile(targetArchiveFilePath))
        {
            isComplete = targetZipFile.Count == expectedTargetFiles.Length;
        }

        return isComplete;
    }

    FileInformation[] IContainer.GetAllFiles(string containerFilePath, SearchOption searchOption)
    {
        Requires.NotNullOrEmpty(containerFilePath);

        var files = new List<FileInformation>();

        try
        {
            using (var archive = new ZipFile(containerFilePath))
            {
                foreach (ZipEntry entry in archive)
                {
                    var file = new FileInformation
                    {
                        ContainerIsFolder = false,
                        ContainerAbsolutePath = containerFilePath,
                        FileRelativePath = entry.Name.Replace('/', '\\'),
                        Size = entry.Size,
                        DataBlockSize = entry.Size,
                        CreationTime = null,
                        LastWriteTime = entry.DateTime,
                        IsDirectory = entry.IsDirectory,
                        CompressionMethod = entry.CompressionMethod.ToString(),
                    };

                    if (entry.HasCrc)
                    {
                        file.ReportedCRC32 = ByteArrayUtility.ByteArrayFromUInt32((uint)entry.Crc);
                    }

                    files.Add(file);
                }
            }
        }
        catch (Exception)
        {
        }

        return files.ToArray();
    }

    void IContainer.CalculateChecksums(FileInformation file)
    {
        Requires.NotNull(file);

        if (File.Exists(file.ContainerAbsolutePath))
        {
            try
            {
                using (var zipArchive = new ZipFile(file.ContainerAbsolutePath))
                {
                    var entryIndex = zipArchive.FindEntry(file.FileRelativePath, true);
                    if (entryIndex != -1)
                    {
                        var entry = zipArchive[entryIndex];
                        if (entry != null)
                        {
                            using (var stream = zipArchive.GetInputStream(entry))
                            {
                                ChecksumService.CalculateAll(file, stream, entry.Size);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }

    bool IContainer.Exists(FileInformation file)
    {
        Requires.NotNull(file);

        if (File.Exists(file.ContainerAbsolutePath))
        {
            using (var sourceZipFile = new ZipFile(file.ContainerAbsolutePath))
            {
                var entry = sourceZipFile.GetEntry(file.FileRelativePath);
                return entry != null;
            }
        }

        return false;
    }

    void IContainer.Copy(FileInformation file, string targetFilePath)
    {
        Requires.NotNull(file);
        Requires.NotNullOrEmpty(targetFilePath);

        using (var sourceZipFile = new ZipFile(file.ContainerAbsolutePath))
        {
            var entry = sourceZipFile.GetEntry(file.FileRelativePath);
            sourceZipFile.Extract(entry, targetFilePath);
        }
    }

    void IContainer.Move(FileInformation file, string targetFilePath)
    {
        Requires.NotNull(file);
        Requires.NotNullOrEmpty(targetFilePath);

        (this as IContainer).Copy(file, targetFilePath);
        (this as IContainer).Remove(file);
    }

    void IContainer.Remove(FileInformation file)
    {
        Requires.NotNull(file);

        var shouldDeleteArchive = false;

        using (var sourceZipFile = new ZipFile(file.ContainerAbsolutePath))
        {
            var entry = sourceZipFile.GetEntry(file.FileRelativePath);
            if (entry != null)
            {
                if (sourceZipFile.Count == 1)
                {
                    // It's faster to just delete the archive
                    shouldDeleteArchive = true;
                }
                else
                {
                    sourceZipFile.BeginUpdate();
                    sourceZipFile.Delete(entry);
                    sourceZipFile.CommitUpdate();
                }
            }
        }

        if (shouldDeleteArchive)
        {
            FileUtility.SafeDelete(file.ContainerAbsolutePath);
        }
    }
}
