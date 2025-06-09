// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;

[DebuggerDisplay("Name = {Name} Label = {Label} SerialNumber = {SerialNumber} ")]
public sealed partial class OfflineExplorerDiskViewModel : ObservableObject
{
    public OfflineExplorerDiskViewModel(OfflineDiskFile offlineDiskFile)
    {
        ArgumentNullException.ThrowIfNull(offlineDiskFile);

        this.OfflineDiskFile = offlineDiskFile;
    }

    public OfflineDiskFile OfflineDiskFile { get; }

    public string FilePath => this.OfflineDiskFile.FilePath;

    public ObservableCollection<OfflineExplorerFolderViewModel> RootFolders { get; private set; } = [];

    public string Name => this.OfflineDiskFile.Header.Name;

    public string Label => this.OfflineDiskFile.Header.Label ?? string.Empty;

    public string SerialNumber => this.OfflineDiskFile.Header.SerialNumber ?? string.Empty;

    [ObservableProperty]
    public partial bool IsFullyLoaded { get; set; } = false;

    [ObservableProperty]
    public partial bool IsFullyLoading { get; set; } = false;

    [ObservableProperty]
    public partial OfflineExplorerFolderViewModel? SelectedFolder { get; set; }

    [ObservableProperty]
    public partial OfflineExplorerFolderViewModel? TreeSelectedFolder { get; set; }

    partial void OnTreeSelectedFolderChanged(OfflineExplorerFolderViewModel? value)
    {
        // Tree selection is only done by clicking on the tree view,
        // and we propagate the selection to the list view.
        // We do not bind the same property to both, this leads to EngineExecutionException.
        if (value is not null)
        {
            this.SelectedFolder = value;
        }
    }

    partial void OnSelectedFolderChanged(OfflineExplorerFolderViewModel? value)
    {
        if (value is not null)
        {
            value.SelectedFile = value?.Files.FirstOrDefault();
        }
    }
}
