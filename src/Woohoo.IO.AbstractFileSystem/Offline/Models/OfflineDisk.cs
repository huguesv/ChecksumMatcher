// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline.Models;

using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

[DebuggerDisplay("Name = {Name} Label = {Label} Serial = {SerialNumber}")]
public class OfflineDisk
{
    public string Name { get; set; } = string.Empty;

    public string? Label { get; set; }

    public string? SerialNumber { get; set; }

    public long TotalSize { get; set; }

    public List<OfflineItem> RootFolders { get; init; } = [];

    public static OfflineDisk Deserialize(string filePath)
    {
        using (var zip = ZipFile.OpenRead(filePath))
        {
            var entry = zip.GetEntry("dir.json");
            if (entry == null)
            {
                throw new FileNotFoundException($"Entry dir.json not found in {filePath}.zip");
            }

            using (var stream = entry.Open())
            {
                var disk = JsonSerializer.Deserialize<OfflineDisk>(stream);
                if (disk == null)
                {
                    throw new JsonException($"Failed to deserialize dir.json");
                }

                foreach (var folder in disk.RootFolders)
                {
                    SetParentRecursive(folder, disk, null);
                }

                return disk;
            }
        }
    }

    public long GetFileCount()
    {
        return this.RootFolders.Sum(item => item.GetFileCount());
    }

    public IEnumerable<OfflineItem> GetAllItems()
    {
        foreach (var item in this.RootFolders)
        {
            foreach (var subItem in item.GetAllItems())
            {
                yield return subItem;
            }
        }
    }

    public IEnumerable<OfflineItem> GetAllFolders()
    {
        foreach (var item in this.RootFolders)
        {
            foreach (var subItem in item.GetAllFolders())
            {
                yield return subItem;
            }
        }
    }

    public OfflineItem? GetItemByPath(string path)
    {
        string[] pathParts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        if (pathParts.Length > 0)
        {
            if (pathParts[0].EndsWith(":"))
            {
                pathParts[0] += '\\';
            }

            return this.GetItemByPath(pathParts);
        }

        return null;
    }

    public OfflineItem? GetItemByPath(string[] pathParts)
    {
        foreach (var item in this.RootFolders)
        {
            if (pathParts.Length == 0)
            {
                return item;
            }

            if (item.Name.Equals(pathParts[0], StringComparison.OrdinalIgnoreCase))
            {
                return item.GetItemByPath(pathParts[1..]);
            }
        }

        return null;
    }

    public void TraverseItems(Action<OfflineItem> action)
    {
        foreach (var item in this.RootFolders)
        {
            action(item);
            item.TraverseItems(action);
        }
    }

    public IEnumerable<OfflineItem> SearchItems(string searchTerm)
    {
        foreach (var item in this.RootFolders)
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

    public void Serialize(string filePath)
    {
        string? folderPath = Path.GetDirectoryName(filePath);
        if (folderPath is null)
        {
            throw new ArgumentException("Invalid file path.", nameof(filePath));
        }

        Directory.CreateDirectory(folderPath);

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (IOException)
            {
            }
        }

        using (var zip = ZipFile.Open(filePath, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry("dir.json");
            using (var stream = entry.Open())
            {
                JsonSerializer.Serialize(stream, this, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true,
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver
                    {
                        Modifiers = { IgnoreEmptyLists },
                    },
                });
            }
        }

        static void IgnoreEmptyLists(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Type == typeof(OfflineItem))
            {
                foreach (var property in typeInfo.Properties.Where(p => p.PropertyType == typeof(List<OfflineItem>)))
                {
                    property.ShouldSerialize = (_, value) =>
                    {
                        var list = value as List<OfflineItem>;
                        return list?.Count > 0;
                    };
                }
            }
        }
    }

    private static void SetParentRecursive(OfflineItem item, OfflineDisk parentDisk, OfflineItem? parentItem)
    {
        item.ParentDisk = parentDisk;
        item.ParentItem = parentItem;
        foreach (var child in item.Items)
        {
            SetParentRecursive(child, parentDisk, item);
        }
    }
}
