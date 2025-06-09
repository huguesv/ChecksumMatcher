// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.IO.AbstractFileSystem.Offline.Models;
using Woohoo.IO.AbstractFileSystem.Offline.Scanning;

public sealed class OfflineExplorerService : IOfflineExplorerService
{
    private const string FileFilter = $"*{OfflineDisk.DefaultFileExtension}";

    private readonly ILogger<OfflineExplorerService> logger;
    private readonly ILocalSettingsService localSettingsService;

    private readonly List<string> folders = [];
    private readonly Lock foldersLock = new();
    private readonly List<FileSystemWatcher> foldersWatchers = [];

    private readonly ConcurrentDictionary<string, OfflineHeader?> headerCache = new();
    private readonly ConcurrentDictionary<string, OfflineDisk?> diskCache = new();

    public OfflineExplorerService(ILogger<OfflineExplorerService> logger, ILocalSettingsService localSettingsService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(localSettingsService);

        this.logger = logger;
        this.localSettingsService = localSettingsService;
        this.folders = [.. this.localSettingsService.ReadSetting<string[]>(KnownSettingKeys.OfflineFolders) ?? []];

        this.StartWatchingFolders();
    }

    public event EventHandler? RepositoryChanged;

    public event EventHandler<OfflineDiskCreateProgressEventArgs>? DiskCreateProgress;

    public Task<string[]> GetFoldersAsync(CancellationToken ct)
    {
        lock (this.foldersLock)
        {
            return Task.FromResult(this.folders.ToArray());
        }
    }

    public Task AddFolderAsync(string folderPath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderPath);

        this.logger.LogInformation("Adding offline folder: {FolderPath}", folderPath);

        lock (this.foldersLock)
        {
            if (this.folders.Contains(folderPath))
            {
                throw new ArgumentException($"Folder '{folderPath}' already exists.");
            }

            this.folders.Add(folderPath);

            this.localSettingsService.SaveSetting(KnownSettingKeys.OfflineFolders, this.folders.ToArray());
        }

        this.StartWatchingFolders();

