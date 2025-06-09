// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public partial class OfflineExplorerViewModel : ObservableRecipient, INavigationAware
{
    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly IClipboardService clipboardService;
    private readonly INavigationService navigationService;
    private readonly IDispatcherQueue dispatcherQueue;

    public OfflineExplorerViewModel(
        IOfflineExplorerService offlineExplorerService,
        ILocalSettingsService localSettingsService,
        IClipboardService clipboardService,
        IDispatcherQueueService dispatcherQueueService,
        INavigationService navigationService)
    {
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(localSettingsService);
        ArgumentNullException.ThrowIfNull(clipboardService);
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(navigationService);

        this.offlineExplorerService = offlineExplorerService;
        this.localSettingsService = localSettingsService;
        this.clipboardService = clipboardService;
        this.navigationService = navigationService;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.IsLoading = true;
        this.LoadDisksAsync(CancellationToken.None).SafeFireAndForget();
    }

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial OfflineExplorerDiskViewModel? SelectedDisk { get; set; }

    [ObservableProperty]
    public partial OfflineExplorerFolderViewModel? SelectedFolder { get; set; }

    [ObservableProperty]
    public partial object? SelectedFile { get; set; }

    [ObservableProperty]
    public partial bool SelectedFileDetailsAvailable { get; set; }

    public ObservableCollection<OfflineExplorerDiskViewModel> Disks { get; } = [];

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private void CreateNewDisk()
    {
        this.navigationService.NavigateTo(typeof(OfflineExplorerCreateDiskViewModel).FullName!);
    }

    partial void OnSelectedDiskChanged(OfflineExplorerDiskViewModel? value)
    {
        if (value is null)
        {
            this.SelectedFolder = null;
            this.SelectedFile = null;
            return;
        }

        if (value.IsFullyLoaded)
        {
            this.SelectedFolder = value?.RootFolders.FirstOrDefault();
            this.SelectedFile = this.SelectedFolder?.SortedFoldersAndFiles.FirstOrDefault();
        }
        else
        {
            this.SelectedFolder = null;
            this.SelectedFile = null;
            this.FullyLoadDiskAsync(value, CancellationToken.None).SafeFireAndForget();
        }
    }

    partial void OnSelectedFolderChanged(OfflineExplorerFolderViewModel? value)
    {
        this.SelectedFile = value?.Files.FirstOrDefault();
    }

    partial void OnSelectedFileChanged(object? value)
    {
        this.SelectedFileDetailsAvailable = value is not null;
    }

    private async Task LoadDisksAsync(CancellationToken ct)
    {
        var repo = await this.offlineExplorerService.GetOfflineRepositoryAsync(ct);

        this.Disks.Clear();
        foreach (var diskFile in repo.Disks.OrderBy(d => d.Header.Name))
        {
            this.Disks.Add(new OfflineExplorerDiskViewModel(diskFile));
        }

        this.SelectedDisk = this.Disks.FirstOrDefault();
        this.SelectedFolder = this.SelectedDisk?.RootFolders.FirstOrDefault();
        this.SelectedFile = this.SelectedFolder?.FoldersAndFiles.FirstOrDefault();

        this.IsLoading = false;
    }

    private async Task FullyLoadDiskAsync(OfflineExplorerDiskViewModel diskViewModel, CancellationToken ct)
    {
        Debug.Assert(!diskViewModel.IsFullyLoaded, "Disk should not be already fully loaded.");

        diskViewModel.IsFullyLoading = true;

        var result = await this.offlineExplorerService.GetDiskAsync(diskViewModel.OfflineDiskFile, ct);
        if (result is not null)
        {
            var rootFolders = await Task.Run(() => LoadRootFolders(this.localSettingsService, this.clipboardService, this.dispatcherQueue, this, result, diskViewModel), ct);

            diskViewModel.RootFolders.Clear();
            foreach (var rootFolder in rootFolders)
            {
                diskViewModel.RootFolders.Add(rootFolder);
            }

            this.SelectedFolder = diskViewModel.RootFolders.FirstOrDefault();
            this.SelectedFile = this.SelectedFolder?.SortedFoldersAndFiles.FirstOrDefault();
        }

        diskViewModel.IsFullyLoaded = true;
        diskViewModel.IsFullyLoading = false;

        static List<OfflineExplorerFolderViewModel> LoadRootFolders(
            ILocalSettingsService localSettingsService,
            IClipboardService clipboardService,
            IDispatcherQueue dispatcherQueue,
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
                    dispatcherQueue);

                rootFolders.Add(itemViewModel);

                LoadChildren(
                    explorerViewModel,
                    itemViewModel,
                    rootFolder,
                    localSettingsService,
                    clipboardService,
                    dispatcherQueue);
            }

            return rootFolders;
        }

        static void LoadChildren(
            OfflineExplorerViewModel explorerViewModel,
            OfflineExplorerFolderViewModel parentViewModel,
            OfflineItem parentItem,
            ILocalSettingsService localSettingsService,
            IClipboardService clipboardService,
            IDispatcherQueue dispatcherQueue)
        {
            foreach (var childItem in parentItem.Items)
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
                        dispatcherQueue);

                    parentViewModel.Folders.Add(folderViewModel);
                    parentViewModel.FoldersAndFiles.Add(folderViewModel);

                    LoadChildren(
                        explorerViewModel,
                        folderViewModel,
                        childItem,
                        localSettingsService,
                        clipboardService,
                        dispatcherQueue);
                }
            }
        }
    }
}
