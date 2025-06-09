// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

public abstract class DatabaseTreeItemViewModel : ObservableObject
{
    public enum ExplorerItemType
    {
        Folder,
        File,
    }

    public string Name { get; }

    public ExplorerItemType Type { get; }

    public ObservableCollection<DatabaseTreeItemViewModel> Children { get; } = [];

    public DatabaseTreeItemViewModel(string name, ExplorerItemType itemType)
    {
        this.Name = name;
        this.Type = itemType;
    }
}
