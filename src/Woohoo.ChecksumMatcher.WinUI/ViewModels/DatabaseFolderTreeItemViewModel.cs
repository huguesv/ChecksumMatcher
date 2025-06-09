// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public partial class DatabaseFolderTreeItemViewModel : DatabaseTreeItemViewModel
{
    [ObservableProperty]
    private bool isExpanded = true;

    public DatabaseFolderTreeItemViewModel(string name)
        : base(name, ExplorerItemType.Folder)
    {
    }
}
