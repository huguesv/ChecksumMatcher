// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;

public sealed partial class OfflineStorageFolderSelectionViewModel : ObservableObject
{
    private readonly IOfflineExplorerService offlineExplorerService;

    public OfflineStorageFolderSelectionViewModel(IOfflineExplorerService offlineExplorerService)
    {
        ArgumentNullException.ThrowIfNull(offlineExplorerService);

        this.offlineExplorerService = offlineExplorerService;
    }

    [ObservableProperty]
    public partial ObservableCollection<OfflineStorageFolderSelectionDiskItemViewModel> Disks { get; set; } = [];

    [ObservableProperty]
    public partial OfflineStorageFolderSelectionDiskItemViewModel? SelectedDisk { get; set; }

    [ObservableProperty]
    public partial OfflineStorageFolderSelectionFolderItemViewModel? SelectedFolder { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsLoaded { get; set; }

    [ObservableProperty]
    public partial bool IsSelectionValid { get; set; }

    public async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (this.IsLoading || this.IsLoaded)
        {
            return;
        }

        await this.LoadDisksAsync(ct);
    }

    private async Task LoadDisksAsync(CancellationToken ct)
    {
        this.IsLoading = true;

        var repository = await this.offlineExplorerService.GetOfflineRepositoryAsync(CancellationToken.None);

        foreach (var diskFile in repository.Disks.OrderBy(d => d.Header.Name))
        {
            var viewModel = new OfflineStorageFolderSelectionDiskItemViewModel(diskFile, this.UpdateIsValidSelection);
            this.Disks.Add(viewModel);
        }

        this.SelectedDisk = this.Disks.FirstOrDefault();

        this.IsLoaded = true;
        this.IsLoading = false;
    }

    private void UpdateIsValidSelection()
    {
        this.IsSelectionValid = this.SelectedDisk is not null && this.SelectedDisk.SelectedFolder is not null;
    }

    partial void OnSelectedDiskChanged(OfflineStorageFolderSelectionDiskItemViewModel? value)
    {
        this.UpdateIsValidSelection();

        if (value is null)
        {
            return;
        }

        if (!value.IsFullyLoaded)
        {
            this.FullyLoadDiskAsync(value, CancellationToken.None).FireAndForget();
        }
    }

    private async Task FullyLoadDiskAsync(OfflineStorageFolderSelectionDiskItemViewModel diskViewModel, CancellationToken ct)
    {
        diskViewModel.IsFullyLoading = true;

        var offlineDisk = await this.offlineExplorerService.GetDiskAsync(diskViewModel.OfflineDiskFile, ct);
        if (offlineDisk is not null)
        {
            var folderViewModels = offlineDisk
                .GetAllFolders()
                .OrderBy(f => f.Path)
                .Select(f => new OfflineStorageFolderSelectionFolderItemViewModel(f.Path))
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
