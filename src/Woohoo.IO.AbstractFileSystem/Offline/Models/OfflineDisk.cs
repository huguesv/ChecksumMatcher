// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline.Models;

using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

[DebuggerDisplay("Name = {Name} Label = {Label} Serial = {SerialNumber}")]
public sealed class OfflineDisk
{
    public const string DefaultFileExtension = ".offz";

    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { IgnoreEmptyLists },
        },
    };

    public string Name { get; set; } = string.Empty;

    public string? Label { get; set; }

    public string? SerialNumber { get; set; }

    public long TotalSize { get; set; }

    public long AvailableFreeSpace { get; set; }

    public long TotalFreeSpace { get; set; }

    public List<OfflineItem> RootFolders { get; init; } = [];

    public static OfflineDisk Deserialize(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        using (var zip = ZipFile.OpenRead(filePath))
        {
            var entry = zip.GetEntry("dir.json") ?? throw new FileNotFoundException($"Entry dir.json not found in {filePath}");
            using (var stream = entry.Open())
            {
                var disk = JsonSerializer.Deserialize<OfflineDisk>(stream) ?? throw new JsonException($"Failed to deserialize dir.json");

                foreach (var folder in disk.RootFolders)
                {
                    SetParentRecursive(folder, disk, null);
                }

                return disk;
            }
        }
    }

    public static OfflineHeader DeserializeHeader(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        using (var zip = ZipFile.OpenRead(filePath))
        {
            var entry = zip.GetEntry("header.json") ?? throw new FileNotFoundException($"Entry header.json not found in {filePath}");
            using (var stream = entry.Open())
            {
                return JsonSerializer.Deserialize<OfflineHeader>(stream) ?? throw new JsonException($"Failed to deserialize header.json");
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
        ArgumentException.ThrowIfNullOrEmpty(path);

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
        ArgumentNullException.ThrowIfNull(pathParts);

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
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in this.RootFolders)
        {
            action(item);
            item.TraverseItems(action);
        }
    }

    public IEnumerable<OfflineItem> SearchItems(string searchTerm)
    {
        ArgumentNullException.ThrowIfNull(searchTerm);

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
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        string? folderPath = Path.GetDirectoryName(filePath) ?? throw new ArgumentException("Invalid file path.", nameof(filePath));

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
            this.WriteHeader(zip);
            this.WriteFolders(zip);
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

    private static void IgnoreEmptyLists(JsonTypeInfo typeInfo)
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

    private void WriteFolders(ZipArchive zip)
    {
        var entry = zip.CreateEntry("dir.json");
        using (var stream = entry.Open())
        {
            JsonSerializer.Serialize(stream, this, SerializationOptions);
        }
    }

    private void WriteHeader(ZipArchive zip)
    {
        var header = new OfflineHeader
        {
            Name = this.Name,
            Label = this.Label,
            SerialNumber = this.SerialNumber,
            TotalSize = this.TotalSize,
            AvailableFreeSpace = this.AvailableFreeSpace,
            TotalFreeSpace = this.TotalFreeSpace,
        };

        var entry = zip.CreateEntry("header.json");
        using (var stream = entry.Open())
        {
            JsonSerializer.Serialize(stream, header, SerializationOptions);
        }
    }
}
