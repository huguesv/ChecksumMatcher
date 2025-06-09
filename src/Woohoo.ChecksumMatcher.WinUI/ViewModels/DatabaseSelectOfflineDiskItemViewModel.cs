// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

[DebuggerDisplay("Name = {Name} Label = {Label} SerialNumber = {SerialNumber} ")]
public partial class DatabaseSelectOfflineDiskItemViewModel : ObservableObject
{
    public DatabaseSelectOfflineDiskItemViewModel(OfflineDiskFile offlineDiskFile)
    {
        ArgumentNullException.ThrowIfNull(offlineDiskFile);

        this.Folders = [];
        this.OfflineDiskFile = offlineDiskFile;
    }

    [ObservableProperty]
    public partial DatabaseSelectOfflineFolderItemViewModel? SelectedFolder { get; set; }

    [ObservableProperty]
    public partial bool IsFullyLoaded { get; set; } = false;

    [ObservableProperty]
    public partial bool IsFullyLoading { get; set; } = false;

    public OfflineDiskFile OfflineDiskFile { get; }

    public OfflineDisk? Disk { get; set; } = null;

    public ObservableCollection<DatabaseSelectOfflineFolderItemViewModel> Folders { get; private set; } = [];

    public string FilePath => this.OfflineDiskFile.FilePath;

    public string Name => this.OfflineDiskFile.Header.Name;

    public string Label => this.OfflineDiskFile.Header.Label ?? string.Empty;

    public string SerialNumber => this.OfflineDiskFile.Header.SerialNumber ?? string.Empty;
}
