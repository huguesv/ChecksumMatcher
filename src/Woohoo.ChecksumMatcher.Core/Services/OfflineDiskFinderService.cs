// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using System.Collections.Generic;
using System.IO;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public class OfflineDiskFinderService : IOfflineDiskFinderService
{
    private readonly ILocalSettingsService settingsService;
    private List<OfflineDiskFindResult>? cachedFindResults;
    private object lockObject = new();

    public OfflineDiskFinderService(ILocalSettingsService settingsService)
    {
        this.settingsService = settingsService;
        this.settingsService.SettingChanged += this.OnSettingChanged;
    }

    public IEnumerable<OfflineDiskFindResult> FindOfflineDisks()
    {
        lock (this.lockObject)
        {
            if (this.cachedFindResults is not null)
            {
                return this.cachedFindResults.ToArray();
            }
        }

        return this.EnumerateResults();
    }

    public OfflineDiskFindResult? TryLoadByName(string diskName)
    {
        var offlineDisks = this.FindOfflineDisks();

        // If the disk is already deserialized, then look it up by name on the disk object.
        var found = offlineDisks.SingleOrDefault(res => res.Disk?.Name == diskName);
        if (found is null)
        {
            // If the disk is not deserialized, then look it up by file name.
            found = offlineDisks.SingleOrDefault(res => string.Equals(Path.GetFileNameWithoutExtension(res.FilePath), diskName, StringComparison.OrdinalIgnoreCase));
        }

        if (found is not null)
        {
            if (found.Disk is null)
            {
                found = this.Load(found.FilePath);
            }
        }

        return found;
    }

    public OfflineDiskFindResult Load(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        var offlineDisk = OfflineDisk.Deserialize(filePath);
        var result = new OfflineDiskFindResult(filePath, offlineDisk);

        lock (this.lockObject)
        {
            var existingResult = this.cachedFindResults?.FirstOrDefault(r => r.FilePath.Equals(result.FilePath, StringComparison.OrdinalIgnoreCase));
            if (existingResult is not null)
            {
                this.cachedFindResults!.Remove(existingResult);
                this.cachedFindResults.Add(result);
            }
        }

        return result;
    }

    private IEnumerable<OfflineDiskFindResult> EnumerateResults()
    {
        lock (this.lockObject)
        {
            this.cachedFindResults = [];

            var offlineDiskFolders = this.settingsService.ReadSetting<string[]>(SettingKeys.OfflineFolders);
            if (offlineDiskFolders is null || offlineDiskFolders.Length == 0)
            {
                return this.cachedFindResults;
            }

            foreach (var folder in offlineDiskFolders)
            {
                var queue = new Queue<string>();
                queue.Enqueue(folder);

                while (queue.Count > 0)
                {
                    var childFolder = queue.Dequeue();

                    foreach (string filePath in Directory.EnumerateFiles(childFolder, "*.zip"))
                    {
                        var result = new OfflineDiskFindResult(filePath, null);
                        this.cachedFindResults.Add(result);
                    }

                    foreach (var childChildFolder in Directory.GetDirectories(childFolder))
                    {
                        queue.Enqueue(childChildFolder);
                    }
                }
            }

            return this.cachedFindResults.ToArray();
        }
    }

    private void OnSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingKey == SettingKeys.DatabaseFolders)
        {
            // TODO
        }
    }
}
