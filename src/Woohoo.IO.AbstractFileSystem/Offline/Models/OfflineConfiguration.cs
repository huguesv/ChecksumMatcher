// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline.Models;

using System.Collections.Generic;
using System.Collections.Immutable;

public sealed class OfflineConfiguration
{
    public OfflineConfiguration(params IEnumerable<OfflineDisk> disks)
    {
        ArgumentNullException.ThrowIfNull(disks);

        this.Disks = [.. disks];
    }

    public ImmutableArray<OfflineDisk> Disks { get; }

    public OfflineItem? GetItemByPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        foreach (var disk in this.Disks)
        {
            var item = disk.GetItemByPath(path);
            if (item != null)
            {
                return item;
            }
        }

        return null;
    }

    public void TraverseItems(Action<OfflineItem> action)
    {
        foreach (var disk in this.Disks)
        {
            foreach (var item in disk.RootFolders)
            {
                action(item);
                item.TraverseItems(action);
            }
        }
    }

    public IEnumerable<OfflineItem> SearchItems(string searchTerm)
    {
        foreach (var disk in this.Disks)
        {
            foreach (var item in disk.RootFolders)
            {
                if (item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    yield return item;
                }

                foreach (var subItem in item.SearchItems(searchTerm))
                {
                    yield return subItem;
                }
            }
        }
    }
}
