// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning;

public sealed class DatabaseService : IDatabaseService
{
    private static readonly string[] DatFileNameOmitList =
    [
        "Cuesheets",
        "GDI Files",
        "SBI Subchannels",
        "Disc Keys"
    ];

    private readonly ILogger<DatabaseService> logger;
    private readonly ILocalSettingsService localSettingsService;
    private readonly IOfflineExplorerService offlineExplorerService;

    private readonly Lock repositoryFoldersLock = new();
    private readonly List<string> repositoryFolders = [];
    private readonly List<FileSystemWatcher> repositoryFoldersWatchers = [];

    private readonly Lock cueFoldersLock = new();
    private readonly List<string> cueFolders = [];

    private readonly ConcurrentDictionary<string, RomDatabase?> databaseCache = new();
    private readonly ConcurrentDictionary<string, DatabaseScanResults> scanResultsCache = new();
    private readonly ConcurrentDictionary<string, DatabaseRebuildResults> rebuildResultsCache = new();

    public DatabaseService(ILogger<DatabaseService> logger, ILocalSettingsService localSettingsService, IOfflineExplorerService offlineExplorerService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(localSettingsService);
        ArgumentNullException.ThrowIfNull(offlineExplorerService);

        this.logger = logger;
        this.localSettingsService = localSettingsService;
        this.offlineExplorerService = offlineExplorerService;
        this.repositoryFolders = [.. this.localSettingsService.ReadSetting<string[]>(KnownSettingKeys.DatabaseFolders) ?? []];
        this.cueFolders = [.. this.localSettingsService.ReadSetting<string[]>(KnownSettingKeys.CueFolders) ?? []];

        this.StartWatchingFolders();
    }

    public event EventHandler? RepositoryChanged;

    public event EventHandler<ScanEventArgs>? ScanProgress;

    public event EventHandler<RebuildEventArgs>? RebuildProgress;

    public event EventHandler<DatabaseCreateEventArgs>? DatabaseCreateProgress;

    public Task<string[]> GetRepositoryFoldersAsync(CancellationToken ct)
    {
        lock (this.repositoryFoldersLock)
        {
            return Task.FromResult(this.repositoryFolders.ToArray());
        }
    }

    public Task AddRepositoryFolderAsync(string folderPath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderPath);

        this.logger.LogInformation("Adding repository folder: {FolderPath}", folderPath);

        lock (this.repositoryFoldersLock)
        {
            if (this.repositoryFolders.Contains(folderPath))
            {
                throw new ArgumentException($"The folder '{folderPath}' is already added.");
            }

            this.repositoryFolders.Add(folderPath);

            this.localSettingsService.SaveSetting(KnownSettingKeys.DatabaseFolders, this.repositoryFolders.ToArray());
        }

        this.StartWatchingFolders();

