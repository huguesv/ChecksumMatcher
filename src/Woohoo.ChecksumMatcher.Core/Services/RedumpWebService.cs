// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Internal.Redump;

public class RedumpWebService : IRedumpWebService
{
    public Task<bool?> ValidateCredentialsAsync(string username, string password, CancellationToken ct)
    {
        return Task.Run(() => RedumpClient.ValidateCredentialsAsync(username, password, ct), ct);
    }

    public Task<bool> DownloadAllAsync(string outputFolderPath, bool useSubfolders, string? username, string? password, DownloaderProgress progress, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(outputFolderPath);
        ArgumentNullException.ThrowIfNull(progress);

        var redumpClient = new RedumpClient(TimeSpan.FromSeconds(60));

        return Task.Run(() => this.DownloadAllAsync(redumpClient, outputFolderPath, useSubfolders, username, password, progress, ct), ct);
    }

    private async Task<bool> DownloadAllAsync(RedumpClient redumpClient, string outputFolderPath, bool useSubfolders, string? username, string? password, DownloaderProgress progress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        await redumpClient.LoginAsync(username ?? string.Empty, password ?? string.Empty, ct);

        int progressMax = RedumpSystems.All.Where(s => !s.IsPrivate || redumpClient.LoggedIn).Sum(s => s.ArtifactCount);
        int currentProgress = 0;

        foreach (var system in RedumpSystems.All)
        {
            ct.ThrowIfCancellationRequested();

            if (!redumpClient.LoggedIn && system.IsPrivate)
            {
                continue;
            }

            var systemProgress = (int count, string artifact) => progress(count, progressMax, system.Name, artifact);
            currentProgress = await this.DownloadSystemAsync(redumpClient, system, outputFolderPath, useSubfolders, systemProgress, currentProgress, ct);
        }

        // Final progress update to indicate completion
        progress(currentProgress, progressMax, string.Empty, string.Empty);

        return true;
    }

    private async Task<int> DownloadSystemAsync(RedumpClient redumpClient, RedumpSystem system, string outDir, bool useSubfolders, Action<int, string> systemProgress, int currentProgress, CancellationToken ct)
    {
        if (system.HasCues)
        {
            currentProgress = await this.DownloadAsync(redumpClient, system, outDir, useSubfolders ? "cue" : null, RedumpUrls.CueFormat, systemProgress, currentProgress, "cue", ct);
        }

        if (system.HasDat)
        {
            currentProgress = await this.DownloadAsync(redumpClient, system, outDir, useSubfolders ? "dat" : null, RedumpUrls.DatFormat, systemProgress, currentProgress, "dat", ct);
        }

        if (system.HasDkeys)
        {
            currentProgress = await this.DownloadAsync(redumpClient, system, outDir, useSubfolders ? "dkey" : null, RedumpUrls.DkeysFormat, systemProgress, currentProgress, "dkey", ct);
        }

        if (system.HasGdi)
        {
            currentProgress = await this.DownloadAsync(redumpClient, system, outDir, useSubfolders ? "gdi" : null, RedumpUrls.GdiFormat, systemProgress, currentProgress, "gdi", ct);
        }

        if (system.HasKeys)
        {
            currentProgress = await this.DownloadAsync(redumpClient, system, outDir, useSubfolders ? "keys" : null, RedumpUrls.KeysFormat, systemProgress, currentProgress, "keys", ct);
        }

        if (system.HasLsd)
        {
            currentProgress = await this.DownloadAsync(redumpClient, system, outDir, useSubfolders ? "lsd" : null, RedumpUrls.LsdFormat, systemProgress, currentProgress, "lsd", ct);
        }

        if (system.HasSbi)
        {
            currentProgress = await this.DownloadAsync(redumpClient, system, outDir, useSubfolders ? "sbi" : null, RedumpUrls.SbiFormat, systemProgress, currentProgress, "sbi", ct);
        }

        return currentProgress;
    }

    private async Task<int> DownloadAsync(RedumpClient redumpClient, RedumpSystem system, string outDir, string? subfolder, string urlFormat, Action<int, string> systemProgress, int currentProgress, string artifact, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        systemProgress(currentProgress, artifact);

        await redumpClient.DownloadToFileAsync(string.Format(urlFormat, system.Id), outDir, subfolder, ct);

        return currentProgress + 1;
    }
}
