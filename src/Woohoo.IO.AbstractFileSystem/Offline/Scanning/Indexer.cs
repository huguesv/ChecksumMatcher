// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline.Scanning;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Woohoo.IO.AbstractFileSystem;
using Woohoo.IO.AbstractFileSystem.Offline.Models;
using Woohoo.IO.AbstractFileSystem.Online;
using Woohoo.Security.Cryptography;

public sealed class Indexer
{
    private readonly IndexerOptions options;
    private readonly Stopwatch stopwatch = new();
    private readonly OnlineArchive onlineArchive = new();

    private CancellationToken cancellationToken;
    private long folderCount;
    private long fileCount;
    private long archiveItemCount;

    public Indexer(IndexerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.options = options;
    }

    public event EventHandler<IndexerProgressEventArgs>? ProgressChanged;

    public OfflineDisk ScanDisk(string rootFolderPath, string name, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(name);

        this.stopwatch.Restart();
        this.cancellationToken = cancellationToken;

        this.folderCount = 0;
        this.fileCount = 0;
        this.archiveItemCount = 0;

        var driveInfo = new DriveInfo(rootFolderPath);

        var serialNumber = string.Empty;
        if (OperatingSystem.IsWindows())
        {
            serialNumber = DriveInfoEx.GetVolumeSerial(driveInfo.RootDirectory.FullName);
        }

        var disk = new OfflineDisk
        {
            Name = name,
            Label = driveInfo.VolumeLabel,
            SerialNumber = serialNumber,
            TotalSize = driveInfo.TotalSize,
        };

        var folderInfo = new DirectoryInfo(rootFolderPath);
        var folder = new OfflineItem
        {
            Kind = OfflineItemKind.Folder,
            Name = folderInfo.Name,
            Path = folderInfo.FullName,
            Created = folderInfo.CreationTime,
            Modified = folderInfo.LastWriteTime,
            ParentItem = null,
            ParentDisk = disk,
        };

        disk.RootFolders.Add(folder);

        this.folderCount++;

        this.ScanFolderItems(folder);

        return disk;
    }

    public void CalculateChecksums(OfflineDisk disk)
    {
        ArgumentNullException.ThrowIfNull(disk);

        foreach (var item in disk.RootFolders)
        {
            this.CalculateChecksums(item);
        }
    }

