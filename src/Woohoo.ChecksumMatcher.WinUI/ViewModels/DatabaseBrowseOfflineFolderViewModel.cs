// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public partial class DatabaseBrowseOfflineFolderViewModel : ObservableObject
{
    private readonly IOfflineExplorerService offlineExplorerService;
    private readonly Func<OfflineDisk, string, CancellationToken, Task> applySelectionCallback;

    public DatabaseBrowseOfflineFolderViewModel(
        IOfflineExplorerService offlineExplorerService,
        Func<OfflineDisk, string, CancellationToken, Task> applySelectionCallback)
    {
        ArgumentNullException.ThrowIfNull(offlineExplorerService);
        ArgumentNullException.ThrowIfNull(applySelectionCallback);

        this.offlineExplorerService = offlineExplorerService;
        this.applySelectionCallback = applySelectionCallback;
    }

    [ObservableProperty]
    public partial ObservableCollection<DatabaseSelectOfflineDiskItemViewModel> Disks { get; set; } = [];

    [ObservableProperty]
    public partial DatabaseSelectOfflineDiskItemViewModel? SelectedDisk { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsLoaded { get; set; }

    public async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (this.IsLoading || this.IsLoaded)
        {
            return;
        }

        await this.LoadDisksAsync(ct);
    }

    [RelayCommand]
    public async Task ApplySelection(CancellationToken ct)
    {
        var disk = this.SelectedDisk?.Disk;
        if (disk is not null)
        {
            var folder = this.SelectedDisk?.SelectedFolder?.FolderPath;
            if (!string.IsNullOrEmpty(folder) && this.applySelectionCallback is not null)
            {
                await this.applySelectionCallback.Invoke(disk, folder, ct);
            }
        }
    }

    private async Task LoadDisksAsync(CancellationToken ct)
    {
        this.IsLoading = true;

        var repository = await this.offlineExplorerService.GetOfflineRepositoryAsync(CancellationToken.None);

        foreach (var diskFile in repository.Disks.OrderBy(d => d.Header.Name))
        {
            var viewModel = new DatabaseSelectOfflineDiskItemViewModel(diskFile);
            this.Disks.Add(viewModel);
        }

        this.SelectedDisk = this.Disks.FirstOrDefault();

        this.IsLoaded = true;
        this.IsLoading = false;
    }

    partial void OnSelectedDiskChanged(DatabaseSelectOfflineDiskItemViewModel? value)
    {
        if (value is null)
        {
            return;
        }

        if (!value.IsFullyLoaded)
        {
            this.FullyLoadDiskAsync(value, CancellationToken.None).SafeFireAndForget();
        }
    }

    private async Task FullyLoadDiskAsync(DatabaseSelectOfflineDiskItemViewModel diskViewModel, CancellationToken ct)
    {
        diskViewModel.IsFullyLoading = true;

        var offlineDisk = await this.offlineExplorerService.GetDiskAsync(diskViewModel.OfflineDiskFile, ct);
        if (offlineDisk is not null)
        {
            var folderViewModels = offlineDisk
                .GetAllFolders()
                .OrderBy(f => f.Path)
                .Select(f => new DatabaseSelectOfflineFolderItemViewModel(f.Path))
                .ToList();

            diskViewModel.Disk = offlineDisk;
            diskViewModel.Folders.Clear();
            foreach (var folder in folderViewModels)
            {
                diskViewModel.Folders.Add(folder);
            }
        }

        diskViewModel.IsFullyLoaded = true;
        diskViewModel.IsFullyLoading = false;
    }
}
