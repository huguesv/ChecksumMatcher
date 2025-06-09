// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;

public abstract class DatabaseTreeItemViewModel : ObservableObject
{
    public DatabaseTreeItemViewModel(string name, ExplorerItemType itemType, DatabaseFolderTreeItemViewModel? parentFolderItem)
    {
        ArgumentNullException.ThrowIfNull(name);

        this.Name = name;
        this.Type = itemType;
        this.ParentFolderItem = parentFolderItem;

        this.FilteredChildren = new AdvancedCollectionView(this.Children, true)
        {
            Filter = (obj) =>
            {
                if (obj is DatabaseTreeItemViewModel item)
                {
                    return item.IsFilterNameMatch();
                }

                return false;
            },
        };
    }

    public enum ExplorerItemType
    {
        Folder,
        File,
    }

    public string Name { get; }

    public ExplorerItemType Type { get; }

    public ObservableCollection<DatabaseTreeItemViewModel> Children { get; } = [];

    public AdvancedCollectionView FilteredChildren { get; }

    public DatabaseFolderTreeItemViewModel? ParentFolderItem { get; }

    public string FilterText { get; private set; } = string.Empty;

    public void ApplyFilter(string value)
    {
        this.FilterText = value;

        foreach (var child in this.Children)
        {
            child.ApplyFilter(value);
        }

        this.FilteredChildren.RefreshFilter();
    }

    protected virtual bool IsFilterNameMatch()
        => string.IsNullOrEmpty(this.FilterText) ||
           this.Name.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase) ||
           this.Children.Any(item => item.IsFilterNameMatch());
}
