// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Models;

[DebuggerDisplay("Name = {databaseFolder.Name} Path = {databaseFolder.FullPath}")]
public sealed partial class DatabaseFolderViewModel : ObservableObject, IDisposable
{
    private readonly DisposableBag disposables = DisposableBag.Create<DatabaseFolderViewModel>();

    private readonly DatabaseFolder databaseFolder;
    private readonly IDatabaseService databaseService;
    private readonly IClipboardService clipboardService;
    private readonly IFilePickerService filePickerService;
    private readonly IFileExplorerService fileExplorerService;
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly ILogger logger;
    private readonly Lazy<DatabaseStatsViewModel> stats;

    private CancellationTokenSource? scanCancellationTokenSource;
    private bool isLoadingScanSettings = false;

    private DatabaseFolderViewModel(
        DatabaseFolder folder,
        IDatabaseService databaseService,
        IClipboardService clipboardService,
        IFilePickerService filePickerService,
        IFileExplorerService fileExplorerService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        IDispatcherQueue dispatcherQueue,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(folder);
        ArgumentNullException.ThrowIfNull(databaseService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(fileExplorerService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);
        ArgumentNullException.ThrowIfNull(dispatcherQueue);
        ArgumentNullException.ThrowIfNull(logger);

        this.databaseFolder = folder;
        this.databaseService = databaseService;
        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.fileExplorerService = fileExplorerService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.dispatcherQueue = dispatcherQueue;
        this.logger = logger;

        this.databaseService.ScanProgress += this.DatabaseService_ScanProgress;
        this.disposables.Add(() => this.databaseService.ScanProgress -= this.DatabaseService_ScanProgress);

        // This will load all the databases in memory in order to compute the
        // stats, so we do it only if the Stats property is accessed.
        this.stats = new Lazy<DatabaseStatsViewModel>(CreateStats);

        DatabaseStatsViewModel CreateStats() =>
            new(
                this.databaseService,
                this.clipboardService,
                this.logger,
                this.databaseFolder,
                () => Task.Run(() => this.CopyHeaderStatsCommand.NotifyCanExecuteChanged()));
    }

    public DatabaseStatsViewModel Stats => this.stats.Value;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearScanResultsCommand))]
    public partial StatusViewModel ScanStatus { get; set; } = new StatusViewModel();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearScanResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelScanCommand))]
    public partial bool IsScanning { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    public partial ObservableCollection<DatabaseScanOnlineFolderItemViewModel> ScanOnlineFolders { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    public partial ObservableCollection<DatabaseScanOfflineFolderItemViewModel> ScanOfflineFolders { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    public partial bool UseOnlineStorage { get; set; } = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
    public partial bool UseOfflineStorage { get; set; } = true;

    public static async Task<DatabaseFolderViewModel> CreateAsync(
        DatabaseFolder folder,
        IDatabaseService databaseService,
        IClipboardService clipboardService,
        IFilePickerService filePickerService,
        IFileExplorerService fileExplorerService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        IDispatcherQueue dispatcherQueue,
        ILogger logger,
        CancellationToken ct)
    {
        var viewModel = new DatabaseFolderViewModel(
            folder,
            databaseService,
            clipboardService,
            filePickerService,
            fileExplorerService,
            operationCompletionService,
            dateTimeProviderService,
            dispatcherQueue,
            logger);
        await viewModel.LoadScanSettingsAsync(ct);
        return viewModel;
    }

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    [RelayCommand(CanExecute = nameof(CanCancelScan))]
    private void CancelScan()
    {
        try
        {
            this.scanCancellationTokenSource?.Cancel();
            this.CancelScanCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand(CanExecute = nameof(CanClearScanResults))]
    private async Task ClearScanResultsAsync(CancellationToken ct)
    {
        try
        {
            await this.databaseService.ClearScanResultsAsync(this.databaseFolder, ct);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand(CanExecute = nameof(CanScan))]
    private async Task ScanAsync(CancellationToken ct)
    {
        try
        {
            if (!this.CanScan())
            {
                return;
            }

            this.scanCancellationTokenSource = new CancellationTokenSource();

            this.IsScanning = true;

            try
            {
                await this.databaseService.ClearScanResultsAsync(this.databaseFolder, this.scanCancellationTokenSource.Token);
                await this.databaseService.ScanAsync(this.databaseFolder, this.scanCancellationTokenSource.Token);

                this.ScanStatus = new StatusViewModel(
                    string.Format(CultureInfo.CurrentUICulture, Localized.ScanProgressCompletedFormat, this.dateTimeProviderService.Now),
                    StatusSeverity.Success);

                await this.operationCompletionService.NotifyCompletion(
                    OperationCompletionResult.Success,
                    Localized.ScanSuccessNotification,
                    CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                this.ScanStatus = new StatusViewModel(Localized.ScanProgressCanceled, StatusSeverity.Error);

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
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task AddScanOfflineFolderAsync(CancellationToken ct)
    {
        try
        {
            var offlineDiskFolder = await this.filePickerService.GetOfflineDiskFolderAsync();
            if (offlineDiskFolder is not null)
            {
                var existing = this.ScanOfflineFolders.FirstOrDefault(f => f.DiskName.Equals(offlineDiskFolder.DiskName, StringComparison.OrdinalIgnoreCase) && f.FolderPath.Equals(offlineDiskFolder.FolderPath, StringComparison.OrdinalIgnoreCase));
                if (existing is null)
                {
                    this.ScanOfflineFolders.Add(new DatabaseScanOfflineFolderItemViewModel(offlineDiskFolder.DiskName, offlineDiskFolder.FolderPath, this.OnIsIncludedChanged, this.RemoveScanOfflineFolderCommand) { IsIncluded = true });
                    this.ScanCommand.NotifyCanExecuteChanged();
                    await this.SaveScanSettingsAsync(ct);
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task RemoveScanOfflineFolderAsync(DatabaseScanOfflineFolderItemViewModel folder, CancellationToken ct)
    {
        try
        {
            if (folder is not null && this.ScanOfflineFolders.Contains(folder))
            {
                this.ScanOfflineFolders.Remove(folder);
                this.ScanCommand.NotifyCanExecuteChanged();
                await this.SaveScanSettingsAsync(ct);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task AddScanOnlineFolderAsync(CancellationToken ct)
    {
        try
        {
            var folderPath = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.ScanOnlineFolder);
            if (!string.IsNullOrEmpty(folderPath))
            {
                var existing = this.ScanOnlineFolders.FirstOrDefault(f => f.Path.Equals(folderPath, StringComparison.OrdinalIgnoreCase));
                if (existing is null)
                {
                    this.ScanOnlineFolders.Add(new DatabaseScanOnlineFolderItemViewModel(this.fileExplorerService, this.dispatcherQueue, this.logger, folderPath, this.RemoveScanOnlineFolderCommand, this.OnIsIncludedChanged) { IsIncluded = true });
                    this.ScanCommand.NotifyCanExecuteChanged();
                    await this.SaveScanSettingsAsync(ct);
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private async Task RemoveScanOnlineFolderAsync(DatabaseScanOnlineFolderItemViewModel folder, CancellationToken ct)
    {
        try
        {
            if (folder is not null && this.ScanOnlineFolders.Contains(folder))
            {
                this.ScanOnlineFolders.Remove(folder);
                this.ScanCommand.NotifyCanExecuteChanged();
                await this.SaveScanSettingsAsync(ct);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopyHeaderStats))]
    private void CopyHeaderStats()
    {
        try
        {
            this.Stats.CopyToClipboard();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private bool CanCopyHeaderStats()
    {
        return !this.Stats.IsCalculating;
    }

    private bool CanScan()
    {
        return
            !this.IsScanning &&
            ((this.UseOnlineStorage && this.ScanOnlineFolders.Any(sf => sf.IsIncluded && Directory.Exists(sf.Path))) || (this.UseOfflineStorage && this.ScanOfflineFolders.Any(sf => sf.IsIncluded)));
    }

    private bool CanCancelScan()
    {
        return this.IsScanning && this.scanCancellationTokenSource?.IsCancellationRequested == false;
    }

    private bool CanClearScanResults()
    {
        return !this.IsScanning && !string.IsNullOrEmpty(this.ScanStatus.Text);
    }

    private void DatabaseService_ScanProgress(object? sender, ScanEventArgs e)
    {
        if (e.Database is null)
        {
            return;
        }

        if (!this.IsScanning && e.Status != Core.Contracts.Models.ScanStatus.Cleared)
        {
            return;
        }

        this.dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                switch (e.Status)
                {
                    case Core.Contracts.Models.ScanStatus.Pending:
                        break;
                    case Core.Contracts.Models.ScanStatus.Started:
                        break;
                    case Core.Contracts.Models.ScanStatus.Scanning:
                        this.ScanStatus = new StatusViewModel(
                            string.Format(CultureInfo.CurrentUICulture, Localized.ScanFolderProgressScanningFormat, e.Database.Name),
                            StatusSeverity.Info);
                        break;
                    case Core.Contracts.Models.ScanStatus.Hashing:
                        break;
                    case Core.Contracts.Models.ScanStatus.Completed:
                        break;
                    case Core.Contracts.Models.ScanStatus.Canceled:
                        break;
                    case Core.Contracts.Models.ScanStatus.Cleared:
                        this.ScanStatus = new StatusViewModel();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in DatabaseService_ScanProgress");
            }
        });
    }

    private async Task LoadScanSettingsAsync(CancellationToken ct)
    {
        try
        {
            this.isLoadingScanSettings = true;

            var settings = await this.databaseService.GetFolderScanSettingsAsync(this.databaseFolder.FullPath, ct);

            this.UseOnlineStorage = settings.UseOnlineFolders;
            this.ScanOnlineFolders = new ObservableCollection<DatabaseScanOnlineFolderItemViewModel>(settings.ScanOnlineFolders.Select(sf => new DatabaseScanOnlineFolderItemViewModel(this.fileExplorerService, this.dispatcherQueue, this.logger, sf.FolderPath, this.RemoveScanOnlineFolderCommand, this.OnIsIncludedChanged) { IsIncluded = sf.IsIncluded }));
            this.UseOfflineStorage = settings.UseOfflineFolders;
            this.ScanOfflineFolders = new ObservableCollection<DatabaseScanOfflineFolderItemViewModel>(settings.ScanOfflineFolders.Select(sf => new DatabaseScanOfflineFolderItemViewModel(sf.DiskName, sf.FolderPath, this.OnIsIncludedChanged, this.RemoveScanOfflineFolderCommand) { IsIncluded = sf.IsIncluded }));
        }
        finally
        {
            this.isLoadingScanSettings = false;
        }
    }

    private async Task SaveScanSettingsAsync(CancellationToken ct)
    {
        var settings = new DatabaseFolderScanSettings
        {
            UseOnlineFolders = this.UseOnlineStorage,
            ScanOnlineFolders = [.. this.ScanOnlineFolders.Select(sf => new ScanOnlineFolderSetting { FolderPath = sf.Path, IsIncluded = sf.IsIncluded })],
            UseOfflineFolders = this.UseOfflineStorage,
            ScanOfflineFolders = [.. this.ScanOfflineFolders.Select(sf => new ScanOfflineFolderSetting { DiskName = sf.DiskName, FolderPath = sf.FolderPath, IsIncluded = sf.IsIncluded })],
        };

        await this.databaseService.SetFolderScanSettingsAsync(this.databaseFolder.FullPath, settings, ct);
    }

    private void OnIsIncludedChanged(DatabaseScanOnlineFolderItemViewModel onlineFolderItemViewModel, bool value)
    {
        if (!this.isLoadingScanSettings)
        {
            this.ScanCommand.NotifyCanExecuteChanged();
            this.SaveScanSettingsAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error saving scan settings."));
        }
    }

    private void OnIsIncludedChanged(DatabaseScanOfflineFolderItemViewModel offlineFolderItemViewModel, bool value)
    {
        if (!this.isLoadingScanSettings)
        {
            this.ScanCommand.NotifyCanExecuteChanged();
            this.SaveScanSettingsAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error saving scan settings."));
        }
    }

    partial void OnUseOfflineStorageChanged(bool value)
    {
        if (!this.isLoadingScanSettings)
        {
            this.SaveScanSettingsAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error saving scan settings."));
        }
    }

    partial void OnUseOnlineStorageChanged(bool value)
    {
        if (!this.isLoadingScanSettings)
        {
            this.SaveScanSettingsAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error saving scan settings."));
        }
    }
}