        this.RepositoryChanged?.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }

    public Task RemoveRepositoryFolderAsync(string folderPath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderPath);

        this.logger.LogInformation("Removing repository folder: {FolderPath}", folderPath);

        lock (this.repositoryFoldersLock)
        {
            if (!this.repositoryFolders.Remove(folderPath))
            {
                throw new ArgumentException($"The folder '{folderPath}' is not found.");
            }

            this.localSettingsService.SaveSetting(KnownSettingKeys.DatabaseFolders, this.repositoryFolders.ToArray());
        }

        this.StartWatchingFolders();

        this.RepositoryChanged?.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }

    public Task<string[]> GetCueFoldersAsync(CancellationToken ct)
    {
        lock (this.cueFoldersLock)
        {
            return Task.FromResult(this.cueFolders.ToArray());
        }
    }

    public Task AddCueFolderAsync(string folderPath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderPath);

        this.logger.LogInformation("Adding cue folder: {FolderPath}", folderPath);

        lock (this.cueFoldersLock)
        {
            if (this.cueFolders.Contains(folderPath))
            {
                throw new ArgumentException($"The folder '{folderPath}' is already added.");
            }

            this.cueFolders.Add(folderPath);

            this.localSettingsService.SaveSetting(KnownSettingKeys.CueFolders, this.cueFolders.ToArray());
        }

        return Task.CompletedTask;
    }

    public Task RemoveCueFolderAsync(string folderPath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderPath);

        this.logger.LogInformation("Removing cue folder: {FolderPath}", folderPath);

        lock (this.cueFoldersLock)
        {
            if (!this.cueFolders.Remove(folderPath))
            {
                throw new ArgumentException($"The folder '{folderPath}' is not found.");
            }

            this.localSettingsService.SaveSetting(KnownSettingKeys.CueFolders, this.cueFolders.ToArray());
        }

        return Task.CompletedTask;
    }

    public async Task<DatabaseRepository> GetRepositoryAsync(CancellationToken ct)
    {
        List<string> folders = [];
        lock (this.repositoryFoldersLock)
        {
            folders.AddRange(this.repositoryFolders);
        }

        return await Task.Run(() => CreateRepository(folders), ct);

        static DatabaseRepository CreateRepository(List<string> folders)
        {
            return new DatabaseRepository()
            {
                IsConfigured = folders.Count > 0,
                RootFolders = [.. folders.Where(Directory.Exists).Select(f => GetFolder(f, string.Empty))],
            };
        }

        static DatabaseFolder GetFolder(string rootAbsolutePath, string relativePath)
        {
            var folderAbsolutePath = Path.Combine(rootAbsolutePath, relativePath);

            var files = Directory
                .EnumerateFiles(folderAbsolutePath, "*.zip")
                .Union(Directory.EnumerateFiles(folderAbsolutePath, "*.dat"))
                .Select(childFilePath => TryCreateDatabaseFile(rootAbsolutePath, relativePath, childFilePath))
                .Where(df => df is not null);

            var folders = Directory
                .GetDirectories(folderAbsolutePath)
                .Select(childFolderPath => GetFolder(rootAbsolutePath, Path.Combine(relativePath, Path.GetFileName(childFolderPath))));

            return new DatabaseFolder
            {
                RootAbsoluteFolderPath = rootAbsolutePath,
                RelativePath = relativePath,
                SubFolders = [.. folders],
                Files = [.. files.Select(f => f!)],
            };
        }

        static DatabaseFile? TryCreateDatabaseFile(string rootAbsolutePath, string relativePath, string childFilePath)
        {
            if (DatFileNameOmitList.Any(omit => Path.GetFileNameWithoutExtension(childFilePath).Contains(omit, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            return new DatabaseFile
            {
                RootAbsoluteFolderPath = rootAbsolutePath,
                RelativePath = Path.Combine(relativePath, Path.GetFileName(childFilePath)),
            };
        }
    }

    public async Task<RomDatabase?> GetDatabaseAsync(DatabaseFile file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        // Quick check of the cache on current thread
        if (this.databaseCache.TryGetValue(file.FullPath, out var cachedDatabase))
        {
            return cachedDatabase;
        }

        return await Task.Run(() => this.databaseCache.GetOrAdd(file.FullPath, _ => LoadDatabase(file)), ct);

        static RomDatabase? LoadDatabase(DatabaseFile file)
        {
            return new DatabaseLoader().TryLoadFrom(Path.Combine(file.FullPath));
        }
    }

    public Task<DatabaseFileScanSettings> GetFileScanSettingsAsync(string id, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        this.logger.LogInformation("Loading scan settings for database: {Id}", id);

        return Task.FromResult(this.localSettingsService.LoadItemScopeSetting(
            id,
            KnownSettingKeys.DatabaseFileScanSettings,
            new DatabaseFileScanSettings()));
    }

    public Task<DatabaseFolderScanSettings> GetFolderScanSettingsAsync(string id, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        this.logger.LogInformation("Loading scan settings for folder: {Id}", id);

        return Task.FromResult(this.localSettingsService.LoadItemScopeSetting(
            id,
            KnownSettingKeys.DatabaseFolderScanSettings,
            new DatabaseFolderScanSettings()));
    }

    public Task SetFileScanSettingsAsync(string id, DatabaseFileScanSettings settings, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(settings);

        this.logger.LogInformation("Saving scan settings for database: {Id}", id);

        this.localSettingsService.SaveItemScopeSetting(
            id,
            KnownSettingKeys.DatabaseFileScanSettings,
            settings);

        return Task.CompletedTask;
    }

    public Task SetFolderScanSettingsAsync(string id, DatabaseFolderScanSettings settings, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(settings);

        this.logger.LogInformation("Saving scan settings for folder: {Id}", id);

        this.localSettingsService.SaveItemScopeSetting(
            id,
            KnownSettingKeys.DatabaseFolderScanSettings,
            settings);

        return Task.CompletedTask;
    }

    public async Task ScanAsync(DatabaseFile file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.logger.LogInformation("Scanning database: {FullPath}", file.FullPath);

        if (this.databaseCache.TryGetValue(file.FullPath, out var cachedDatabase))
        {
            this.ScanProgress?.Invoke(this, new ScanEventArgs { DatabaseFile = file, Database = cachedDatabase, ProgressPercentage = 0, Status = ScanStatus.Pending, Results = new DatabaseScanResults() });
        }
        else
        {
            this.ScanProgress?.Invoke(this, new ScanEventArgs { DatabaseFile = file, ProgressPercentage = 0, Status = ScanStatus.Pending, Results = new DatabaseScanResults() });
        }

        await Task.Run(DoScanAsync, ct);

        async Task DoScanAsync()
        {
            var db = await this.GetDatabaseAsync(file, ct)
                ?? throw new DatabaseImportException($"The database file '{file.FullPath}' could not be loaded.");

            this.ScanProgress?.Invoke(this, new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Started, Results = new DatabaseScanResults() });

            ct.ThrowIfCancellationRequested();

            var scanSettings = await this.GetEffectiveScanSettingsAsync(file, db, ct);

            ct.ThrowIfCancellationRequested();

            this.ScanProgress?.Invoke(this, new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Scanning, Results = new DatabaseScanResults() });

            this.scanResultsCache.TryRemove(db.Name, out _);

            this.scanResultsCache[db.Name] = await Scanner.ScanAsync((ea) => this.ScanProgress?.Invoke(this, ea), file, db, scanSettings, this.offlineExplorerService.FindDiskByNameAsync, ct);
        }
    }

    public async Task ScanAsync(DatabaseFolder folder, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(folder);

        this.logger.LogInformation("Scanning folder: {FullPath}", folder.FullPath);

        foreach (var subFolder in folder.SubFolders)
        {
            ct.ThrowIfCancellationRequested();

            // Scan each subfolder recursively
            await this.ScanAsync(subFolder, ct);
        }

        foreach (var file in folder.Files)
        {
            ct.ThrowIfCancellationRequested();

            // Scan each file individually
            await this.ScanAsync(file, ct);
        }
    }

    public async Task<DatabaseScanResults?> GetScanResultsAsync(DatabaseFile file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.logger.LogInformation("Retrieving scan results for database: {FullPath}", file.FullPath);

        var db = await this.GetDatabaseAsync(file, ct)
            ?? throw new DatabaseImportException($"The database file '{file.FullPath}' could not be loaded.");

        return this.scanResultsCache.TryGetValue(db.Name, out var results) ? results : null;
    }

    public async Task ClearScanResultsAsync(DatabaseFile file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.logger.LogInformation("Clearing scan results for database: {FullPath}", file.FullPath);

        var db = await this.GetDatabaseAsync(file, ct)
            ?? throw new DatabaseImportException($"The database file '{file.FullPath}' could not be loaded.");

        this.scanResultsCache.Remove(db.Name, out _);

        this.ScanProgress?.Invoke(this, new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Cleared, Results = new DatabaseScanResults() });
    }

    public Task ClearScanResultsAsync(DatabaseFolder folder, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(folder);

        this.logger.LogInformation("Clearing scan results for folder: {FullPath}", folder.FullPath);

        return Task.Run(async () => await ClearFolderScanResultsAsync(folder, this, ct), ct);

        static async Task ClearFolderScanResultsAsync(DatabaseFolder folder, IDatabaseService databaseService, CancellationToken ct)
        {
            foreach (var subFolder in folder.SubFolders)
            {
                ct.ThrowIfCancellationRequested();
                await ClearFolderScanResultsAsync(subFolder, databaseService, ct);
            }

            foreach (var file in folder.Files)
            {
                ct.ThrowIfCancellationRequested();
                await databaseService.ClearScanResultsAsync(file, ct);
            }
        }
    }

    public Task SetRebuildSettingsAsync(string id, RebuildSettings settings, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(settings);

        this.logger.LogInformation("Saving rebuild settings for database: {Id}", id);

        this.localSettingsService.SaveItemScopeSetting(
            id,
            KnownSettingKeys.RebuildSettings,
            settings);

        return Task.CompletedTask;
    }

    public Task<RebuildSettings> GetRebuildSettingsAsync(string id, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        this.logger.LogInformation("Loading rebuild settings for database: {Id}", id);

        return Task.FromResult(this.localSettingsService.LoadItemScopeSetting(
            id,
            KnownSettingKeys.RebuildSettings,
            new RebuildSettings()));
    }

    public async Task RebuildAsync(DatabaseFile file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.logger.LogInformation("Rebuilding database: {FullPath}", file.FullPath);

        this.RebuildProgress?.Invoke(this, new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 0, Status = RebuildStatus.Pending, Results = new DatabaseRebuildResults() });

        await Task.Run(DoRebuildAsync, ct);

        async Task DoRebuildAsync()
        {
            var db = await this.GetDatabaseAsync(file, ct)
                ?? throw new DatabaseImportException($"The database file '{file.FullPath}' could not be loaded.");

            var cueFolders = await this.GetCueFoldersAsync(ct);
            var rebuildSettings = await this.GetRebuildSettingsAsync(db.Name, ct);

            ct.ThrowIfCancellationRequested();

            this.rebuildResultsCache.TryRemove(db.Name, out _);

            this.rebuildResultsCache[db.Name] = await Scanner.RebuildAsync(
                (ea) => this.RebuildProgress?.Invoke(this, ea),
                file,
                db,
                rebuildSettings,
                cueFolders,
                ct);
        }
    }

    public async Task<DatabaseRebuildResults?> GetRebuildResultsAsync(DatabaseFile file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.logger.LogInformation("Retrieving rebuild results for database: {FullPath}", file.FullPath);

        var db = await this.GetDatabaseAsync(file, ct)
            ?? throw new DatabaseImportException($"The database file '{file.FullPath}' could not be loaded.");

        return this.rebuildResultsCache.TryGetValue(db.Name, out var results) ? results : null;
    }

    public async Task CreateDatabaseAsync(string sourceFolderPath, string targetDatabaseFilePath, DatabaseCreateSettings settings, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(targetDatabaseFilePath);
        ArgumentNullException.ThrowIfNull(settings);

        this.logger.LogInformation("Creating database from source folder: {SourceFolder} to target file: {DatabaseFilePath}", sourceFolderPath, targetDatabaseFilePath);

        var targetFolderPath = Path.GetDirectoryName(targetDatabaseFilePath);
        if (string.IsNullOrEmpty(targetFolderPath))
        {
            throw new ArgumentException($"The target database file path '{targetDatabaseFilePath}' is invalid.");
        }

        if (!Directory.Exists(sourceFolderPath))
        {
            throw new DirectoryNotFoundException($"The source folder '{sourceFolderPath}' does not exist.");
        }

        this.DatabaseCreateProgress?.Invoke(this, new DatabaseCreateEventArgs { ProgressPercentage = 0, Status = DatabaseCreateStatus.Pending, Results = new DatabaseCreateResults() });

        await Task.Run(DoCreateDatabaseAsync, ct);

        async Task DoCreateDatabaseAsync()
        {
            await Scanner.CreateDatabaseAsync(
                (ea) => this.DatabaseCreateProgress?.Invoke(this, ea),
                sourceFolderPath,
                targetFolderPath,
                targetDatabaseFilePath,
                settings,
                ct);
        }
    }

    public IEnumerable<string> GetRebuildTargetContainerTypes()
    {
        yield return KnownContainerTypes.Folder;
        yield return KnownContainerTypes.Zip;
        yield return KnownContainerTypes.SevenZip;

        if (RuntimeInformation.OSArchitecture == Architecture.X64 || RuntimeInformation.OSArchitecture == Architecture.X86)
        {
            yield return KnownContainerTypes.TorrentSevenZip;
            yield return KnownContainerTypes.TorrentZip;
        }
    }

    private async Task<EffectiveScanSettings> GetEffectiveScanSettingsAsync(DatabaseFile databaseFile, RomDatabase db, CancellationToken ct)
    {
        var fileScanSettings = await this.GetFileScanSettingsAsync(db.Name, ct);

        // Start with the settings for the database
        var onlineFolders = new List<EffectiveOnlineFolderSetting>();
        if (fileScanSettings.UseOnlineFolders)
        {
            onlineFolders.AddRange(fileScanSettings
                .ScanOnlineFolders
                .Where(sof => sof.IsIncluded)
                .Select(sof => new EffectiveOnlineFolderSetting { FolderPath = sof.FolderPath }));
        }

        var offlineFolders = new List<EffectiveOfflineFolderSetting>();
        if (fileScanSettings.UseOfflineFolders)
        {
            offlineFolders.AddRange(fileScanSettings
                .ScanOfflineFolders
                .Where(sof => sof.IsIncluded)
                .Select(sof => new EffectiveOfflineFolderSetting { DiskName = sof.DiskName, FolderPath = sof.FolderPath }));
        }

        // Then go up the folder hierarchy to find additional settings
        var repo = await this.GetRepositoryAsync(ct);
        var parentFolder = this.GetParent(repo, databaseFile);
        while (parentFolder is not null)
        {
            var folderScanSettings = await this.GetFolderScanSettingsAsync(parentFolder.FullPath, ct);
            if (folderScanSettings.UseOnlineFolders)
            {
                foreach (var onlineFolder in folderScanSettings.ScanOnlineFolders.Where(sof => sof.IsIncluded))
                {
                    var onlineFolderPath = Path.Combine(onlineFolder.FolderPath, db.Name.TrimEnd('.'));
                    onlineFolders.Add(new EffectiveOnlineFolderSetting
                    {
                        FolderPath = onlineFolderPath,
                    });
                }
            }

            if (folderScanSettings.UseOfflineFolders)
            {
                foreach (var offlineFolder in folderScanSettings.ScanOfflineFolders.Where(sof => sof.IsIncluded))
                {
                    var offlineFolderPath = Path.Combine(offlineFolder.FolderPath, db.Name.TrimEnd('.'));
                    offlineFolders.Add(new EffectiveOfflineFolderSetting
                    {
                        DiskName = offlineFolder.DiskName,
                        FolderPath = offlineFolderPath,
                    });
                }
            }

            parentFolder = this.GetParent(repo, parentFolder);
        }

        return new EffectiveScanSettings
        {
            ForceCalculateChecksums = fileScanSettings.ForceCalculateChecksums,
            ScanOnlineFolders = [.. onlineFolders.Distinct()],
            ScanOfflineFolders = [.. offlineFolders.Distinct()],
        };
    }

    private DatabaseFolder? GetParent(DatabaseRepository repo, DatabaseFolder folderToFind)
    {
        return InternalGetParent(repo.RootFolders, folderToFind);

        static DatabaseFolder? InternalGetParent(IEnumerable<DatabaseFolder> folders, DatabaseFolder folderToFind)
        {
            foreach (var folder in folders)
            {
                if (folder.SubFolders.Any(f => f.FullPath == folderToFind.FullPath))
                {
                    return folder;
                }

                var subFolder = InternalGetParent(folder.SubFolders, folderToFind);
                if (subFolder is not null)
                {
                    return subFolder;
                }
            }

            return null;
        }
    }

    private DatabaseFolder? GetParent(DatabaseRepository repo, DatabaseFile fileToFind)
    {
        return InternalGetParent(repo.RootFolders, fileToFind);

        static DatabaseFolder? InternalGetParent(IEnumerable<DatabaseFolder> folders, DatabaseFile fileToFind)
        {
            foreach (var folder in folders)
            {
                if (folder.Files.Any(f => f.FullPath == fileToFind.FullPath))
                {
                    return folder;
                }

                var subFolder = InternalGetParent(folder.SubFolders, fileToFind);
                if (subFolder is not null)
                {
                    return subFolder;
                }
            }

            return null;
        }
    }

    private void StopWatchingFolders()
    {
        lock (this.repositoryFoldersLock)
        {
            foreach (var watcher in this.repositoryFoldersWatchers)
            {
                this.logger.LogInformation("Stopped file system watcher for repository folder: {FolderPath}", watcher.Path);

                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            this.repositoryFoldersWatchers.Clear();
        }
    }

    private void StartWatchingFolders()
    {
        this.StopWatchingFolders();

        List<string> folders = [];
        lock (this.repositoryFoldersLock)
        {
            folders.AddRange(this.repositoryFolders);
        }

        var watchers = folders
            .Where(Directory.Exists)
            .Select(this.CreateWatcher)
            .ToList();

        lock (this.repositoryFoldersLock)
        {
            this.repositoryFoldersWatchers.AddRange(watchers);
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
        watcher.Filters.Add("*.dat");
        watcher.Filters.Add("*.zip");
        watcher.EnableRaisingEvents = true;

        this.logger.LogInformation("Started file system watcher for repository folder: {FolderPath}", folder);

        return watcher;
    }

    private void OnFileSystemChanged(object? sender, FileSystemEventArgs e)
    {
        if (DatFileNameOmitList.Any(omit => e.Name?.Contains(omit, StringComparison.OrdinalIgnoreCase) == true))
        {
            return;
        }

        if ((e.ChangeType & WatcherChangeTypes.Changed) != 0 ||
            (e.ChangeType & WatcherChangeTypes.Deleted) != 0 ||
            (e.ChangeType & WatcherChangeTypes.Renamed) != 0)
        {
            this.databaseCache.Clear();
            this.scanResultsCache.Clear();
            this.rebuildResultsCache.Clear();
        }

        // Notify subscribers that they need to refresh
        this.RepositoryChanged?.Invoke(this, EventArgs.Empty);
    }

    internal class DatabaseLoader
    {
        private readonly DatabaseImporterProvider importerProvider;

        public DatabaseLoader()
        {
            this.importerProvider = new DatabaseImporterProvider();
        }

        public RomDatabase? TryLoadFrom(string filePath)
        {
            try
            {
                if (string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    using var zipArchive = new ZipArchive(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                    if (zipArchive.Entries.Count != 1)
                    {
                        return null;
                    }

                    var entry = zipArchive.Entries.FirstOrDefault();
                    if (entry != null)
                    {
                        if (entry.Name.EndsWith(".dat", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using var stream = entry.Open();
                            var reader = new StreamReader(stream);
                            var text = reader.ReadToEnd();
                            return this.importerProvider.Load(text, Path.GetDirectoryName(filePath) ?? string.Empty);
                        }
                    }
                }
                else if (string.Equals(Path.GetExtension(filePath), ".dat", StringComparison.InvariantCultureIgnoreCase))
                {
                    return this.importerProvider.Load(File.ReadAllText(filePath), Path.GetDirectoryName(filePath) ?? string.Empty);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
