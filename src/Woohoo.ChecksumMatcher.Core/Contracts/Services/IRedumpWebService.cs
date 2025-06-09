// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System.Collections.Immutable;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;

public interface IRedumpWebService
{
    public ImmutableArray<RedumpSystemInfo> GetSystems();

    Task<bool?> ValidateCredentialsAsync(string username, string password, CancellationToken ct);

    Task<bool> DownloadAllAsync(string outputFolderPath, bool useSubfolders, string? username, string? password, DownloaderProgress progress, CancellationToken ct);

    Task<bool> DownloadAsync(string[] ids, string outputFolderPath, bool useSubfolders, string? username, string? password, DownloaderProgress progress, CancellationToken ct);
}
