// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Services;

using System;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;
using Woohoo.ChecksumMatcher.WinUI.ViewModels;

internal sealed partial class OperationListenerService : IOperationListenerService, IDisposable
{
    private readonly DisposableBag disposables = DisposableBag.Create<OperationListenerService>();

    private readonly IDatabaseService databaseService;
    private readonly IHistoryService historyService;
    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly ILogger<OperationListenerService> logger;

    public OperationListenerService(
        IDatabaseService databaseService,
        IHistoryService historyService,
        IOfflineExplorerService offlineExplorerService,
        ILogger<OperationListenerService> logger)
    {
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(historyService);
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(logger);

        this.databaseService = databaseService;
        this.historyService = historyService;
        this.offlineExplorerService = offlineExplorerService;
        this.logger = logger;

        this.databaseService.RebuildProgress += this.DatabaseService_RebuildProgress;
        this.databaseService.ScanProgress += this.DatabaseService_ScanProgress;
        this.databaseService.DatabaseCreateProgress += this.DatabaseService_DatabaseCreateProgress;
        this.offlineExplorerService.DiskCreateProgress += this.OfflineExplorerService_DiskCreateProgress;

        this.disposables.Add(() =>
        {
            this.databaseService.RebuildProgress -= this.DatabaseService_RebuildProgress;
            this.databaseService.ScanProgress -= this.DatabaseService_ScanProgress;
            this.databaseService.DatabaseCreateProgress -= this.DatabaseService_DatabaseCreateProgress;
            this.offlineExplorerService.DiskCreateProgress -= this.OfflineExplorerService_DiskCreateProgress;
        });
    }

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    private void DatabaseService_RebuildProgress(object? sender, RebuildEventArgs e)
    {
        try
        {
            switch (e.Status)
            {
                case RebuildStatus.Pending:
                    this.historyService.AddHistoryItem(new HistoryItem
                    {
                        Id = e.OperationId,
                        Title = Localized.HistoryPageTitleRebuild,
                        Subtitle = e.DatabaseFile.RelativePath,
                        StartTime = DateTimeOffset.Now,
                        Details = Localized.HistoryPageDetailsPending,
                        Status = HistoryItemStatus.Pending,
                        NavigationPage = typeof(DatabaseLibraryViewModel).FullName!,
                        NavigationParameter = DatabaseLibraryViewModel.EncodeNavigationParameter(DatabaseLibraryViewType.Rebuild, e.DatabaseFile),
                        PreventQuit = true,
                    });
                    break;
                case RebuildStatus.Started:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsStarted, DateTimeOffset.Now);
                    break;
                case RebuildStatus.Scanning:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsScanning, DateTimeOffset.Now);
                    break;
                case RebuildStatus.Hashing:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsHashing, DateTimeOffset.Now);
                    break;
                case RebuildStatus.Building:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsBuilding, DateTimeOffset.Now);
                    break;
                case RebuildStatus.Completed:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.Completed, Localized.HistoryPageDetailsCompleted, DateTimeOffset.Now);
                    break;
                case RebuildStatus.Canceled:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.Canceled, Localized.HistoryPageDetailsCanceled, DateTimeOffset.Now);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in RebuildProgress event handler.");
        }
    }

    private void DatabaseService_ScanProgress(object? sender, ScanEventArgs e)
    {
        try
        {
            switch (e.Status)
            {
                case ScanStatus.Pending:
                    this.historyService.AddHistoryItem(new HistoryItem
                    {
                        Id = e.OperationId,
                        Title = Localized.HistoryPageTitleScan,
                        Subtitle = e.DatabaseFile.RelativePath,
                        StartTime = DateTimeOffset.Now,
                        Details = Localized.HistoryPageDetailsPending,
                        Status = HistoryItemStatus.Pending,
                        NavigationPage = typeof(DatabaseLibraryViewModel).FullName!,
                        NavigationParameter = DatabaseLibraryViewModel.EncodeNavigationParameter(DatabaseLibraryViewType.Scan, e.DatabaseFile),
                        PreventQuit = false,
                    });
                    break;
                case ScanStatus.Started:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsStarted, DateTimeOffset.Now);
                    break;
                case ScanStatus.Scanning:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsScanning, DateTimeOffset.Now);
                    break;
                case ScanStatus.Hashing:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsHashing, DateTimeOffset.Now);
                    break;
                case ScanStatus.Completed:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.Completed, Localized.HistoryPageDetailsCompleted, DateTimeOffset.Now);
                    break;
                case ScanStatus.Canceled:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.Canceled, Localized.HistoryPageDetailsCanceled, DateTimeOffset.Now);
                    break;
                case ScanStatus.Cleared:
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in ScanProgress event handler.");
        }
    }

    private void DatabaseService_DatabaseCreateProgress(object? sender, DatabaseCreateEventArgs e)
    {
        try
        {
            switch (e.Status)
            {
                case DatabaseCreateStatus.Pending:
                    this.historyService.AddHistoryItem(new HistoryItem
                    {
                        Id = e.OperationId,
                        Title = Localized.HistoryPageTitleCreateDatabase,
                        Subtitle = e.DatabaseName,
                        StartTime = DateTimeOffset.Now,
                        Details = Localized.HistoryPageDetailsPending,
                        Status = HistoryItemStatus.Pending,
                        NavigationPage = typeof(DatabaseCreateViewModel).FullName!,
                        NavigationParameter = null,
                        PreventQuit = true,
                    });
                    break;
                case DatabaseCreateStatus.Started:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsStarted, DateTimeOffset.Now);
                    break;
                case DatabaseCreateStatus.Scanning:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsScanning, DateTimeOffset.Now);
                    break;
                case DatabaseCreateStatus.Hashing:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsHashing, DateTimeOffset.Now);
                    break;
                case DatabaseCreateStatus.Completed:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.Completed, Localized.HistoryPageDetailsCompleted, DateTimeOffset.Now);
                    break;
                case DatabaseCreateStatus.Canceled:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.Canceled, Localized.HistoryPageDetailsCanceled, DateTimeOffset.Now);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in DatabaseCreateProgress event handler.");
        }
    }

    private void OfflineExplorerService_DiskCreateProgress(object? sender, OfflineDiskCreateProgressEventArgs e)
    {
        try
        {
            switch (e.Status)
            {
                case OfflineDiskCreateStatus.Pending:
                    this.historyService.AddHistoryItem(new HistoryItem
                    {
                        Id = e.OperationId,
                        Title = Localized.HistoryPageTitleCreateOfflineDisk,
                        Subtitle = e.DiskName,
                        StartTime = DateTimeOffset.Now,
                        Details = Localized.HistoryPageDetailsPending,
                        Status = HistoryItemStatus.Pending,
                        NavigationPage = typeof(OfflineExplorerCreateDiskViewModel).FullName!,
                        NavigationParameter = null,
                        PreventQuit = true,
                    });
                    break;
                case OfflineDiskCreateStatus.Scanning:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.InProgress, Localized.HistoryPageDetailsScanning, DateTimeOffset.Now);
                    break;
                case OfflineDiskCreateStatus.Completed:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.Completed, Localized.HistoryPageDetailsCompleted, DateTimeOffset.Now);
                    break;
                case OfflineDiskCreateStatus.Canceled:
                    this.historyService.UpdateStatus(e.OperationId, HistoryItemStatus.Canceled, Localized.HistoryPageDetailsCanceled, DateTimeOffset.Now);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in DiskCreateProgress event handler.");
        }
    }
}
