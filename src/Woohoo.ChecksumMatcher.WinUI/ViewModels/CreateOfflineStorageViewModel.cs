// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.System;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.IO.AbstractFileSystem.Offline.Scanning;

public partial class CreateOfflineStorageViewModel : ObservableObject
{
    private const double ProgressThrottleInSecs = 1.0;

    private readonly IFilePickerService filePickerService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly DispatcherQueue dispatcherQueue;

    private readonly Indexer indexer;
    private CancellationTokenSource? cancellationTokenSource;
    private IndexerProgressEventArgs? lastProgressEventArgs;
    private IndexerProgressEventArgs? lastShownProgressEventArgs;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    private string sourceFolder = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    private string targetFolder = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    private string diskName = string.Empty;

    [ObservableProperty]
    private string status = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateDiskCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool isIndexing;

    [ObservableProperty]
    private bool isIndexingError;

    public CreateOfflineStorageViewModel(IFilePickerService filePickerService, ILocalSettingsService localSettingsService, IDispatcherQueueService dispatcherQueueService)
    {
        this.filePickerService = filePickerService;
        this.localSettingsService = localSettingsService;
        this.dispatcherQueue = dispatcherQueueService.GetWindowsDispatcher();
        this.indexer = new Indexer(new IndexerOptions { CalculateChecksums = false, IndexArchiveContent = true });
        this.indexer.ProgressChanged += (sender, e) =>
        {
            this.lastProgressEventArgs = e;

            bool throttled = e.TimeSpan.TotalSeconds - this.lastShownProgressEventArgs?.TimeSpan.TotalSeconds < ProgressThrottleInSecs;
            if (!throttled)
            {
                this.lastShownProgressEventArgs = e;
                this.dispatcherQueue.TryEnqueue(() =>
                {
                    this.Status = $"Indexing disk... ({e.FolderCount} folders, {e.FileCount} files, {e.ArchiveItemCount} archive items) {e.TimeSpan.TotalSeconds:0.0} secs";
                });
            }
        };

        var offlineFolders = this.localSettingsService.ReadSetting<string[]>(SettingKeys.OfflineFolders);
        if (offlineFolders is not null)
        {
            this.TargetFolder = offlineFolders.FirstOrDefault() ?? string.Empty;
        }
    }

    [RelayCommand]
    public async Task BrowseSourceFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync();
        if (!string.IsNullOrEmpty(folderPath))
        {
            this.SourceFolder = folderPath;
        }
    }

    [RelayCommand]
    public async Task BrowseTargetFolderAsync()
    {
        var folderPath = await this.filePickerService.GetOpenFolderPathAsync();
        if (!string.IsNullOrEmpty(folderPath))
        {
            this.TargetFolder = folderPath;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    public void Cancel()
    {
        this.cancellationTokenSource?.Cancel();
    }

    [RelayCommand(CanExecute = nameof(CanCreateDisk))]
    public void CreateDisk()
    {
        if (!Directory.Exists(this.SourceFolder))
        {
            return;
        }

        this.cancellationTokenSource = new CancellationTokenSource();
        this.IsIndexing = true;
        this.IsIndexingError = false;
        this.CancelCommand.NotifyCanExecuteChanged();

        this.Status = "Indexing disk...";

        _ = Task.Run(() =>
        {
            string endStatus = string.Empty;
            bool error = false;
            try
            {
                var index = this.indexer.ScanDisk(this.SourceFolder, this.DiskName, this.cancellationTokenSource.Token);
                index.Serialize(Path.Combine(this.TargetFolder, this.DiskName + ".zip"));

                if (this.lastProgressEventArgs is not null)
                {
                    endStatus = $"Indexing complete ({this.lastProgressEventArgs.FolderCount} folders, {this.lastProgressEventArgs.FileCount} files, {this.lastProgressEventArgs.ArchiveItemCount} archive items) {this.lastProgressEventArgs.TimeSpan.TotalSeconds:0.0} secs";
                    this.lastProgressEventArgs = null;
                }
                else
                {
                    endStatus = "Indexing complete.";
                }
            }
            catch (OperationCanceledException)
            {
                endStatus = "Indexing canceled.";
            }
            catch (Exception ex)
            {
                error = true;
                endStatus = $"Indexing error: {ex.Message}";
            }
            finally
            {
                this.lastShownProgressEventArgs = null;
                this.dispatcherQueue.TryEnqueue(() =>
                {
                    this.IsIndexing = false;
                    this.IsIndexingError = error;
                    this.Status = endStatus;
                    this.cancellationTokenSource?.Dispose();
                    this.cancellationTokenSource = null;
                });
            }
        });
    }

    public bool CanCancel()
    {
        return this.IsIndexing && this.cancellationTokenSource is not null && !this.cancellationTokenSource.IsCancellationRequested;
    }

    public bool CanCreateDisk()
    {
        return !string.IsNullOrEmpty(this.SourceFolder) && !string.IsNullOrEmpty(this.TargetFolder) && !string.IsNullOrEmpty(this.DiskName) && !this.IsIndexing;
    }
}
