// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline;

using System.IO;
using System.IO.Enumeration;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

internal class OfflineEnumerator
{
    private readonly OfflineConfiguration configuration;

    public OfflineEnumerator(OfflineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        this.configuration = configuration;
    }

    public static bool IsPatternMatch(OfflineItem item, string pattern)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(pattern);

        return IsPatternMatch(item.Name, pattern);
    }

    public static bool IsPatternMatch(string name, string pattern)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(pattern);

        if (string.IsNullOrEmpty(pattern))
        {
            return true; // No pattern means all paths match.
        }

        return FileSystemName.MatchesSimpleExpression(pattern, name);
    }

    public static string ToPath(OfflineItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.Path;
    }

    public static IFileSystemInfo ToInfo(OfflineConfiguration configuration, OfflineItem item)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(item);

        return item.Kind switch
        {
            OfflineItemKind.Folder => ToDirectoryInfo(configuration, item),
            OfflineItemKind.File or OfflineItemKind.ArchiveFile => ToFileInfo(configuration, item),
            _ => throw new NotSupportedException($"Unsupported item kind: {item.Kind}"),
        };
    }

    public static IDirectoryInfo ToDirectoryInfo(OfflineConfiguration configuration, OfflineItem item)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(item);

        return new OfflineDirectoryInfo(configuration, item);
    }

    public static IFileInfo ToFileInfo(OfflineConfiguration configuration, OfflineItem item)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(item);

        return new OfflineFileInfo(configuration, item);
    }

    public IEnumerable<OfflineItem> EnumerateItems(string path, params IEnumerable<Func<OfflineItem, bool>> filters)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var folder = this.configuration.GetItemByPath(path);
        if (folder is not null)
        {
            IEnumerable<OfflineItem> items = folder.Items;
            foreach (var filter in filters)
            {
                items = items.Where(filter);
            }

            return items;
        }

        return [];
    }

    public IEnumerable<OfflineItem> EnumerateItems(string path, EnumerationOptions enumerationOptions, params IEnumerable<Func<OfflineItem, bool>> filters)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        // TODO
        throw new NotImplementedException();
    }

    public IEnumerable<OfflineItem> EnumerateItems(string path, SearchOption searchOption, params IEnumerable<Func<OfflineItem, bool>> filters)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (searchOption == SearchOption.TopDirectoryOnly)
        {
            return this.EnumerateItems(path);
        }

        // For SearchOption.AllDirectories, we need to recursively search through all subdirectories.
        var directoriesToSearch = new Queue<OfflineItem>();

        var folder = this.configuration.GetItemByPath(path);
        if (folder is not null)
        {
            directoriesToSearch.Enqueue(folder);
        }

        IEnumerable<OfflineItem> results = [];

        while (directoriesToSearch.Count > 0)
        {
            var currentDirectory = directoriesToSearch.Dequeue();

            IEnumerable<OfflineItem> items = currentDirectory.Items;
            foreach (var filter in filters)
            {
                items = items.Where(filter);
            }

            results = results.Concat(items);

            foreach (var item in items)
            {
                if (item.Kind == OfflineItemKind.Folder)
                {
                    directoriesToSearch.Enqueue(item);
                }
            }
        }

        return results;
    }
}
