// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

[DebuggerDisplay("Name = {Name} Label = {Label} SerialNumber = {SerialNumber} ")]
public partial class OfflineExplorerDiskViewModel : ObservableObject
{
    private readonly OfflineDisk disk;

    public OfflineExplorerDiskViewModel(OfflineDisk disk)
    {
        this.disk = disk;
    }

    public ObservableCollection<OfflineExplorerFolderViewModel> RootFolders { get; private set; } = [];

    public string Name => this.disk.Name;

    public string Label => this.disk.Label ?? string.Empty;

    public string SerialNumber => this.disk.SerialNumber ?? string.Empty;
}
