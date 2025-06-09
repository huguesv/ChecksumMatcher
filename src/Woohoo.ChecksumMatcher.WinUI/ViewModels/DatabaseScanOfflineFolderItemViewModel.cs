// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class DatabaseScanOfflineFolderItemViewModel : ObservableObject
{
    private readonly Func<DatabaseScanOfflineFolderItemViewModel, CancellationToken, Task> removeFunc;
    private readonly Action<DatabaseScanOfflineFolderItemViewModel, bool> changeIncludeFunc;

    public DatabaseScanOfflineFolderItemViewModel(
        string diskName,
        string folderPath,
        Func<DatabaseScanOfflineFolderItemViewModel, CancellationToken, Task> removeFunc,
        Action<DatabaseScanOfflineFolderItemViewModel, bool> changeIncludeFunc)
    {
        ArgumentNullException.ThrowIfNull(diskName);
        ArgumentNullException.ThrowIfNull(folderPath);
        ArgumentNullException.ThrowIfNull(removeFunc);
        ArgumentNullException.ThrowIfNull(changeIncludeFunc);

        this.DiskName = diskName;
        this.FolderPath = folderPath;
        this.removeFunc = removeFunc;
        this.changeIncludeFunc = changeIncludeFunc;
        this.DisplayName = $"[{diskName}] {folderPath}";
    }

    [ObservableProperty]
    public partial bool IsIncluded { get; set; } = true;

    public string FolderPath { get; }

    public string DiskName { get; }

    public string DisplayName { get; }

    [RelayCommand]
    private async Task RemoveAsync(CancellationToken ct)
    {
        await this.removeFunc(this, ct);
    }

    partial void OnIsIncludedChanged(bool value)
    {
        this.changeIncludeFunc(this, value);
    }
}
