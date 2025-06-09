// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.System;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

public partial class BrowseOfflineFolderViewModel : ObservableObject
{
    private readonly IOfflineDiskFinderService offlineDiskFinderService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly DispatcherQueue dispatcherQueue;
    private readonly Action<OfflineDisk, string> applySelectionCallback;

    [ObservableProperty]
    private ObservableCollection<SelectOfflineDiskItemViewModel> disks = [];

    [ObservableProperty]
    private SelectOfflineDiskItemViewModel? selectedDisk;

    [ObservableProperty]
    private bool isLoading = true;

    public BrowseOfflineFolderViewModel(IOfflineDiskFinderService offlineDiskFinderService, ILocalSettingsService localSettingsService, DispatcherQueue dispatcherQueue, Action<OfflineDisk, string> applySelectionCallback)
    {
        this.offlineDiskFinderService = offlineDiskFinderService;
        this.localSettingsService = localSettingsService;
        this.dispatcherQueue = dispatcherQueue;
        this.applySelectionCallback = applySelectionCallback;

        this.IsLoading = true;
        this.LoadDisksAsync().SafeFireAndForget();
    }

    [RelayCommand]
    public void ApplySelection()
    {
        var disk = this.SelectedDisk?.Disk;
        if (disk is not null)
        {
            var folder = this.SelectedDisk?.SelectedFolder?.FolderPath;
            if (!string.IsNullOrEmpty(folder))
            {
                this.applySelectionCallback?.Invoke(disk, folder);
            }
        }
    }

    private async Task LoadDisksAsync()
    {
        // Create the disk indexes on a worker thread.
        var indexes = await Task.Run(() => LoadDisksWorkerThread(this.offlineDiskFinderService, this.dispatcherQueue).ToArray());

        // Add the disk indexes to the collection on the UI thread.
        foreach (var indexViewModel in indexes)
        {
            this.Disks.Add(indexViewModel);
        }

        this.SelectedDisk = this.Disks.FirstOrDefault();

        this.IsLoading = false;
    }

    private static IEnumerable<SelectOfflineDiskItemViewModel> LoadDisksWorkerThread(IOfflineDiskFinderService offlineDiskFinderService, DispatcherQueue dispatcherQueue)
    {
        foreach (var indexFilePath in EnumerateDiskFilePaths(offlineDiskFinderService))
        {
            var result = offlineDiskFinderService.Load(indexFilePath);
            if (result.Disk is null)
            {
                continue;
            }

            var indexViewModel = new SelectOfflineDiskItemViewModel(result.Disk);
            yield return indexViewModel;
        }
    }

    private static IEnumerable<string> EnumerateDiskFilePaths(IOfflineDiskFinderService offlineDiskFinderService)
    {
        foreach (var result in offlineDiskFinderService.FindOfflineDisks())
        {
            yield return result.FilePath;
        }
    }
}
