// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;

public sealed partial class DatabaseStatsViewModel : ObservableObject
{
    private readonly IDatabaseService databaseService;
    private readonly IClipboardService clipboardService;
    private readonly Action? onCalculatingComplete;

    public DatabaseStatsViewModel(IDatabaseService databaseService, IClipboardService clipboardService, RomDatabase db)
    {
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(db);

        this.databaseService = databaseService;
        this.clipboardService = clipboardService;

        this.SummaryDatabaseCount = 1;
        this.SummaryContainerCount = db.Games.Count;
        this.SummaryFileCount = db.Games.Sum(g => g.Roms.Count);
        this.SummarySize = db.Games.Sum(g => g.Roms.Sum(long (RomFile r) => r.Size));
    }

    public DatabaseStatsViewModel(IDatabaseService databaseService, IClipboardService clipboardService, DatabaseFolder folder, Action onCalculatingComplete)
    {
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(folder);
        ArgumentNullException.ThrowIfNull(onCalculatingComplete);

        this.databaseService = databaseService;
        this.clipboardService = clipboardService;
        this.onCalculatingComplete = onCalculatingComplete;

        this.IsCalculating = true;
        this.ComputeFolderStatsAsync(folder, CancellationToken.None).FireAndForget();
    }

    [ObservableProperty]
    public partial bool IsCalculating { get; set; }

    [ObservableProperty]
    public partial long SummaryDatabaseCount { get; set; }

    [ObservableProperty]
    public partial long SummaryContainerCount { get; set; }

    [ObservableProperty]
    public partial long SummaryFileCount { get; set; }

    [ObservableProperty]
    public partial long SummarySize { get; set; }

    public void CopyToClipboard()
    {
        var current = new StringBuilder();

        AppendProperty(Localized.StatisticDatabaseCount, this.SummaryDatabaseCount);
        AppendProperty(Localized.StatisticContainerCount, this.SummaryContainerCount);
        AppendProperty(Localized.StatisticFileCount, this.SummaryFileCount);
        AppendProperty(Localized.StatisticFileSizeTotal, this.SummarySize);

        this.clipboardService.SetText(current.ToString());

        void AppendProperty(string header, long value)
        {
            current.AppendLine(header);
            current.AppendLine(value.ToString());
            current.AppendLine();
        }
    }

    private async Task ComputeFolderStatsAsync(DatabaseFolder folder, CancellationToken ct)
    {
        var result = await ComputeAsync(this.databaseService, folder, ct);

        this.SummaryDatabaseCount = result.Item1;
        this.SummaryContainerCount = result.Item2;
        this.SummaryFileCount = result.Item3;
        this.SummarySize = result.Item4;
        this.IsCalculating = false;
        this.onCalculatingComplete?.Invoke();

        static async Task<Tuple<long, long, long, long>> ComputeAsync(IDatabaseService databaseService, DatabaseFolder folder, CancellationToken ct)
        {
            long dbCount = 0;
            long containerCount = 0;
            long fileCount = 0;
            long size = 0;
            foreach (var game in folder.Files)
            {
                if (ct.IsCancellationRequested)
                {
                    return new Tuple<long, long, long, long>(dbCount, containerCount, fileCount, size);
                }

                var database = await databaseService.GetDatabaseAsync(game, ct);
                if (database is not null)
                {
                    dbCount++;
                    containerCount += database.Games.Count;
                    fileCount += database.Games.Sum(g => g.Roms.Count);
                    size += database.Games.Sum(g => g.Roms.Sum(r => r.Size));
                }
            }

            // Recursively compute stats for subfolders
            foreach (var subFolder in folder.SubFolders)
            {
                if (ct.IsCancellationRequested)
                {
                    return new Tuple<long, long, long, long>(dbCount, containerCount, fileCount, size);
                }

                var subStats = await ComputeAsync(databaseService, subFolder, ct);
                dbCount += subStats.Item1;
                containerCount += subStats.Item2;
                fileCount += subStats.Item3;
                size += subStats.Item4;
            }

            return new Tuple<long, long, long, long>(dbCount, containerCount, fileCount, size);
        }
    }
}
