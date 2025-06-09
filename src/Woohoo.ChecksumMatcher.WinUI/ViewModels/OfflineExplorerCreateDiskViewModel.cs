// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Models;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public partial class OfflineExplorerCreateDiskViewModel : ObservableObject
{
    private const double ProgressThrottleInSecs = 1.0;

    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly IFilePickerService filePickerService;
    private readonly IOperationCompletionService operationCompletionService;
    private readonly IDateTimeProviderService dateTimeProviderService;
    private readonly IDispatcherQueue dispatcherQueue;

    private CancellationTokenSource? cancellationTokenSource;
    private OfflineDiskCreateProgressEventArgs? lastDiskCreateProgressEventArgs;
    private OfflineDiskCreateProgressEventArgs? lastShownDiskCreateProgressEventArgs;

    public OfflineExplorerCreateDiskViewModel(
        IOfflineExplorerService offlineExplorerService,
        IFilePickerService filePickerService,
        IOperationCompletionService operationCompletionService,
        IDateTimeProviderService dateTimeProviderService,
        IDispatcherQueueService dispatcherQueueService)
    {
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(filePickerService);
        ArgumentNullException.ThrowIfNull(operationCompletionService);
        ArgumentNullException.ThrowIfNull(dateTimeProviderService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);

        this.offlineExplorerService = offlineExplorerService;
        this.filePickerService = filePickerService;
        this.operationCompletionService = operationCompletionService;
        this.dateTimeProviderService = dateTimeProviderService;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.offlineExplorerService.DiskCreateProgress += this.OfflineExplorerService_DiskCreateProgress;

        this.Drives = new(DriveInfo.GetDrives().Select(info => new OfflineExplorerDriveViewModel(info, this.dispatcherQueue)));
        this.SelectedDrive = this.Drives.FirstOrDefault(d => d.IsReady) ?? this.Drives.FirstOrDefault();

        this.LoadSettingsAsync(CancellationToken.None).SafeFireAndForget();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    public partial string TargetFolder { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    public partial string DiskName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StatusViewModel Status { get; set; } = new StatusViewModel(string.Empty, StatusSeverity.None);

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

    [RelayCommand]
    private async Task BrowseTargetFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync(FilePickerSettingIdentifiers.OfflineStorageTargetFolder);
        if (!string.IsNullOrEmpty(folderPath))
        {
            this.TargetFolder = folderPath;
        }
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        this.Drives.Clear();
        foreach (var drive in DriveInfo.GetDrives())
        {
            this.Drives.Add(new OfflineExplorerDriveViewModel(drive, this.dispatcherQueue));
        }

        // Select the first ready drive or the first drive.
        this.SelectedDrive = this.Drives.FirstOrDefault(d => d.IsReady) ?? this.Drives.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        this.cancellationTokenSource?.Cancel();
        this.CancelCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCreateDisk))]
    private async Task CreateDiskAsync()
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

            await this.offlineExplorerService.CreateDiskAsync(
                sourceFolderPath,
                targetDiskFilePath,
                diskName,
                settings,
                this.cancellationTokenSource.Token);

            var last = this.lastDiskCreateProgressEventArgs;
            this.lastDiskCreateProgressEventArgs = null;
            if (last is not null)
            {
                this.Status = new StatusViewModel(
                    string.Format(
                        CultureInfo.CurrentUICulture,
                        Localized.OfflineDiskProgressCompleteFormatWithStats,
                        diskName,
                        last.FolderCount,
                        last.FileCount,
                        last.ArchiveItemCount,
                        last.TimeSpan.TotalSeconds,
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
            this.lastShownDiskCreateProgressEventArgs = null;
            this.IsIndexing = false;
            this.cancellationTokenSource?.Dispose();
            this.cancellationTokenSource = null;
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
        }
    }

    private void OfflineExplorerService_DiskCreateProgress(object? sender, OfflineDiskCreateProgressEventArgs args)
    {
        this.lastDiskCreateProgressEventArgs = args;

        bool throttled = args.TimeSpan.TotalSeconds - this.lastShownDiskCreateProgressEventArgs?.TimeSpan.TotalSeconds < ProgressThrottleInSecs;
        if (!throttled)
        {
            this.lastShownDiskCreateProgressEventArgs = args;
            this.dispatcherQueue.TryEnqueue(() =>
            {
                this.Status = new StatusViewModel(
                    string.Format(
                        CultureInfo.CurrentUICulture,
                        Localized.OfflineDiskProgressCreatingFormatWithStats,
                        args.DiskName,
                        args.FolderCount,
                        args.FileCount,
                        args.ArchiveItemCount,
                        args.TimeSpan.TotalSeconds),
                    StatusSeverity.Info);
            });
        }
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
            this.Status = new StatusViewModel(string.Empty, StatusSeverity.None);
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
