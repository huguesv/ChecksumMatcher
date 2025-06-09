// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Collections;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public partial class DatabaseFileViewModel : ObservableRecipient
{
    private readonly DatabaseFile databaseFile;
    private readonly RomDatabase db;
    private readonly IDatabaseService databaseService;
    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly IClipboardService clipboardService;
    private readonly IFilePickerService filePickerService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly IDispatcherQueue dispatcherQueue;

    private DatabaseGameFilterKind filterKind = DatabaseGameFilterKind.All;
    private long? filterTextNumeric = null;

    private CancellationTokenSource? scanCancellationTokenSource;
    private CancellationTokenSource? rebuildCancellationTokenSource;

    private bool isLoadingScanSettings = false;

    private DatabaseFileViewModel(
        DatabaseFile databaseFile,
        RomDatabase db,
        IDatabaseService databaseService,
        IOfflineExplorerService offlineExplorerService,
        IClipboardService clipboardService,
        IFilePickerService filePickerService,
        IFileExplorerService fileExplorerService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        IDispatcherQueue dispatcherQueue)
    {
        ArgumentNullException.ThrowIfNull(databaseFile);
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);
        ArgumentNullException.ThrowIfNull(dispatcherQueue);

        this.databaseFile = databaseFile;
        this.db = db;
        this.databaseService = databaseService;
        this.offlineExplorerService = offlineExplorerService;
        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.fileExplorerService = fileExplorerService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.dispatcherQueue = dispatcherQueue;

        this.databaseService.RebuildProgress += this.DatabaseService_RebuildProgress;
        this.databaseService.ScanProgress += this.DatabaseService_ScanProgress;

        foreach (var game in db.Games.OrderBy(g => g.Name))
        {
            var gameViewModel = new DatabaseGameViewModel(game, this.clipboardService);
            this.Games.Add(gameViewModel);
        }

        this.Metadata = new DatabaseMetadataViewModel(this.clipboardService, db);
        this.Stats = new DatabaseStatsViewModel(this.databaseService, this.clipboardService, db);

        this.FilteredGames = new AdvancedCollectionView(this.Games, true)
        {
            Filter = (obj) =>
            {
                if (obj is DatabaseGameViewModel game)
                {
                    return (this.IsFilterNameMatch(game) || this.IsFilterSizeMatch(game) || this.IsFilterChecksumMatch(game)) && this.IsFilterStatusMatch(game) && this.IsRomFilterMatch(game);
                }

                return false;
            },
        };
        this.FilteredGames.ObserveFilterProperty(nameof(this.FilterText));
        this.FilteredGames.SortDescriptions.Add(new SortDescription(nameof(DatabaseGameViewModel.Name), SortDirection.Ascending));

        this.BrowseOfflineFolder = new DatabaseBrowseOfflineFolderViewModel(this.offlineExplorerService, this.ApplyBrowseOfflineFolderSelectionAsync);

        this.UpdateFilterDescription();
        this.UpdateRomFilterDescription();

        this.ScanMatchedFiles.CollectionChanged += (s, e) =>
        {
            this.ScanMatchedFilesLabel = this.ScanMatchedFiles.Count > 0
                ? string.Format(CultureInfo.CurrentUICulture, Localized.ScanResultMatchedWithCountLabel, this.ScanMatchedFiles.Count)
                : Localized.ScanResultMatchedNoCountLabel;
        };

        this.ScanMissingFiles.CollectionChanged += (s, e) =>
        {
            this.ScanMissingLabel = this.ScanMissingFiles.Count > 0
                ? string.Format(CultureInfo.CurrentUICulture, Localized.ScanResultMissingWithCountLabel, this.ScanMissingFiles.Count)
                : Localized.ScanResultMissingNoCountLabel;
        };

        this.ScanUnusedFiles.CollectionChanged += (s, e) =>
        {
            this.ScanUnusedFilesLabel = this.ScanUnusedFiles.Count > 0
                ? string.Format(CultureInfo.CurrentUICulture, Localized.ScanResultUnusedWithCountLabel, this.ScanUnusedFiles.Count)
                : Localized.ScanResultUnusedNoCountLabel;
        };

        this.ScanIncorrectNameFiles.CollectionChanged += (s, e) =>
        {
            this.ScanIncorrectNameFilesLabel = this.ScanIncorrectNameFiles.Count > 0
                ? string.Format(CultureInfo.CurrentUICulture, Localized.ScanResultWrongNameWithCountLabel, this.ScanIncorrectNameFiles.Count)
                : Localized.ScanResultWrongNameNoCountLabel;
        };

        this.ScanSortedMatchedFiles = new AdvancedCollectionView(this.ScanMatchedFiles, true)
        {
            SortDescriptions = { new SortDescription(nameof(DatabaseScanResultActualViewModel.ActualDisplayName), SortDirection.Ascending) },
        };

        this.ScanSortedUnusedFiles = new AdvancedCollectionView(this.ScanUnusedFiles, true)
        {
            SortDescriptions = { new SortDescription(nameof(DatabaseScanResultActualViewModel.ActualDisplayName), SortDirection.Ascending) },
        };

        this.ScanSortedMissingFiles = new AdvancedCollectionView(this.ScanMissingFiles, true)
        {
            SortDescriptions = { new SortDescription(nameof(DatabaseScanResultExpectedViewModel.ExpectedDisplayName), SortDirection.Ascending) },
        };

        this.ScanSortedIncorrectNameFiles = new AdvancedCollectionView(this.ScanIncorrectNameFiles, true)
        {
            SortDescriptions = { new SortDescription(nameof(DatabaseScanResultWrongNameViewModel.ExpectedDisplayName), SortDirection.Ascending) },
        };

        this.RebuildSortedMatchedFiles = new AdvancedCollectionView(this.RebuildMatchedFiles, true)
        {
            SortDescriptions = { new SortDescription(SortDirection.Ascending) },
        };

        this.RebuildSortedUnusedFiles = new AdvancedCollectionView(this.RebuildUnusedFiles, true)
        {
            SortDescriptions = { new SortDescription(SortDirection.Ascending) },
        };

        var containerTypes = this.databaseService
            .GetRebuildTargetContainerTypes()
            .Select(type => new DatabaseTargetContainerTypeViewModel { Type = type, DisplayName = ContainerTypeToDisplayName(type) });

        this.RebuildTargetContainerTypes = new ObservableCollection<DatabaseTargetContainerTypeViewModel>(containerTypes);
        this.RebuildTargetContainerType = this.RebuildTargetContainerTypes.FirstOrDefault();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    public partial ObservableCollection<DatabaseScanOnlineFolderItemViewModel> ScanOnlineFolders { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    public partial ObservableCollection<DatabaseScanOfflineFolderItemViewModel> ScanOfflineFolders { get; set; } = [];

    [ObservableProperty]
    public partial DatabaseBrowseOfflineFolderViewModel BrowseOfflineFolder { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearScanResultsCommand))]
    public partial StatusViewModel ScanStatus { get; set; } = new StatusViewModel(string.Empty, StatusSeverity.None);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearScanResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(RebuildCommand))]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelScanCommand))]
    public partial bool IsScanning { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RebuildCommand))]
    public partial string RebuildSourceFolderPath { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RebuildCommand))]
    public partial string RebuildTargetFolderPath { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearRebuildResultsCommand))]
    public partial StatusViewModel RebuildStatus { get; set; } = new StatusViewModel(string.Empty, StatusSeverity.None);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RebuildCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelRebuildCommand))]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearRebuildResultsCommand))]
    public partial bool IsRebuilding { get; set; }

    [ObservableProperty]
    public partial bool RebuildRemoveSource { get; set; } = false;

    [ObservableProperty]
    public partial DatabaseTargetContainerTypeViewModel? RebuildTargetContainerType { get; set; }

    [ObservableProperty]
    public partial bool RebuildForceCalculateChecksums { get; set; }

    [ObservableProperty]
    public partial bool RebuildFindMissingCueFiles { get; set; }

    [ObservableProperty]
    public partial string FilterDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RomFilterDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial RomStatus? RomFilterStatus { get; set; } = null;

    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DatabaseGameViewModel? SelectedGame { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    public partial bool UseOnlineStorage { get; set; } = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    public partial bool UseOfflineStorage { get; set; } = true;

    [ObservableProperty]
    public partial bool ForceCalculateChecksums { get; set; }

    [ObservableProperty]
    public partial string ScanMatchedFilesLabel { get; private set; } = Localized.ScanResultMatchedNoCountLabel;

    [ObservableProperty]
    public partial string ScanUnusedFilesLabel { get; private set; } = Localized.ScanResultUnusedNoCountLabel;

    [ObservableProperty]
    public partial string ScanMissingLabel { get; private set; } = Localized.ScanResultMissingNoCountLabel;

    [ObservableProperty]
    public partial string ScanIncorrectNameFilesLabel { get; private set; } = Localized.ScanResultWrongNameNoCountLabel;

    public ObservableCollection<DatabaseTargetContainerTypeViewModel> RebuildTargetContainerTypes { get; }

    public AdvancedCollectionView FilteredGames { get; }

    public ObservableCollection<DatabaseGameViewModel> Games { get; private set; } = [];

    public ObservableCollection<DatabaseScanResultActualViewModel> ScanMatchedFiles { get; private set; } = [];

    public AdvancedCollectionView ScanSortedMatchedFiles { get; }

    public ObservableCollection<DatabaseScanResultActualViewModel> ScanUnusedFiles { get; private set; } = [];

    public AdvancedCollectionView ScanSortedUnusedFiles { get; }

    public ObservableCollection<DatabaseScanResultExpectedViewModel> ScanMissingFiles { get; private set; } = [];

    public AdvancedCollectionView ScanSortedMissingFiles { get; }

    public ObservableCollection<DatabaseScanResultWrongNameViewModel> ScanIncorrectNameFiles { get; private set; } = [];

    public AdvancedCollectionView ScanSortedIncorrectNameFiles { get; }

    public ObservableCollection<string> RebuildMatchedFiles { get; private set; } = [];

    public AdvancedCollectionView RebuildSortedMatchedFiles { get; }

    public ObservableCollection<string> RebuildUnusedFiles { get; private set; } = [];

    public AdvancedCollectionView RebuildSortedUnusedFiles { get; }

    public DatabaseMetadataViewModel Metadata { get; }

    public DatabaseStatsViewModel Stats { get; }

    public static async Task<DatabaseFileViewModel> CreateAsync(
        DatabaseFile databaseFile,
        RomDatabase db,
        IDatabaseService databaseService,
        IOfflineExplorerService offlineExplorerService,
        IClipboardService clipboardService,
        IFilePickerService filePickerService,
        IFileExplorerService fileExplorerService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        IDispatcherQueue dispatcherQueue,
        CancellationToken ct)
    {
        var viewModel = new DatabaseFileViewModel(
            databaseFile,
            db,
            databaseService,
            offlineExplorerService,
            clipboardService,
            filePickerService,
            fileExplorerService,
            operationCompletionService,
            dateTimeProviderService,
            dispatcherQueue);

        await viewModel.LoadScanSettingsAsync(ct);
        await viewModel.LoadScanResultsAsync(ct);

        return viewModel;
    }

    private static string ContainerTypeToDisplayName(string type)
    {
        return type switch
        {
            KnownContainerTypes.Folder => Localized.ContainerTypeFolder,
            KnownContainerTypes.TorrentZip => Localized.ContainerTypeTorrentZip,
            KnownContainerTypes.TorrentSevenZip => Localized.ContainerTypeTorrentSevenZip,
            KnownContainerTypes.Zip => Localized.ContainerTypeZip,
            KnownContainerTypes.SevenZip => Localized.ContainerTypeSevenZipUncompressed,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    [RelayCommand(CanExecute = nameof(CanCancelScan))]
    private void CancelScan()
    {
        this.scanCancellationTokenSource?.Cancel();
        this.CancelScanCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanClearScanResults))]
    private async Task ClearScanResultsAsync(CancellationToken ct)
    {
        await this.databaseService.ClearScanResultsAsync(this.databaseFile, ct);
    }

    [RelayCommand(CanExecute = nameof(CanScan))]
    private async Task ScanAsync(CancellationToken ct)
    {
        if (!this.CanScan())
        {
            return;
        }

        this.scanCancellationTokenSource = new CancellationTokenSource();

        this.IsScanning = true;

        try
        {
            await this.databaseService.ClearScanResultsAsync(this.databaseFile, this.scanCancellationTokenSource.Token);
            await this.databaseService.ScanAsync(this.databaseFile, this.scanCancellationTokenSource.Token);

            await this.operationCompletionService.NotifyCompletion(
                OperationCompletionResult.Success,
                Localized.ScanSuccessNotification,
                CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            this.ScanStatus = new StatusViewModel(Localized.ScanProgressCanceled, StatusSeverity.Info);

            await this.operationCompletionService.NotifyCompletion(
                OperationCompletionResult.Cancelled,
                Localized.ScanCancelNotification,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            this.ScanStatus = new StatusViewModel(
                string.Format(CultureInfo.CurrentUICulture, Localized.ScanProgressErrorFormat, ex.Message),
                StatusSeverity.Error);

            await this.operationCompletionService.NotifyCompletion(
                OperationCompletionResult.Error,
                Localized.ScanErrorNotification,
                CancellationToken.None);
        }
        finally
        {
            this.IsScanning = false;
            this.scanCancellationTokenSource?.Dispose();
            this.scanCancellationTokenSource = null;
        }
    }

    private bool CanScan()
    {
        return !this.IsRebuilding && !this.IsScanning;
    }

    private bool CanCancelScan()
    {
        return this.IsScanning && this.scanCancellationTokenSource?.IsCancellationRequested == false;
    }

    private bool CanClearScanResults()
    {
        return !this.IsScanning && !string.IsNullOrEmpty(this.ScanStatus.Text);
    }

    [RelayCommand(CanExecute = nameof(CanCancelRebuild))]
    private void CancelRebuild()
    {
        this.rebuildCancellationTokenSource?.Cancel();
        this.CancelRebuildCommand.NotifyCanExecuteChanged();
    }

    private bool CanCancelRebuild()
    {
        return this.IsRebuilding && this.rebuildCancellationTokenSource?.IsCancellationRequested == false;
    }

    [RelayCommand(CanExecute = nameof(CanClearRebuildResults))]
    private void ClearRebuildResults()
    {
        this.RebuildStatus = new StatusViewModel(string.Empty, StatusSeverity.None);

        this.RebuildMatchedFiles.Clear();
        this.RebuildUnusedFiles.Clear();
    }

    private bool CanClearRebuildResults()
    {
        return !this.IsRebuilding && !string.IsNullOrEmpty(this.RebuildStatus.Text);
    }

    [RelayCommand(CanExecute = nameof(CanRebuild))]
    private async Task RebuildAsync()
    {
        if (!this.CanRebuild())
        {
            return;
        }

        this.ClearRebuildResults();

        this.rebuildCancellationTokenSource = new CancellationTokenSource();

        this.IsRebuilding = true;
        try
        {
            var settings = new RebuildSettings
            {
                SourceFolderPath = this.RebuildSourceFolderPath,
                TargetFolderPath = this.RebuildTargetFolderPath,
                TargetContainerType = this.RebuildTargetContainerType?.Type ?? KnownContainerTypes.Folder,
                ForceCalculateChecksums = this.RebuildForceCalculateChecksums,
                RemoveSource = this.RebuildRemoveSource,
                FindMissingCueFiles = this.RebuildFindMissingCueFiles,
            };

            await this.databaseService.RebuildAsync(this.databaseFile, settings, this.rebuildCancellationTokenSource.Token);

            this.RebuildStatus = new StatusViewModel(
                string.Format(CultureInfo.CurrentUICulture, Localized.RebuildProgressCompletedFormat, this.dateTimeProviderService.Now),
                StatusSeverity.Success);

            await this.operationCompletionService.NotifyCompletion(
                OperationCompletionResult.Success,
                Localized.RebuildSuccessNotification,
                CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            this.RebuildStatus = new StatusViewModel(Localized.RebuildProgressCanceled, StatusSeverity.Error);

            await this.operationCompletionService.NotifyCompletion(
                OperationCompletionResult.Cancelled,
                Localized.RebuildCancelNotification,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            this.RebuildStatus = new StatusViewModel(
                string.Format(CultureInfo.CurrentUICulture, Localized.RebuildProgressErrorFormat, ex.Message),
                StatusSeverity.Error);

            await this.operationCompletionService.NotifyCompletion(
                OperationCompletionResult.Error,
                Localized.RebuildErrorNotification,
                CancellationToken.None);
        }
        finally
        {
            this.IsRebuilding = false;
            this.rebuildCancellationTokenSource?.Dispose();
            this.rebuildCancellationTokenSource = null;

            this.FilteredGames.RefreshFilter();
        }
    }

    private bool CanRebuild()
    {
        return
            !this.IsRebuilding &&
            !this.IsScanning &&
            Directory.Exists(this.RebuildSourceFolderPath) &&
            !string.IsNullOrEmpty(this.RebuildTargetFolderPath);
    }

    [RelayCommand]
    private async Task AddScanOnlineFolderAsync(CancellationToken ct)
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.ScanOnlineFolder);
        if (!string.IsNullOrEmpty(folderPath))
        {
            var existing = this.ScanOnlineFolders.FirstOrDefault(f => f.FolderPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                this.ScanOnlineFolders.Add(new DatabaseScanOnlineFolderItemViewModel(this.RemoveScanFolderAsync, this.OnIsIncludedChanged) { FolderPath = folderPath, IsIncluded = true });
                this.ScanCommand.NotifyCanExecuteChanged();
                await this.SaveScanSettingsAsync(ct);
            }
        }
    }

    [RelayCommand]
    private async Task BrowseRebuildSourceFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.RebuildSourceFolder);
        if (!string.IsNullOrEmpty(folderPath))
        {
            this.RebuildSourceFolderPath = folderPath;
        }
    }

    [RelayCommand]
    private async Task BrowseRebuildTargetFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.RebuildTargetFolder);
        if (!string.IsNullOrEmpty(folderPath))
        {
            this.RebuildTargetFolderPath = folderPath;
        }
    }

    [RelayCommand]
    private void CopyHeaderInfo()
    {
        this.Metadata.CopyToClipboard();
    }

    [RelayCommand]
    private void CopyHeaderStats()
    {
        this.Stats.CopyToClipboard();
    }

    [RelayCommand]
    private void FilterAll()
    {
        this.filterKind = DatabaseGameFilterKind.All;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    private void FilterComplete()
    {
        this.filterKind = DatabaseGameFilterKind.Complete;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    private void FilterMissing()
    {
        this.filterKind = DatabaseGameFilterKind.Missing;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    private void FilterPartial()
    {
        this.filterKind = DatabaseGameFilterKind.Partial;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    private void FilterWrongName()
    {
        this.filterKind = DatabaseGameFilterKind.WrongName;
        this.UpdateFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    private void FilterRomStatus(string status)
    {
        switch (status)
        {
            case "all":
                this.RomFilterStatus = null;
                break;

            case "good":
                this.RomFilterStatus = RomStatus.Good;
                break;

            case "bad":
                this.RomFilterStatus = RomStatus.BadDump;
                break;

            case "verified":
                this.RomFilterStatus = RomStatus.Verified;
                break;

            case "nodump":
                this.RomFilterStatus = RomStatus.NoDump;
                break;
        }

        this.UpdateRomFilterDescription();
        this.FilteredGames.RefreshFilter();
    }

    [RelayCommand]
    private void CopyAllRebuildMatched()
    {
        this.clipboardService.SetText(string.Join(Environment.NewLine, this.RebuildMatchedFiles));
    }

    [RelayCommand]
    private void CopyRebuildMatched(string text)
    {
        this.clipboardService.SetText(text);
    }

    [RelayCommand]
    private void CopyAllRebuildUnused()
    {
        this.clipboardService.SetText(string.Join(Environment.NewLine, this.RebuildUnusedFiles));
    }

    [RelayCommand]
    private void CopyRebuildUnused(string text)
    {
        this.clipboardService.SetText(text);
    }

    [RelayCommand]
    private void CopyAllScanMatched()
    {
        this.clipboardService.SetText(string.Join(Environment.NewLine, this.ScanMatchedFiles.Select(item => item.ActualDisplayName)));
    }

    [RelayCommand]
    private void CopyScanMatched(DatabaseScanResultActualViewModel viewModel)
    {
        this.clipboardService.SetText(viewModel.ActualDisplayName);
    }

    [RelayCommand]
    private void CopyAllScanMissing()
    {
        this.clipboardService.SetText(string.Join(Environment.NewLine, this.ScanMissingFiles.Select(item => item.ExpectedDisplayName)));
    }

    [RelayCommand]
    private void CopyScanMissing(DatabaseScanResultExpectedViewModel viewModel)
    {
        this.clipboardService.SetText(viewModel.ExpectedDisplayName);
    }

    [RelayCommand]
    private void CopyAllScanUnused()
    {
        this.clipboardService.SetText(string.Join(Environment.NewLine, this.ScanUnusedFiles.Select(item => item.ActualDisplayName)));
    }

    [RelayCommand]
    private void CopyScanUnused(DatabaseScanResultActualViewModel viewModel)
    {
        this.clipboardService.SetText(viewModel.ActualDisplayName);
    }

    [RelayCommand]
    private void CopyAllScanWrongNamed()
    {
        this.clipboardService.SetText(string.Join(Environment.NewLine + Environment.NewLine, this.ScanIncorrectNameFiles.Select(item => $"Actual:   {item.ActualDisplayName}{Environment.NewLine}Expected: {item.ExpectedDisplayName}")));
    }

    [RelayCommand]
    private void CopyAllScanWrongNamedActual()
    {
        this.clipboardService.SetText(string.Join(Environment.NewLine, this.ScanIncorrectNameFiles.Select(item => item.ActualDisplayName)));
    }

    [RelayCommand]
    private void CopyAllScanWrongNamedExpected()
    {
        this.clipboardService.SetText(string.Join(Environment.NewLine, this.ScanIncorrectNameFiles.Select(item => item.ExpectedDisplayName)));
    }

    [RelayCommand(CanExecute = nameof(CanOpenScanWrongNamedInExplorer))]
    private void OpenScanWrongNamedInExplorer(DatabaseScanResultWrongNameViewModel viewModel)
    {
        this.fileExplorerService.OpenInExplorer(viewModel.ActualContainerAbsolutePath);
    }

    [RelayCommand(CanExecute = nameof(CanOpenScanActualInExplorer))]
    private void OpenScanActualInExplorer(DatabaseScanResultActualViewModel viewModel)
    {
        this.fileExplorerService.OpenInExplorer(viewModel.ActualContainerAbsolutePath);
    }

    private bool CanOpenScanWrongNamedInExplorer(DatabaseScanResultWrongNameViewModel viewModel)
    {
        return viewModel is not null && (File.Exists(viewModel.ActualContainerAbsolutePath) || Directory.Exists(viewModel.ActualContainerAbsolutePath));
    }

    private bool CanOpenScanActualInExplorer(DatabaseScanResultActualViewModel viewModel)
    {
        return viewModel is not null && (File.Exists(viewModel.ActualContainerAbsolutePath) || Directory.Exists(viewModel.ActualContainerAbsolutePath));
    }

    [RelayCommand]
    private void CopyScanWrongNamedActual(DatabaseScanResultWrongNameViewModel viewModel)
    {
        this.clipboardService.SetText($"{viewModel.ActualContainerName}\\{viewModel.ActualFileRelativePath}");
    }

    [RelayCommand]
    private void CopyScanWrongNamedExpected(DatabaseScanResultWrongNameViewModel viewModel)
    {
        this.clipboardService.SetText($"{viewModel.ExpectedContainerName}\\{viewModel.ExpectedFileRelativePath}");
    }

    private void DatabaseService_ScanProgress(object? sender, ScanEventArgs e)
    {
        if (e.DatabaseFile.FullPath != this.databaseFile.FullPath)
        {
            // Ignore events for other database files.
            return;
        }

        this.dispatcherQueue.TryEnqueue(() =>
        {
            switch (e.Status)
            {
                case Core.Contracts.Models.ScanStatus.Pending:
                    this.IsScanning = true;
                    this.ScanStatus = new StatusViewModel(Localized.ScanProgressPending, StatusSeverity.Info);
                    break;
                case Core.Contracts.Models.ScanStatus.Started:
                    this.ScanStatus = new StatusViewModel(Localized.ScanProgressStarted, StatusSeverity.Info);
                    break;
                case Core.Contracts.Models.ScanStatus.Scanning:
                    this.ScanStatus = new StatusViewModel(Localized.ScanProgressScanning, StatusSeverity.Info);
                    break;
                case Core.Contracts.Models.ScanStatus.Hashing:
                    Debug.Assert(e.HashingFile is not null, "Hashing file should not be null in Hashing status.");
                    this.ScanStatus = new StatusViewModel(
                        string.Format(CultureInfo.CurrentUICulture, Localized.ScanProgressHashingFormat, $"{e.HashingFile?.ContainerName}\\{e.HashingFile?.RomRelativeFilePath}"),
                        StatusSeverity.Info);
                    break;
                case Core.Contracts.Models.ScanStatus.Completed:
                    this.IsScanning = false;
                    this.ScanStatus = new StatusViewModel(
                        string.Format(CultureInfo.CurrentUICulture, Localized.ScanProgressCompletedFormat, this.dateTimeProviderService.Now),
                        StatusSeverity.Success);
                    this.FilteredGames.RefreshFilter();
                    break;
                case Core.Contracts.Models.ScanStatus.Canceled:
                    this.IsScanning = false;
                    this.ScanStatus = new StatusViewModel(Localized.ScanProgressCanceled, StatusSeverity.Error);
                    break;
                case Core.Contracts.Models.ScanStatus.Cleared:
                    this.ScanStatus = new StatusViewModel(string.Empty, StatusSeverity.None);

                    this.ScanMatchedFiles.Clear();
                    this.ScanMissingFiles.Clear();
                    this.ScanUnusedFiles.Clear();
                    this.ScanIncorrectNameFiles.Clear();

                    foreach (var game in this.Games)
                    {
                        game.Status = DatabaseGameStatus.Unknown;
                        game.IsContainerWrongName = false;

                        foreach (var rom in game.Roms)
                        {
                            rom.Status = DatabaseRomStatus.Unknown;
                        }
                    }

                    break;
                default:
                    break;
            }

            this.AddScanResults(e.Results);
        });
    }

    private void AddScanResults(DatabaseScanResults results)
    {
        foreach (var missing in results.Missing)
        {
            var viewModel = new DatabaseScanResultExpectedViewModel
            {
                ExpectedContainerName = missing.ContainerName,
                ExpectedFileRelativePath = missing.RomRelativeFilePath,
            };

            this.ScanMissingFiles.Add(viewModel);

            var game = this.Games.SingleOrDefault(temp => temp.Name == missing.ContainerName);
            if (game is not null)
            {
                var rom = game.Roms.SingleOrDefault(temp => temp.Name == missing.RomRelativeFilePath);
                if (rom is not null)
                {
                    rom.Status = DatabaseRomStatus.NotFound;
                }

                game.RefreshStatus();
            }
        }

        foreach (var wrongNamed in results.WrongNamed)
        {
            var viewModel = new DatabaseScanResultWrongNameViewModel
            {
                ActualContainerAbsolutePath = wrongNamed.File.ContainerPath,
                ActualContainerName = wrongNamed.File.ContainerName,
                ActualFileRelativePath = wrongNamed.File.RomRelativeFilePath,
                ExpectedContainerName = wrongNamed.Rom.ContainerName,
                ExpectedFileRelativePath = wrongNamed.Rom.RomRelativeFilePath,
            };

            this.ScanIncorrectNameFiles.Add(viewModel);

            var game = this.Games.SingleOrDefault(temp => temp.Name == wrongNamed.Rom.ContainerName);
            if (game is not null)
            {
                var rom = game.Roms.SingleOrDefault(temp => temp.Name == wrongNamed.Rom.RomRelativeFilePath);
                if (rom is not null)
                {
                    if (wrongNamed.File.RomRelativeFilePath != wrongNamed.Rom.RomRelativeFilePath)
                    {
                        rom.Status = DatabaseRomStatus.FoundWrongName;
                    }
                    else
                    {
                        rom.Status = DatabaseRomStatus.Found;
                    }
                }

                if (wrongNamed.File.ContainerName != wrongNamed.Rom.ContainerName)
                {
                    game.IsContainerWrongName = true;
                }

                game.RefreshStatus();
            }
        }

        foreach (var matched in results.Matched)
        {
            var viewModel = new DatabaseScanResultActualViewModel
            {
                ActualContainerAbsolutePath = matched.File.ContainerPath,
                ActualContainerName = matched.File.ContainerName,
                ActualFileRelativePath = matched.File.RomRelativeFilePath,
            };

            this.ScanMatchedFiles.Add(viewModel);

            var game = this.Games.SingleOrDefault(temp => temp.Name == matched.Rom.ContainerName);
            if (game is not null)
            {
                var rom = game.Roms.SingleOrDefault(temp => temp.Name == matched.Rom.RomRelativeFilePath);
                if (rom is not null)
                {
                    rom.Status = DatabaseRomStatus.Found;
                }

                game.RefreshStatus();
            }
        }

        foreach (var unused in results.Unused)
        {
            var viewModel = new DatabaseScanResultActualViewModel
            {
                ActualContainerAbsolutePath = unused.ContainerPath,
                ActualContainerName = unused.ContainerName,
                ActualFileRelativePath = unused.RomRelativeFilePath,
            };

            this.ScanUnusedFiles.Add(viewModel);
        }
    }

    private void DatabaseService_RebuildProgress(object? sender, RebuildEventArgs e)
    {
        this.dispatcherQueue.TryEnqueue(() =>
        {
            switch (e.Status)
            {
                case Core.Contracts.Models.RebuildStatus.Pending:
                    this.RebuildStatus = new StatusViewModel(Localized.RebuildProgressPending, StatusSeverity.Info);
                    break;
                case Core.Contracts.Models.RebuildStatus.Started:
                    this.RebuildStatus = new StatusViewModel(Localized.RebuildProgressStarted, StatusSeverity.Info);
                    break;
                case Core.Contracts.Models.RebuildStatus.Scanning:
                    this.RebuildStatus = new StatusViewModel(Localized.RebuildProgressScanning, StatusSeverity.Info);
                    break;
                case Core.Contracts.Models.RebuildStatus.Hashing:
                    Debug.Assert(e.HashingFile is not null, "Hashing file should not be null in Hashing status.");
                    this.RebuildStatus = new StatusViewModel(
                        string.Format(CultureInfo.CurrentUICulture, Localized.RebuildProgressHashingFormat, $"{e.HashingFile?.ContainerName}\\{e.HashingFile?.RomRelativeFilePath}"),
                        StatusSeverity.Info);
                    break;
                case Core.Contracts.Models.RebuildStatus.Building:
                    Debug.Assert(e.BuildingRom is not null, "Building rom should not be null in Building status.");
                    this.RebuildStatus = new StatusViewModel(
                        string.Format(CultureInfo.CurrentUICulture, Localized.RebuildProgressBuildingFormat, $"{e.BuildingRom?.ContainerName}\\{e.BuildingRom?.RomRelativeFilePath}"),
                        StatusSeverity.Info);
                    break;
                default:
                    break;
            }

            foreach (var matched in e.Results.Matched)
            {
                this.RebuildMatchedFiles.Add(matched.Rom.ContainerName + "\\" + matched.Rom.RomRelativeFilePath);
            }

            foreach (var unused in e.Results.Unused)
            {
                this.RebuildUnusedFiles.Add(unused.ContainerName + "\\" + unused.RomRelativeFilePath);
            }
        });
    }

    private async Task ApplyBrowseOfflineFolderSelectionAsync(OfflineDisk disk, string folderPath, CancellationToken ct)
    {
        if (disk is null || string.IsNullOrEmpty(folderPath))
        {
            return;
        }

        var existing = this.ScanOfflineFolders.FirstOrDefault(f => f.FolderPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            this.ScanOfflineFolders.Add(new DatabaseScanOfflineFolderItemViewModel(disk.Name, folderPath, this.RemoveScanFolderAsync, this.OnIsIncludedChanged) { IsIncluded = true });
            this.ScanCommand.NotifyCanExecuteChanged();
            await this.SaveScanSettingsAsync(ct);
        }
    }

    private bool IsFilterSizeMatch(DatabaseGameViewModel game)
    {
        return this.filterTextNumeric.HasValue && game.Roms.Any(r => r.Size == this.filterTextNumeric);
    }

    private bool IsFilterChecksumMatch(DatabaseGameViewModel game)
    {
        switch (this.FilterText.Length)
        {
            case 8:
                return game.Roms.Any(r => r.CRC32 == this.FilterText);
            case 32:
                return game.Roms.Any(r => r.MD5 == this.FilterText);
            case 40:
                return game.Roms.Any(r => r.SHA1 == this.FilterText);
            case 64:
                return game.Roms.Any(r => r.SHA256 == this.FilterText);
            default:
                return false;
        }
    }

    private bool IsFilterNameMatch(DatabaseGameViewModel game)
    {
        return string.IsNullOrEmpty(this.FilterText) || game.Name.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsRomFilterMatch(DatabaseGameViewModel game)
    {
        if (this.RomFilterStatus is null)
        {
            return true;
        }

        return game.Roms.Any(r => r.RomStatus == this.RomFilterStatus);
    }

    private bool IsFilterStatusMatch(DatabaseGameViewModel game)
    {
        return this.filterKind == DatabaseGameFilterKind.All ||
            (this.filterKind == DatabaseGameFilterKind.Complete && game.Status == DatabaseGameStatus.Complete) ||
            (this.filterKind == DatabaseGameFilterKind.Partial && game.Status == DatabaseGameStatus.Partial) ||
            (this.filterKind == DatabaseGameFilterKind.Missing && game.Status == DatabaseGameStatus.Missing) ||
            (this.filterKind == DatabaseGameFilterKind.WrongName && (game.Status == DatabaseGameStatus.PartialIncorrectName || game.Status == DatabaseGameStatus.CompleteIncorrectName));
    }

    partial void OnFilterTextChanged(string value)
    {
        if (long.TryParse(value, out long result))
        {
            this.filterTextNumeric = result;
        }
        else
        {
            this.filterTextNumeric = null;
        }

        this.FilteredGames.RefreshFilter();
    }

    private void UpdateFilterDescription()
    {
        switch (this.filterKind)
        {
            case DatabaseGameFilterKind.All:
                this.FilterDescription = Localized.ScanStatusFilterAll;
                break;
            case DatabaseGameFilterKind.Missing:
                this.FilterDescription = Localized.ScanStatusFilterMissing;
                break;
            case DatabaseGameFilterKind.Complete:
                this.FilterDescription = Localized.ScanStatusFilterComplete;
                break;
            case DatabaseGameFilterKind.Partial:
                this.FilterDescription = Localized.ScanStatusFilterPartial;
                break;
            case DatabaseGameFilterKind.WrongName:
                this.FilterDescription = Localized.ScanStatusFilterWrongName;
                break;
            case DatabaseGameFilterKind.Unknown:
                this.FilterDescription = Localized.ScanStatusFilterUnknown;
                break;
            default:
                break;
        }
    }

    private void UpdateRomFilterDescription()
    {
        if (this.RomFilterStatus is null)
        {
            this.RomFilterDescription = Localized.DumpStatusFilterDescriptionAll;
            return;
        }

        switch (this.RomFilterStatus.Value)
        {
            case RomStatus.BadDump:
                this.RomFilterDescription = Localized.DumpStatusFilterDescriptionBad;
                break;
            case RomStatus.NoDump:
                this.RomFilterDescription = Localized.DumpStatusFilterDescriptionNoDump;
                break;
            case RomStatus.Good:
                this.RomFilterDescription = Localized.DumpStatusFilterDescriptionGood;
                break;
            case RomStatus.Verified:
                this.RomFilterDescription = Localized.DumpStatusFilterDescriptionVerified;
                break;
            default:
                break;
        }
    }

    private async Task RemoveScanFolderAsync(DatabaseScanOnlineFolderItemViewModel folder, CancellationToken ct)
    {
        if (folder is not null && this.ScanOnlineFolders.Contains(folder))
        {
            this.ScanOnlineFolders.Remove(folder);
            this.ScanCommand.NotifyCanExecuteChanged();
            await this.SaveScanSettingsAsync(ct);
        }
    }

    private async Task RemoveScanFolderAsync(DatabaseScanOfflineFolderItemViewModel folder, CancellationToken ct)
    {
        if (folder is not null && this.ScanOfflineFolders.Contains(folder))
        {
            this.ScanOfflineFolders.Remove(folder);
            this.ScanCommand.NotifyCanExecuteChanged();
            await this.SaveScanSettingsAsync(ct);
        }
    }

    private void OnIsIncludedChanged(DatabaseScanOnlineFolderItemViewModel onlineFolderItemViewModel, bool value)
    {
        this.ScanCommand.NotifyCanExecuteChanged();
        this.SaveScanSettingsAsync(CancellationToken.None).SafeFireAndForget();
    }

    private void OnIsIncludedChanged(DatabaseScanOfflineFolderItemViewModel offlineFolderItemViewModel, bool value)
    {
        this.ScanCommand.NotifyCanExecuteChanged();
        this.SaveScanSettingsAsync(CancellationToken.None).SafeFireAndForget();
    }

    partial void OnUseOfflineStorageChanged(bool value)
    {
        if (!this.isLoadingScanSettings)
        {
            this.SaveScanSettingsAsync(CancellationToken.None).SafeFireAndForget();
        }
    }

    partial void OnUseOnlineStorageChanged(bool value)
    {
        if (!this.isLoadingScanSettings)
        {
            this.SaveScanSettingsAsync(CancellationToken.None).SafeFireAndForget();
        }
    }

    partial void OnForceCalculateChecksumsChanged(bool value)
    {
        if (!this.isLoadingScanSettings)
        {
            this.SaveScanSettingsAsync(CancellationToken.None).SafeFireAndForget();
        }
    }

    private async Task LoadScanResultsAsync(CancellationToken ct)
    {
        var results = await this.databaseService.GetScanResultsAsync(this.databaseFile, ct);
        if (results is not null)
        {
            this.AddScanResults(results);
        }
    }

    private async Task LoadScanSettingsAsync(CancellationToken ct)
    {
        try
        {
            this.isLoadingScanSettings = true;

            var settings = await this.databaseService.GetFileScanSettingsAsync(this.db.Name, ct);

            this.ForceCalculateChecksums = settings.ForceCalculateChecksums;
            this.UseOnlineStorage = settings.UseOnlineFolders;
            this.ScanOnlineFolders = new ObservableCollection<DatabaseScanOnlineFolderItemViewModel>(settings.ScanOnlineFolders.Select(sf => new DatabaseScanOnlineFolderItemViewModel(this.RemoveScanFolderAsync, this.OnIsIncludedChanged) { FolderPath = sf.FolderPath, IsIncluded = sf.IsIncluded }));
            this.UseOfflineStorage = settings.UseOfflineFolders;
            this.ScanOfflineFolders = new ObservableCollection<DatabaseScanOfflineFolderItemViewModel>(settings.ScanOfflineFolders.Select(sf => new DatabaseScanOfflineFolderItemViewModel(sf.DiskName, sf.FolderPath, this.RemoveScanFolderAsync, this.OnIsIncludedChanged) { IsIncluded = sf.IsIncluded }));
        }
        finally
        {
            this.isLoadingScanSettings = false;
        }
    }

    private async Task SaveScanSettingsAsync(CancellationToken ct)
    {
        var settings = new DatabaseFileScanSettings
        {
            ForceCalculateChecksums = this.ForceCalculateChecksums,
            UseOnlineFolders = this.UseOnlineStorage,
            ScanOnlineFolders = [.. this.ScanOnlineFolders.Select(sf => new ScanOnlineFolderSetting { FolderPath = sf.FolderPath, IsIncluded = sf.IsIncluded })],
            UseOfflineFolders = this.UseOfflineStorage,
            ScanOfflineFolders = [.. this.ScanOfflineFolders.Select(sf => new ScanOfflineFolderSetting { DiskName = sf.DiskName, FolderPath = sf.FolderPath, IsIncluded = sf.IsIncluded })],
        };

        await this.databaseService.SetFileScanSettingsAsync(this.db.Name, settings, ct);
    }
}
