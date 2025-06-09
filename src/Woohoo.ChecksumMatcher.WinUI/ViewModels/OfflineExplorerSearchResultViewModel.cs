// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

[DebuggerDisplay("Name = {Name}")]
public sealed partial class OfflineExplorerSearchResultViewModel : ObservableObject
{
    private readonly OfflineExplorerViewModel offlineExplorerViewModel;

    public OfflineExplorerSearchResultViewModel(
        OfflineExplorerFileViewModel item,
        OfflineExplorerViewModel offlineExplorerViewModel)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(offlineExplorerViewModel);

        this.Item = item;
        this.offlineExplorerViewModel = offlineExplorerViewModel;
    }

    public string Name => this.Item.Name;

    public string Container => this.Item.Container;

    public long Size => this.Item.Size;

    public OfflineExplorerFileViewModel Item { get; }

    [RelayCommand]
    private void OpenContainingFolder()
    {
        if (this.offlineExplorerViewModel.SelectedDisk is null)
        {
            return;
        }

        this.offlineExplorerViewModel.SelectedDisk.SelectedFolder = this.Item.ParentViewModel;

        if (this.offlineExplorerViewModel.SelectedDisk.SelectedFolder is null)
        {
            return;
        }

        this.offlineExplorerViewModel.SelectedDisk.SelectedFolder.SelectedFile = this.Item;
    }
}
