// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

public partial class ScanOfflineFolderItemViewModel : ObservableObject
{
    private readonly DatabaseViewModel database;

    [ObservableProperty]
    private bool isIncluded = true;

    public ScanOfflineFolderItemViewModel(DatabaseViewModel database, string diskName, string folderPath)
    {
        this.database = database;
        this.DiskName = diskName;
        this.FolderPath = folderPath;
        this.DisplayName = $"[{diskName}] {folderPath}";
    }

    public string FolderPath { get; }

    public string DiskName { get; }

    public string DisplayName { get; }

    [RelayCommand]
    public void Remove()
    {
        this.database.RemoveScanFolder(this);
    }

    partial void OnIsIncludedChanged(bool value)
    {
        this.database.OnIsIncludedChanged(this, value);
    }
}
