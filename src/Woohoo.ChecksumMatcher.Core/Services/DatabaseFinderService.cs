// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization;

public class DatabaseFinderService : IDatabaseFinderService
{
    private readonly ILocalSettingsService settingsService;
    private List<DatabaseFindResult>? cachedFindResults;

    public DatabaseFinderService(ILocalSettingsService settingsService)
    {
        this.settingsService = settingsService;
        this.settingsService.SettingChanged += this.OnSettingChanged;
    }

    public IEnumerable<DatabaseFindResult> FindDatabases()
    {
        if (this.cachedFindResults is not null)
        {
            return this.cachedFindResults;
        }

        return this.EnumerateResults();
    }

    public DatabaseFindResult? LoadDatabase(string rootFolder, string absoluteFilePath)
    {
        RomDatabase? db;
        try
        {
            var loader = new DatabaseLoader(new ClrMameHeaderLoader());
            db = loader.TryLoadFrom(absoluteFilePath);
        }
        catch
        {
            db = null;
        }

        if (db is not null)
        {
            var result = new DatabaseFindResult(db, rootFolder, absoluteFilePath, Path.GetRelativePath(rootFolder, absoluteFilePath));

            var existingResult = this.cachedFindResults?.FirstOrDefault(r => r.AbsoluteFilePath.Equals(result.AbsoluteFilePath, StringComparison.OrdinalIgnoreCase));
            if (existingResult is not null)
            {
                this.cachedFindResults!.Remove(existingResult);
                this.cachedFindResults.Add(result);
            }

            return result;
        }

        return null;
    }

    private IEnumerable<DatabaseFindResult> EnumerateResults()
    {
        this.cachedFindResults = [];

        var databaseFolders = this.settingsService.ReadSetting<string[]>(SettingKeys.DatabaseFolders);
        if (databaseFolders is null || databaseFolders.Length == 0)
        {
            yield break;
        }

        foreach (var folder in databaseFolders)
        {
            var queue = new Queue<string>();
            queue.Enqueue(folder);

            while (queue.Count > 0)
            {
                var childFolder = queue.Dequeue();

                foreach (string filePath in Directory.EnumerateFiles(childFolder, "*.zip").Union(Directory.EnumerateFiles(childFolder, "*.dat")))
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    if (fileName.Contains("Cuesheets", StringComparison.OrdinalIgnoreCase) ||
                        fileName.Contains("GDI Files", StringComparison.OrdinalIgnoreCase) ||
                        fileName.Contains("SBI Subchannels", StringComparison.OrdinalIgnoreCase) ||
                        fileName.Contains("Disc Keys", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var result = new DatabaseFindResult(null, folder, filePath, Path.GetRelativePath(folder, filePath));
                    this.cachedFindResults.Add(result);
                    yield return result;
                }

                foreach (var childChildFolder in Directory.GetDirectories(childFolder))
                {
                    queue.Enqueue(childChildFolder);
                }
            }
        }
    }

    private void OnSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingKey == SettingKeys.DatabaseFolders)
        {
            // TODO
        }
    }

    internal class DatabaseLoader
    {
        private readonly DatabaseImporterProvider importerProvider;

        public DatabaseLoader(IDatabaseHeaderLoader headerLoader)
        {
            this.importerProvider = new DatabaseImporterProvider(headerLoader);
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

    internal class ClrMameHeaderLoader : IDatabaseHeaderLoader
    {
        public string Load(string workingFolderPath, string headerFileName)
        {
            var headerFilePath = workingFolderPath.Length > 0 ? Path.Combine(workingFolderPath, headerFileName) : headerFileName;
            return !File.Exists(headerFilePath)
                ? throw new DatabaseImportException(string.Format(CultureInfo.CurrentCulture, "Header file not found: {0}", headerFilePath))
                : File.ReadAllText(headerFilePath);
        }
    }
}
