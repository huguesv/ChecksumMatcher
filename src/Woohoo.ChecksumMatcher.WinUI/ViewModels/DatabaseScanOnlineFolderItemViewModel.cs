// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class DatabaseScanOnlineFolderItemViewModel : ObservableObject
{
    private readonly Func<DatabaseScanOnlineFolderItemViewModel, CancellationToken, Task> removeFunc;
    private readonly Action<DatabaseScanOnlineFolderItemViewModel, bool> changeIncludeFunc;

    public DatabaseScanOnlineFolderItemViewModel(
        Func<DatabaseScanOnlineFolderItemViewModel, CancellationToken, Task> removeFunc,
        Action<DatabaseScanOnlineFolderItemViewModel, bool> changeIncludeFunc)
    {
        ArgumentNullException.ThrowIfNull(removeFunc);
        ArgumentNullException.ThrowIfNull(changeIncludeFunc);

        this.removeFunc = removeFunc;
        this.changeIncludeFunc = changeIncludeFunc;
    }

    [ObservableProperty]
    public partial bool IsIncluded { get; set; } = true;

    public required string FolderPath { get; init; }

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
