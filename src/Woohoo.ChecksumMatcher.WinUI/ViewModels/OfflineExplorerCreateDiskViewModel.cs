// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
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
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public sealed partial class OfflineExplorerCreateDiskViewModel : ObservableObject, IDisposable
{
    private readonly DisposableBag disposables = DisposableBag.Create<OfflineExplorerCreateDiskViewModel>();

    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly IFilePickerService filePickerService;
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly VolumeWatcher logicalDiskManager;

    private CancellationTokenSource? cancellationTokenSource;

    public OfflineExplorerCreateDiskViewModel(
        IOfflineExplorerService offlineExplorerService,
        IFilePickerService filePickerService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        IDispatcherQueueService dispatcherQueueService,
        ILogger<OfflineExplorerCreateDiskViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(logger);

        this.offlineExplorerService = offlineExplorerService;
        this.filePickerService = filePickerService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.logicalDiskManager = new VolumeWatcher();
        this.logicalDiskManager.VolumesChanged += this.LogicalDiskManager_VolumesChanged;
        this.disposables.Add(() => this.logicalDiskManager.VolumesChanged -= this.LogicalDiskManager_VolumesChanged);

        this.offlineExplorerService.DiskCreateProgress += this.OfflineExplorerService_DiskCreateProgress;
        this.disposables.Add(() => this.offlineExplorerService.DiskCreateProgress -= this.OfflineExplorerService_DiskCreateProgress);

        this.Drives = new(DriveInfo.GetDrives().Select(info => new OfflineExplorerDriveViewModel(info, this.dispatcherQueue, this.logger)));
        this.SelectedDrive = null;

        this.LoadSettingsAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error loading settings."));
    }

    public ObservableCollection<string> TargetFolders { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    public partial string TargetFolder { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    public partial string DiskName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StatusViewModel Status { get; set; } = new StatusViewModel();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    public partial bool IsIndexing { get; set; }

    [ObservableProperty]
    public partial bool IsIndexingError { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    public partial OfflineExplorerDriveViewModel? SelectedDrive { get; set; }

    public ObservableCollection<OfflineExplorerDriveViewModel> Drives { get; }

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    private void LogicalDiskManager_VolumesChanged(object? sender, EventArgs e)
    {
        this.dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                this.RefreshDrives();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in LogicalDiskManager_VolumesChanged");
            }
        });
    }

    [RelayCommand]
    private async Task BrowseTargetFolderAsync()
    {
        try
        {
            var folderPath = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.OfflineStorageTargetFolder);
            if (!string.IsNullOrEmpty(folderPath))
            {
                this.TargetFolder = folderPath;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        try
        {
            var previousSelectedDrive = this.SelectedDrive;

            this.Drives.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                this.Drives.Add(new OfflineExplorerDriveViewModel(drive, this.dispatcherQueue, this.logger));
            }

            // Restore the previously selected drive if it still exists.
            this.SelectedDrive = previousSelectedDrive is not null
                ? this.Drives.FirstOrDefault(d => d.Name == previousSelectedDrive.Name)
                : null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        try
        {
            this.cancellationTokenSource?.Cancel();
            this.CancelCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateDisk))]
    private async Task CreateDiskAsync()
    {
        try
        {
            if (this.SelectedDrive is null)
            {
                return;
            }

            this.cancellationTokenSource = new CancellationTokenSource();

            var diskName = this.DiskName.Trim();

            this.IsIndexing = true;
            this.IsIndexingError = false;
            this.Status = new StatusViewModel(
                string.Format(
                    CultureInfo.CurrentUICulture,
                    Localized.OfflineDiskProgressCreatingFormat,
                    diskName),
                StatusSeverity.Info);

            try
            {
                var sourceFolderPath = this.SelectedDrive.FullPath;
                var targetDiskFilePath = this.GetTargetDiskFilePath(diskName);
                var settings = new OfflineDiskCreateSettings
                {
                    CalculateChecksums = false,
                    IndexArchiveContent = true,
                };

                var result = await this.offlineExplorerService.CreateDiskAsync(
                    sourceFolderPath,
                    targetDiskFilePath,
                    diskName,
                    settings,
                    this.cancellationTokenSource.Token);

                if (result.TimeSpan.HasValue)
                {
                    this.Status = new StatusViewModel(
                        string.Format(
                            CultureInfo.CurrentUICulture,
                            Localized.OfflineDiskProgressCompleteFormatWithStats,
                            diskName,
                            result.FolderCount,
                            result.FileCount,
                            result.ArchiveItemCount,
                            result.TimeSpan.Value.TotalSeconds,
                            this.dateTimeProviderService.Now),
                        StatusSeverity.Success);
                }
                else
                {
                    this.Status = new StatusViewModel(
                        string.Format(
                            CultureInfo.CurrentUICulture,
                            Localized.OfflineDiskProgressCompleteFormat,
                            diskName,
                            this.dateTimeProviderService.Now),
                        StatusSeverity.Success);
                }

                await this.operationCompletionService.NotifyCompletionWithOpenInExplorer(
                    OperationCompletionResult.Success,
                    Localized.OfflineDiskSuccessNotification,
                    targetDiskFilePath,
                    CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                this.Status = new StatusViewModel(
                    string.Format(CultureInfo.CurrentUICulture, Localized.OfflineDiskProgressCanceledFormat, diskName),
                    StatusSeverity.Error);

                await this.operationCompletionService.NotifyCompletion(
                    OperationCompletionResult.Cancelled,
                    Localized.OfflineDiskCancelNotification,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                this.IsIndexingError = true;
                this.Status = new StatusViewModel(
                    string.Format(CultureInfo.CurrentUICulture, Localized.OfflineDiskProgressErrorFormat, ex.Message),
                    StatusSeverity.Error);

                await this.operationCompletionService.NotifyCompletion(
                    OperationCompletionResult.Error,
                    Localized.OfflineDiskErrorNotification,
                    CancellationToken.None);
            }
            finally
            {
                this.IsIndexing = false;
                this.cancellationTokenSource?.Dispose();
                this.cancellationTokenSource = null;
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private bool CanCancel()
    {
        return this.IsIndexing &&
            this.cancellationTokenSource is not null &&
            !this.cancellationTokenSource.IsCancellationRequested;
    }

    private bool CanCreateDisk()
    {
        return this.SelectedDrive is not null &&
            Directory.Exists(this.SelectedDrive.FullPath) &&
            !string.IsNullOrEmpty(this.TargetFolder) &&
            !string.IsNullOrEmpty(this.DiskName) &&
            !this.IsIndexing;
    }

    private async Task LoadSettingsAsync(CancellationToken ct)
    {
        var folders = await this.offlineExplorerService.GetFoldersAsync(ct);
        if (folders is not null)
        {
            this.TargetFolder = folders.FirstOrDefault() ?? string.Empty;

            this.TargetFolders.Clear();
            foreach (var folder in folders)
            {
                this.TargetFolders.Add(folder);
            }
        }
    }

    private void OfflineExplorerService_DiskCreateProgress(object? sender, OfflineDiskCreateProgressEventArgs args)
    {
        if (args.Status != OfflineDiskCreateStatus.Scanning)
        {
            return;
        }

        this.dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                this.Status = new StatusViewModel(
                        string.Format(
                            CultureInfo.CurrentUICulture,
                            Localized.OfflineDiskProgressCreatingFormatWithStats,
                            args.DiskName,
                            args.FolderCount,
                            args.FileCount,
                            args.ArchiveItemCount,
                            args.TimeSpan.HasValue ? args.TimeSpan.Value.TotalSeconds : 0),
                        StatusSeverity.Info);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error in OfflineExplorerService_DiskCreateProgress");
            }
        });
    }

    private string GetTargetDiskFilePath(string diskName)
    {
        return Path.Combine(this.TargetFolder, diskName + OfflineDisk.DefaultFileExtension);
    }

    private void ResetStatus()
    {
        var diskPath = this.GetTargetDiskFilePath(this.DiskName.Trim());
        if (File.Exists(diskPath))
        {
            this.Status = new StatusViewModel(Localized.OfflineDiskProgressFileExists, StatusSeverity.Error);
        }
        else
        {
            this.Status = new StatusViewModel();
        }
    }

    partial void OnDiskNameChanged(string value)
    {
        this.ResetStatus();
    }

    partial void OnSelectedDriveChanged(OfflineExplorerDriveViewModel? value)
    {
        this.ResetStatus();
    }

    partial void OnTargetFolderChanged(string value)
    {
        this.ResetStatus();
    }
}
