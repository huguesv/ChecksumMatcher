// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class ScanOnlineFolderItemViewModel : ObservableObject
{
    private readonly DatabaseViewModel database;

    [ObservableProperty]
    private bool isIncluded = true;

    public ScanOnlineFolderItemViewModel(DatabaseViewModel database)
    {
        this.database = database;
    }

    public required string FolderPath { get; init; }

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
