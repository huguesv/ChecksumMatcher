// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.UnitTest.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;

internal class TestDatabaseService : IDatabaseService
{
#pragma warning disable CS0067
    public event EventHandler? RepositoryChanged;

    public event EventHandler<ScanEventArgs>? ScanProgress;

    public event EventHandler<RebuildEventArgs>? RebuildProgress;

    public event EventHandler<DatabaseCreateEventArgs>? DatabaseCreateProgress;
#pragma warning restore CS0067

    public Task AddCueFolderAsync(string folderPath, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task AddRepositoryFolderAsync(string folderPath, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task ClearScanResultsAsync(DatabaseFile file, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task ClearScanResultsAsync(DatabaseFolder folder, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task CreateDatabaseAsync(string sourceFolderPath, string targetDatabaseFilePath, DatabaseCreateSettings settings, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<string[]> GetCueFoldersAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<RomDatabase?> GetDatabaseAsync(DatabaseFile file, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<DatabaseFileScanSettings> GetFileScanSettingsAsync(string id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<DatabaseFolderScanSettings> GetFolderScanSettingsAsync(string id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<DatabaseRebuildResults?> GetRebuildResultsAsync(DatabaseFile file, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<RebuildSettings> GetRebuildSettingsAsync(string id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetRebuildTargetContainerTypes()
    {
        throw new NotImplementedException();
    }

    public Task<DatabaseRepository> GetRepositoryAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<string[]> GetRepositoryFoldersAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<DatabaseScanResults?> GetScanResultsAsync(DatabaseFile file, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task RebuildAsync(DatabaseFile file, RebuildSettings rebuildSettings, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task RebuildAsync(DatabaseFile file, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task RemoveCueFolderAsync(string folderPath, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task RemoveRepositoryFolderAsync(string folderPath, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task ScanAsync(DatabaseFile file, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task ScanAsync(DatabaseFolder folder, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task SetFileScanSettingsAsync(string id, DatabaseFileScanSettings settings, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task SetFolderScanSettingsAsync(string id, DatabaseFolderScanSettings settings, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task SetRebuildSettingsAsync(string id, RebuildSettings settings, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
