// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline.Models;

using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

[DebuggerDisplay("Name = {Name}")]
public class OfflineItem
{
    public OfflineItemKind Kind { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? Created { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? Modified { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Size { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReportedCRC32 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MD5 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SHA1 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SHA256 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SHA512 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CRC32 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<OfflineItem> Items { get; init; } = [];

    [JsonIgnore]
    public OfflineDisk? ParentDisk { get; set; }

    [JsonIgnore]
    public OfflineItem? ParentItem { get; set; }

    public long GetFileCount()
    {
        long count = 0;

        if (this.Kind == OfflineItemKind.File || this.Kind == OfflineItemKind.ArchiveFile)
        {
            count++;
        }

        foreach (OfflineItem item in this.Items)
        {
            count += item.GetFileCount();
        }

        return count;
    }

    public IEnumerable<OfflineItem> GetAllItems()
    {
        yield return this;

        foreach (var item in this.Items)
        {
            foreach (var subItem in item.GetAllItems())
            {
                yield return subItem;
            }
        }
    }

    public IEnumerable<OfflineItem> GetAllFolders()
    {
        if (this.Kind == OfflineItemKind.Folder)
        {
            yield return this;

            foreach (var item in this.Items)
            {
                foreach (var subItem in item.GetAllFolders())
                {
                    yield return subItem;
                }
            }
        }
    }

    public IEnumerable<OfflineItem> SearchItems(string searchTerm)
    {
        foreach (var item in this.Items)
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

    public bool Match(string searchTerm, long? searchTermNumeric)
    {
        return
            this.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (this.CRC32 is not null && this.CRC32.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
            (this.MD5 is not null && this.MD5.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
            (this.SHA1 is not null && this.SHA1.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
            (searchTermNumeric.HasValue && this.Size == searchTermNumeric.Value);
    }

    public void TraverseItems(Action<OfflineItem> action)
    {
        action(this);
        foreach (var item in this.Items)
        {
            item.TraverseItems(action);
        }
    }

    public OfflineItem? GetItemByPath(string[] pathParts)
    {
        if (pathParts.Length == 0)
        {
            return this;
        }

        foreach (var item in this.Items)
        {
            if (item.Name.Equals(pathParts[0], StringComparison.OrdinalIgnoreCase))
            {
                return item.GetItemByPath(pathParts[1..]);
            }
        }

        return null;
    }
}