    public void CalculateChecksums(OfflineItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Kind == OfflineItemKind.File || item.Kind == OfflineItemKind.ArchiveFile)
        {
            var hashCalculator = new HashCalculator();
            var hashResult = hashCalculator.Calculate(["crc32", "md5", "sha1", "sha256", "sha512"], item.Path);

            item.MD5 = SafeHashResult(hashResult, "md5");
            item.SHA1 = SafeHashResult(hashResult, "sha1");
            item.SHA256 = SafeHashResult(hashResult, "sha256");
            item.SHA512 = SafeHashResult(hashResult, "sha512");
            item.CRC32 = SafeHashResult(hashResult, "crc32");

            static string? SafeHashResult(HashCalculatorResult result, string hashName)
            {
                if (result.Checksums.TryGetValue(hashName, out var hash))
                {
                    return HashCalculator.HexToString(hash);
                }

                return null;
            }
        }
        else if (item.Kind == OfflineItemKind.Folder)
        {
            foreach (var subItem in item.Items)
            {
                this.CalculateChecksums(subItem);
            }
        }
        else
        {
            throw new NotSupportedException($"Unsupported item kind: {item.Kind}");
        }
    }

    private void ScanFolderItems(OfflineItem folder)
    {
        if (folder.ParentDisk is null)
        {
            throw new InvalidOperationException("Parent disk is null");
        }

        Queue<OfflineItem> foldersToScan = new();
        foldersToScan.Enqueue(folder);

        while (foldersToScan.Count > 0)
        {
            var currentFolder = foldersToScan.Dequeue();
            try
            {
                this.cancellationToken.ThrowIfCancellationRequested();

                var folderInfo = new DirectoryInfo(currentFolder.Path);
                if (!folderInfo.Exists)
                {
                    continue;
                }

                var childFiles = folderInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                    .Select(info => this.CreateFile(info, currentFolder.ParentItem, currentFolder.ParentDisk!))
                    .ToList();

                currentFolder.Items.AddRange(childFiles);

                this.fileCount += childFiles.Count;

                var childFolders = folderInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly)
                    .Select(info => this.CreateFolder(info, currentFolder.ParentItem, currentFolder.ParentDisk!))
                    .ToList();

                currentFolder.Items.AddRange(childFolders);

                this.folderCount += childFolders.Count;

                childFolders.ForEach(cf => foldersToScan.Enqueue(cf));
            }
            catch (UnauthorizedAccessException)
            {
            }

            this.ProgressChanged?.Invoke(this, new IndexerProgressEventArgs(this.folderCount, this.fileCount, this.archiveItemCount, this.stopwatch.Elapsed));
        }
    }

    private OfflineItem CreateFolder(DirectoryInfo folderInfo, OfflineItem? parentItem, OfflineDisk parentDisk)
    {
        this.cancellationToken.ThrowIfCancellationRequested();

        return new OfflineItem
        {
            Kind = OfflineItemKind.Folder,
            Name = folderInfo.Name,
            Path = folderInfo.FullName,
            Created = folderInfo.CreationTime,
            Modified = folderInfo.LastWriteTime,
            ParentItem = parentItem,
            ParentDisk = parentDisk,
        };
    }

    private OfflineItem CreateFile(FileInfo fileInfo, OfflineItem? parentItem, OfflineDisk parentDisk)
    {
        this.cancellationToken.ThrowIfCancellationRequested();

        if (this.onlineArchive.IsSupportedArchiveFile(fileInfo.FullName))
        {
            var archiveItem = new OfflineItem
            {
                Kind = OfflineItemKind.ArchiveFile,
                Name = fileInfo.Name,
                Path = fileInfo.FullName,
                Created = fileInfo.CreationTime,
                Modified = fileInfo.LastWriteTime,
                Size = fileInfo.Length,
                ParentItem = parentItem,
                ParentDisk = parentDisk,
            };

            if (this.options.IndexArchiveContent)
            {
                this.onlineArchive
                    .EnumerateEntries(fileInfo.FullName)
                    .Where(entry => !entry.IsDirectory)
                    .Select(entry => this.CreateArchiveChild(entry, archiveItem, parentDisk))
                    .ToList()
                    .ForEach(archiveItem.Items.Add);

                this.archiveItemCount += archiveItem.Items.Count;

                this.ProgressChanged?.Invoke(this, new IndexerProgressEventArgs(this.folderCount, this.fileCount, this.archiveItemCount, this.stopwatch.Elapsed));
            }

            return archiveItem;
        }
        else
        {
            return new OfflineItem
            {
                Kind = OfflineItemKind.File,
                Name = fileInfo.Name,
                Path = fileInfo.FullName,
                Created = fileInfo.CreationTime,
                Modified = fileInfo.LastWriteTime,
                Size = fileInfo.Length,
                ParentItem = parentItem,
                ParentDisk = parentDisk,
            };
        }
    }

    private OfflineItem CreateArchiveChild(IArchiveEntry entry, OfflineItem? parentItem, OfflineDisk parentDisk)
    {
        this.cancellationToken.ThrowIfCancellationRequested();

        return new OfflineItem
        {
            Kind = OfflineItemKind.File,
            Name = entry.Name,
            Path = entry.Name,
            Created = entry.LastModifiedUtc,
            Modified = entry.LastModifiedUtc,
            Size = entry.Size,
            ReportedCRC32 = entry.ReportedCRC32 is not null ? HashCalculator.HexToString(entry.ReportedCRC32) : null,
            ParentItem = parentItem,
            ParentDisk = parentDisk,
        };
    }
}
