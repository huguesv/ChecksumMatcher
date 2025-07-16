// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.SevenZip;

using System;
using System.Collections.Generic;
using System.IO;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.IO.AbstractFileSystem;
using Woohoo.IO.Compression.SevenZip;

internal sealed class SevenZipContainer : IContainer
{
    string IContainer.FileExtension => ".7z";

    public static bool IsComplete(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        var targetZipFile = new SevenZipFile(targetArchiveFilePath);
        var isComplete = targetZipFile.Entries.Count == expectedTargetFiles.Length;

        return isComplete;
    }

    FileInformation[] IContainer.GetAllFiles(string containerFilePath, SearchOption searchOption, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(containerFilePath);

        ct.ThrowIfCancellationRequested();

        var files = new List<FileInformation>();

        try
        {
            var archive = new SevenZipFile(containerFilePath);
            foreach (var entry in archive.Entries)
            {
                var file = new FileInformation
                {
                    ContainerIsFolder = false,
                    ContainerAbsolutePath = containerFilePath,
                    FileRelativePath = entry.Name,
                    Size = (long)entry.Size,
                    DataBlockSize = (long)entry.Size,
                    ReportedCRC32 = ChecksumConversion.ToByteArray((uint)entry.CRC32),
                    LastWriteTime = entry.LastWriteTime,
                    CreationTime = entry.CreationTime,
                    IsDirectory = entry.IsDirectory,
                    CompressionMethod = entry.Method,
                };

                files.Add(file);
            }
        }
        catch (Exception)
        {
        }

        return files.ToArray();
    }

    void IContainer.CalculateChecksums(FileInformation file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (File.Exists(file.ContainerAbsolutePath))
        {
            try
            {
                var archive = new SevenZipFile(file.ContainerAbsolutePath);

                var entry = archive.GetEntry(file.FileRelativePath);
                if (entry is not null)
                {
                    var tempFilePath = Path.GetTempFileName();
                    try
                    {
                        archive.Extract(entry, tempFilePath);
                        using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                        {
                            ChecksumService.CalculateAll(file, stream, stream.Length, ct);
                        }
                    }
                    finally
                    {
                        File.Delete(tempFilePath);
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
        ArgumentNullException.ThrowIfNull(file);

        if (File.Exists(file.ContainerAbsolutePath))
        {
            var archive = new SevenZipFile(file.ContainerAbsolutePath);
            var entry = archive.GetEntry(file.FileRelativePath);
            return entry is not null;
        }

        return false;
    }

    void IContainer.Copy(FileInformation file, string targetFilePath)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (string.IsNullOrEmpty(targetFilePath))
        {
            throw new ArgumentNullException(nameof(targetFilePath));
        }

        var archive = new SevenZipFile(file.ContainerAbsolutePath);
        var entry = archive.GetEntry(file.FileRelativePath);
        if (entry is not null)
        {
            archive.Extract(entry, targetFilePath);
        }
    }

    void IContainer.Move(FileInformation file, string targetFilePath)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        (this as IContainer).Copy(file, targetFilePath);
        (this as IContainer).Remove(file);
    }

    void IContainer.Remove(FileInformation file)
    {
        // Not supported
    }

    public void RemoveContainer(string containerFilePath)
    {
        FileUtility.SafeDelete(containerFilePath);
    }
}
