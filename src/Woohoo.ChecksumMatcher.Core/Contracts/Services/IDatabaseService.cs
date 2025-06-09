// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System;
using System.Threading.Tasks;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;

public interface IDatabaseService
{
    event EventHandler? RepositoryChanged;

    event EventHandler<ScanEventArgs>? ScanProgress;

    event EventHandler<RebuildEventArgs>? RebuildProgress;

    event EventHandler<DatabaseCreateEventArgs>? DatabaseCreateProgress;

    Task<DatabaseRepository> GetRepositoryAsync(CancellationToken ct);

    Task<string[]> GetRepositoryFoldersAsync(CancellationToken ct);

    Task AddRepositoryFolderAsync(string folderPath, CancellationToken ct);

    Task RemoveRepositoryFolderAsync(string folderPath, CancellationToken ct);

    Task<string[]> GetCueFoldersAsync(CancellationToken ct);

    Task AddCueFolderAsync(string folderPath, CancellationToken ct);

    Task RemoveCueFolderAsync(string folderPath, CancellationToken ct);

    Task<RomDatabase?> GetDatabaseAsync(DatabaseFile file, CancellationToken ct);

    Task SetFileScanSettingsAsync(string id, DatabaseFileScanSettings settings, CancellationToken ct);

    Task SetFolderScanSettingsAsync(string id, DatabaseFolderScanSettings settings, CancellationToken ct);

    Task<DatabaseFileScanSettings> GetFileScanSettingsAsync(string id, CancellationToken ct);

    Task<DatabaseFolderScanSettings> GetFolderScanSettingsAsync(string id, CancellationToken ct);

    Task ScanAsync(DatabaseFile file, CancellationToken ct);

    Task ScanAsync(DatabaseFolder folder, CancellationToken ct);

    Task<DatabaseScanResults?> GetScanResultsAsync(DatabaseFile file, CancellationToken ct);

    Task ClearScanResultsAsync(DatabaseFile file, CancellationToken ct);

    Task ClearScanResultsAsync(DatabaseFolder folder, CancellationToken ct);

    Task SetRebuildSettingsAsync(string id, RebuildSettings settings, CancellationToken ct);

    Task<RebuildSettings> GetRebuildSettingsAsync(string id, CancellationToken ct);

    Task RebuildAsync(DatabaseFile file, CancellationToken ct);

    Task<DatabaseRebuildResults?> GetRebuildResultsAsync(DatabaseFile file, CancellationToken ct);

    Task CreateDatabaseAsync(string sourceFolderPath, string targetDatabaseFilePath, DatabaseCreateSettings settings, CancellationToken ct);

    IEnumerable<string> GetRebuildTargetContainerTypes();
}
