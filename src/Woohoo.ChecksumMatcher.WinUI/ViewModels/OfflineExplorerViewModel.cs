// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.ChecksumMatcher.WinUI.Helpers;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public sealed partial class OfflineExplorerViewModel : ObservableRecipient, INavigationAware, IDisposable
{
    private readonly DisposableBag disposables = DisposableBag.Create<DatabaseLibraryViewModel>();

    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly IClipboardService clipboardService;
    private readonly INavigationService navigationService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly IDispatcherQueueTimer repositoryChangeDebounceTimer;

    public OfflineExplorerViewModel(
        IOfflineExplorerService offlineExplorerService,
        ILocalSettingsService localSettingsService,
        IClipboardService clipboardService,
        IDispatcherQueueService dispatcherQueueService,
        INavigationService navigationService,
        ILogger<OfflineExplorerViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(localSettingsService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(logger);

        this.offlineExplorerService = offlineExplorerService;
        this.localSettingsService = localSettingsService;
        this.clipboardService = clipboardService;
        this.navigationService = navigationService;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();
        this.repositoryChangeDebounceTimer = this.dispatcherQueue.CreateTimer();

        this.IsLoading = true;
        this.LoadDisksAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error loading disks."));

        this.offlineExplorerService.RepositoryChanged += this.OfflineStorage_RepositoryChanged;
        this.disposables.Add(() => this.offlineExplorerService.RepositoryChanged -= this.OfflineStorage_RepositoryChanged);
    }

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial bool IsConfigured { get; set; } = false;

    [ObservableProperty]
    public partial OfflineExplorerDiskViewModel? SelectedDisk { get; set; }

    public ObservableCollection<OfflineExplorerDiskViewModel> Disks { get; } = [];

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }

    private void OfflineStorage_RepositoryChanged(object? sender, EventArgs e)
    {
        this.repositoryChangeDebounceTimer.Debounce(DoReload, TimeSpan.FromMilliseconds(2000), immediate: false);

        void DoReload()
        {
            try
            {
                this.IsLoading = true;
                this.LoadDisksAsync(CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error loading disks."));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to reload offline disks after repository change.");
            }
        }
    }

    [RelayCommand]
    private void CreateNewDisk()
    {
        try
        {
            this.navigationService.NavigateTo(typeof(OfflineExplorerCreateDiskViewModel).FullName!);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        try
        {
            this.navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    partial void OnSelectedDiskChanged(OfflineExplorerDiskViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        if (value.IsFullyLoaded)
        {
            if (value.SelectedFolder is null)
            {
                // If no folder is selected, select the first root folder.
                value.SelectedFolder = value.RootFolders.FirstOrDefault();
            }
        }
        else
        {
            value.SelectedFolder = null;
            this.FullyLoadDiskAsync(value, CancellationToken.None).FireAndForget((ex) => this.logger.LogError(ex, "Error loading disk."));
        }
    }

    private async Task LoadDisksAsync(CancellationToken ct)
    {
        var repo = await this.offlineExplorerService.GetOfflineRepositoryAsync(ct);

        this.IsConfigured = repo.IsConfigured;

        this.Disks.Clear();
        foreach (var diskFile in repo.Disks.OrderBy(d => d.Header.Name))
        {
            this.Disks.Add(new OfflineExplorerDiskViewModel(diskFile));
        }

        this.SelectedDisk = this.Disks.FirstOrDefault();
        if (this.SelectedDisk is not null)
        {
            this.SelectedDisk.SelectedFolder = this.SelectedDisk.RootFolders.FirstOrDefault();
            if (this.SelectedDisk.SelectedFolder is not null)
            {
                this.SelectedDisk.SelectedFolder.SelectedFile = this.SelectedDisk.SelectedFolder.SortedFoldersAndFiles.FirstOrDefault();
            }
        }

        this.IsLoading = false;
    }

    private async Task FullyLoadDiskAsync(OfflineExplorerDiskViewModel diskViewModel, CancellationToken ct)
    {
        Debug.Assert(!diskViewModel.IsFullyLoaded, "Disk should not be already fully loaded.");

        diskViewModel.IsFullyLoading = true;

        var result = await this.offlineExplorerService.GetDiskAsync(diskViewModel.OfflineDiskFile, ct);
        if (result is not null)
        {
            var rootFolders = await Task.Run(() => LoadRootFolders(this.localSettingsService, this.clipboardService, this.dispatcherQueue, this.logger, this, result, diskViewModel), ct);

            diskViewModel.RootFolders.Clear();
            foreach (var rootFolder in rootFolders)
            {
                diskViewModel.RootFolders.Add(rootFolder);
            }

            diskViewModel.SelectedFolder = diskViewModel.RootFolders.FirstOrDefault();
        }

        diskViewModel.IsFullyLoaded = true;
        diskViewModel.IsFullyLoading = false;

        static List<OfflineExplorerFolderViewModel> LoadRootFolders(
            ILocalSettingsService localSettingsService,
            IClipboardService clipboardService,
            IDispatcherQueue dispatcherQueue,
            ILogger logger,
            OfflineExplorerViewModel explorerViewModel,
            OfflineDisk disk,
            OfflineExplorerDiskViewModel diskViewModel)
        {
            List<OfflineExplorerFolderViewModel> rootFolders = [];

            foreach (var rootFolder in disk.RootFolders ?? [])
            {
                var itemViewModel = new OfflineExplorerFolderViewModel(
                    rootFolder,
                    explorerViewModel,
                    null,
                    localSettingsService,
                    clipboardService,
                    dispatcherQueue,
                    logger);

                rootFolders.Add(itemViewModel);

                LoadChildren(
                    explorerViewModel,
                    itemViewModel,
                    rootFolder,
                    localSettingsService,
                    clipboardService,
                    dispatcherQueue,
                    logger);
            }

            return rootFolders;
        }

        static void LoadChildren(
            OfflineExplorerViewModel explorerViewModel,
            OfflineExplorerFolderViewModel parentViewModel,
            OfflineItem parentItem,
            ILocalSettingsService localSettingsService,
            IClipboardService clipboardService,
            IDispatcherQueue dispatcherQueue,
            ILogger logger)
        {
            foreach (var childItem in parentItem.Items.Where(f => !f.IsSystem))
            {
                if (childItem.Kind == OfflineItemKind.File)
                {
                    var fileViewModel = new OfflineExplorerFileViewModel(
                        childItem,
                        parentViewModel,
                        clipboardService);

                    parentViewModel.Files.Add(fileViewModel);
                    parentViewModel.FoldersAndFiles.Add(fileViewModel);
                }
                else
                {
                    var folderViewModel = new OfflineExplorerFolderViewModel(
                        childItem,
                        explorerViewModel,
                        parentViewModel,
                        localSettingsService,
                        clipboardService,
                        dispatcherQueue,
                        logger);

                    parentViewModel.Folders.Add(folderViewModel);
                    parentViewModel.FoldersAndFiles.Add(folderViewModel);

                    LoadChildren(
                        explorerViewModel,
                        folderViewModel,
                        childItem,
                        localSettingsService,
                        clipboardService,
                        dispatcherQueue,
                        logger);
                }
            }
        }
    }
}
