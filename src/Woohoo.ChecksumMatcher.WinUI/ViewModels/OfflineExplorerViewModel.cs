// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Woohoo.ChecksumMatcher.WinUI.Contracts.Services;
using Woohoo.ChecksumMatcher.WinUI.Contracts.ViewModels;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public partial class OfflineExplorerViewModel : ObservableRecipient, INavigationAware
{
    private readonly ILocalSettingsService localSettingsService;
    private readonly IOfflineDiskFinderService offlineDiskFinderService;
    private readonly IClipboardService clipboardService;
    private readonly IFilePickerService filePickerService;
    private readonly DispatcherQueue dispatcherQueue;

    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private OfflineExplorerDiskViewModel? selectedDisk;

    [ObservableProperty]
    private OfflineExplorerFolderViewModel? selectedFolder;

    [ObservableProperty]
    private object? selectedFile;

    [ObservableProperty]
    private bool selectedFileDetailsAvailable;

    public OfflineExplorerViewModel(ILocalSettingsService localSettingsService, IOfflineDiskFinderService offlineDiskFinderService, IClipboardService clipboardService, IFilePickerService filePickerService, IDispatcherQueueService dispatcherQueueService)
    {
        this.localSettingsService = localSettingsService;
        this.offlineDiskFinderService = offlineDiskFinderService;
        this.clipboardService = clipboardService;
        this.filePickerService = filePickerService;
        this.dispatcherQueue = dispatcherQueueService.GetUIDispatcher();

        this.IsLoading = true;
        this.LoadDisksAsync().SafeFireAndForget();
    }

    public ObservableCollection<OfflineExplorerDiskViewModel> Indexes { get; } = [];

    public void OnNavigatedTo(object parameter)
    {
        //this.SampleItems.Clear();

        //// TODO: Replace with real data.
        //var data = await _sampleDataService.GetListDetailsDataAsync();

        //foreach (var item in data)
        //{
        //    this.SampleItems.Add(item);
        //}
    }

    public void OnNavigatedFrom()
    {
    }

    public void EnsureItemSelected()
    {
        //this.Selected ??= this.SampleItems.First();
    }

    partial void OnSelectedDiskChanged(OfflineExplorerDiskViewModel? value)
    {
        this.SelectedFolder = value?.RootFolders.FirstOrDefault();
        this.SelectedFile = this.SelectedFolder?.Files.FirstOrDefault();
    }

    partial void OnSelectedFolderChanged(OfflineExplorerFolderViewModel? value)
    {
        this.SelectedFile = value?.Files.FirstOrDefault();
    }

    partial void OnSelectedFileChanged(object? value)
    {
        this.SelectedFileDetailsAvailable = value is not null;
    }

    private async Task LoadDisksAsync()
    {
        // Create the disk indexes on a worker thread.
        var indexes = await Task.Run(() => LoadDisksWorkerThread(this.offlineDiskFinderService, this.localSettingsService, this.clipboardService, this.dispatcherQueue, this).ToArray());

        // Add the disk indexes to the collection on the UI thread.
        foreach (var indexViewModel in indexes)
        {
            this.Indexes.Add(indexViewModel);
        }

        this.SelectedDisk = this.Indexes.FirstOrDefault();
        this.SelectedFolder = this.SelectedDisk?.RootFolders.FirstOrDefault();
        this.SelectedFile = this.SelectedFolder?.FoldersAndFiles.FirstOrDefault();

        this.IsLoading = false;
    }

    private static IEnumerable<OfflineExplorerDiskViewModel> LoadDisksWorkerThread(IOfflineDiskFinderService offlineDiskFinderService, ILocalSettingsService localSettingsService, IClipboardService clipboardService,  DispatcherQueue dispatcherQueue, OfflineExplorerViewModel offlineExplorerViewModel)
    {
        foreach (var indexFilePath in EnumerateIndexFilePaths(offlineDiskFinderService))
        {
            var result = offlineDiskFinderService.Load(indexFilePath);
            if (result.Disk is null)
            {
                continue;
            }

            var indexViewModel = new OfflineExplorerDiskViewModel(result.Disk);
            foreach (var rootFolder in result.Disk.RootFolders)
            {
                var itemViewModel = new OfflineExplorerFolderViewModel(rootFolder, offlineExplorerViewModel, null, localSettingsService, clipboardService, dispatcherQueue);
                indexViewModel.RootFolders.Add(itemViewModel);

                LoadChildrenRecursiveWorkerThread(offlineExplorerViewModel, itemViewModel, rootFolder, localSettingsService, clipboardService, dispatcherQueue);
            }

            yield return indexViewModel;
        }
    }

    private static IEnumerable<string> EnumerateIndexFilePaths(IOfflineDiskFinderService offlineDiskFinderService)
    {
        foreach (var result in offlineDiskFinderService.FindOfflineDisks())
        {
            yield return result.FilePath;
        }
    }

    private static void LoadChildrenRecursiveWorkerThread(OfflineExplorerViewModel offlineExplorerViewModel, OfflineExplorerFolderViewModel parentViewModel, OfflineItem parentItem, ILocalSettingsService localSettingsService, IClipboardService clipboardService, DispatcherQueue dispatcherQueue)
    {
        foreach (var childItem in parentItem.Items)
        {
            if (childItem.Kind == OfflineItemKind.File)
            {
                var fileViewModel = new OfflineExplorerFileViewModel(childItem, parentViewModel, clipboardService);
                parentViewModel.Files.Add(fileViewModel);
                parentViewModel.FoldersAndFiles.Add(fileViewModel);
            }
            else
            {
                var folderViewModel = new OfflineExplorerFolderViewModel(childItem, offlineExplorerViewModel, parentViewModel, localSettingsService, clipboardService, dispatcherQueue);
                parentViewModel.Folders.Add(folderViewModel);
                parentViewModel.FoldersAndFiles.Add(folderViewModel);
                LoadChildrenRecursiveWorkerThread(offlineExplorerViewModel, folderViewModel, childItem, localSettingsService, clipboardService, dispatcherQueue);
            }
        }
    }
}
