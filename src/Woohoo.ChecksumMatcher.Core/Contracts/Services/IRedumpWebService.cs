// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using Woohoo.ChecksumMatcher.Core.Contracts.Models;

public interface IRedumpWebService
{
    Task<bool?> ValidateCredentialsAsync(string username, string password, CancellationToken ct);

    Task<bool> DownloadAllAsync(string outDir, bool useSubfolders, string? username, string? password, DownloaderProgress progress, CancellationToken ct);
}