        this.RepositoryChanged?.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }

    public Task RemoveFolderAsync(string folderPath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderPath);

        this.logger.LogInformation("Removing offline folder: {FolderPath}", folderPath);

        lock (this.foldersLock)
        {
            if (!this.folders.Remove(folderPath))
            {
                throw new ArgumentException($"Folder '{folderPath}' does not exist.");
            }

            this.localSettingsService.SaveSetting(KnownSettingKeys.OfflineFolders, this.folders.ToArray());
        }

        this.headerCache.Clear();
        this.diskCache.Clear();

        this.StartWatchingFolders();

        this.RepositoryChanged?.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }

    public async Task<OfflineDisk?> FindDiskByNameAsync(string name, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        this.logger.LogInformation("Searching for disk by name: {Name}", name);

        var repository = await this.GetOfflineRepositoryAsync(ct);

        var diskFile = repository.Disks.FirstOrDefault(df => df.Header.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (diskFile is not null)
        {
            return await this.GetDiskAsync(diskFile, ct);
        }

        return null;
    }

    public async Task<OfflineDisk?> GetDiskAsync(OfflineDiskFile file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        // Quick check of the cache on current thread
        if (this.diskCache.TryGetValue(file.FilePath, out var cachedDisk))
        {
            return cachedDisk;
        }

        return await Task.Run(() => this.diskCache.GetOrAdd(file.FilePath, TryDeserializeDisk), ct);
    }

    public async Task<OfflineRepository> GetOfflineRepositoryAsync(CancellationToken ct)
    {
        var folders = await this.GetFoldersAsync(ct);

        this.logger.LogInformation("Getting offline repository.");

        return await Task.Run(() => CreateRepository(folders), ct);

        OfflineRepository CreateRepository(string[] folders)
        {
            var diskFiles = folders
                .Where(Directory.Exists)
                .SelectMany(folderPath => Directory.EnumerateFiles(folderPath, FileFilter, SearchOption.AllDirectories))
                .Select(TryCreateDiskFile)
                .Where(diskFile => diskFile is not null);

            return new OfflineRepository
            {
                IsConfigured = folders.Length > 0,
                Disks = [.. diskFiles.Select(df => df!)],
            };
        }

        OfflineDiskFile? TryCreateDiskFile(string filePath)
        {
            var header = this.headerCache.GetOrAdd(filePath, TryDeserializeHeader);
            return header is null ? null : new OfflineDiskFile { FilePath = filePath, Header = header };
        }
    }

    public async Task CreateDiskAsync(string sourceFolderPath, string targetDiskFilePath, string diskName, OfflineDiskCreateSettings settings, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(targetDiskFilePath);
        ArgumentException.ThrowIfNullOrEmpty(diskName);
        ArgumentNullException.ThrowIfNull(settings);

        if (!Directory.Exists(sourceFolderPath))
        {
            throw new DirectoryNotFoundException($"Source folder '{sourceFolderPath}' does not exist.");
        }

        this.logger.LogInformation("Creating disk from folder: {SourceFolderPath} to file: {TargetDiskFilePath} with name: {DiskName}", sourceFolderPath, targetDiskFilePath, diskName);

        await Task.Run(() => CreateDisk(sourceFolderPath, targetDiskFilePath, diskName, settings, ct), ct);

        void CreateDisk(string sourceFolderPath, string targetDiskFilePath, string diskName, OfflineDiskCreateSettings settings, CancellationToken ct)
        {
            var options = new IndexerOptions
            {
                CalculateChecksums = settings.CalculateChecksums,
                IndexArchiveContent = settings.IndexArchiveContent,
            };

            var indexer = new Indexer(options);

            indexer.ProgressChanged += OnProgressChanged;
            try
            {
                var index = indexer.ScanDisk(sourceFolderPath, diskName, ct);
                index.Serialize(targetDiskFilePath);
            }
            finally
            {
                indexer.ProgressChanged -= OnProgressChanged;
            }

            void OnProgressChanged(object? sender, IndexerProgressEventArgs e)
            {
                this.DiskCreateProgress?.Invoke(this, new OfflineDiskCreateProgressEventArgs(diskName, e.FolderCount, e.FileCount, e.ArchiveItemCount, e.TimeSpan));
            }
        }
    }

    private static OfflineDisk? TryDeserializeDisk(string filePath)
    {
        try
        {
            return OfflineDisk.Deserialize(filePath);
        }
        catch
        {
            return null;
        }
    }

    private static OfflineHeader? TryDeserializeHeader(string filePath)
    {
        try
        {
            return OfflineDisk.DeserializeHeader(filePath);
        }
        catch
        {
            return null;
        }
    }

    private void StopWatchingFolders()
    {
        lock (this.foldersLock)
        {
            foreach (var watcher in this.foldersWatchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            this.foldersWatchers.Clear();
        }
    }

    private void StartWatchingFolders()
    {
        this.StopWatchingFolders();

        List<string> folders = [];
        lock (this.foldersLock)
        {
            folders.AddRange(this.folders);
        }

        var watchers = folders
            .Where(Directory.Exists)
            .Select(this.CreateWatcher)
            .ToList();

        lock (this.foldersLock)
        {
            this.foldersWatchers.AddRange(watchers);
        }
    }

    private FileSystemWatcher CreateWatcher(string folder)
    {
        var watcher = new FileSystemWatcher(folder)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
        };

        watcher.Changed += this.OnFileSystemChanged;
        watcher.Created += this.OnFileSystemChanged;
        watcher.Deleted += this.OnFileSystemChanged;
        watcher.Filters.Add(FileFilter);
        watcher.EnableRaisingEvents = true;

        return watcher;
    }

    private void OnFileSystemChanged(object? sender, FileSystemEventArgs e)
    {
        if ((e.ChangeType & WatcherChangeTypes.Changed) != 0 ||
            (e.ChangeType & WatcherChangeTypes.Deleted) != 0 ||
            (e.ChangeType & WatcherChangeTypes.Renamed) != 0)
        {
            this.headerCache.Clear();
            this.diskCache.Clear();
        }

        // Notify subscribers that they need to refresh
        this.RepositoryChanged?.Invoke(this, EventArgs.Empty);
    }
}
