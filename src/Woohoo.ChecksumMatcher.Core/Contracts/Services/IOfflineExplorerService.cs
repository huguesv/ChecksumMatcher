// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System;
using System.Threading.Tasks;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public interface IOfflineExplorerService
{
    event EventHandler? RepositoryChanged;

    event EventHandler<OfflineDiskCreateProgressEventArgs>? DiskCreateProgress;

    Task<OfflineRepository> GetOfflineRepositoryAsync(CancellationToken ct);

    Task<OfflineDisk?> GetDiskAsync(OfflineDiskFile file, CancellationToken ct);

    Task<OfflineDisk?> FindDiskByNameAsync(string name, CancellationToken ct);

    Task<string[]> GetFoldersAsync(CancellationToken ct);

    Task AddFolderAsync(string folderPath, CancellationToken ct);

    Task RemoveFolderAsync(string folderPath, CancellationToken ct);

    Task CreateDiskAsync(string sourceFolderPath, string targetDiskFilePath, string diskName, OfflineDiskCreateSettings settings, CancellationToken ct);
}
