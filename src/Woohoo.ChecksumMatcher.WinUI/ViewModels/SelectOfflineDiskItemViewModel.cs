// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

[DebuggerDisplay("Name = {Name} Label = {Label} SerialNumber = {SerialNumber} ")]
public partial class SelectOfflineDiskItemViewModel : ObservableObject
{
    [ObservableProperty]
    private SelectOfflineFolderItemViewModel? selectedFolder;

    public SelectOfflineDiskItemViewModel(OfflineDisk disk)
    {
        this.Disk = disk;

        this.Folders = new ObservableCollection<SelectOfflineFolderItemViewModel>(disk.GetAllFolders().OrderBy(f => f.Path).Select(f => new SelectOfflineFolderItemViewModel(f.Path)));
    }

    public OfflineDisk Disk { get; }

    public ObservableCollection<SelectOfflineFolderItemViewModel> Folders { get; private set; } = [];

    public string Name => this.Disk.Name;

    public string Label => this.Disk.Label ?? string.Empty;

    public string SerialNumber => this.Disk.SerialNumber ?? string.Empty;
}
