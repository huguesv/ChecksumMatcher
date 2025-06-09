// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public sealed partial class DatabaseScanOfflineFolderItemViewModel : ObservableObject
{
    private readonly Action<DatabaseScanOfflineFolderItemViewModel, bool> changeIncludeFunc;

    public DatabaseScanOfflineFolderItemViewModel(
        string diskName,
        string folderPath,
        Action<DatabaseScanOfflineFolderItemViewModel, bool> changeIncludeFunc,
        IAsyncRelayCommand? removeCommand)
    {
        ArgumentNullException.ThrowIfNull(diskName);
        ArgumentNullException.ThrowIfNull(folderPath);
        ArgumentNullException.ThrowIfNull(changeIncludeFunc);

        this.DiskName = diskName;
        this.FolderPath = folderPath;
        this.changeIncludeFunc = changeIncludeFunc;
        this.DisplayName = $"[{diskName}] {folderPath}";
        this.RemoveCommand = removeCommand;
    }

    public IAsyncRelayCommand? RemoveCommand { get; }

    [ObservableProperty]
    public partial bool IsIncluded { get; set; } = true;

    public string FolderPath { get; }

    public string DiskName { get; }

    public string DisplayName { get; }

    partial void OnIsIncludedChanged(bool value)
    {
        this.changeIncludeFunc(this, value);
    }
}
