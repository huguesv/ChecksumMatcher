// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.SevenZip;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using global::SevenZip;

public class SevenZipFile
{
    static SevenZipFile()
    {
        SevenZipLibrary.Initialize();
    }

    public SevenZipFile(string archiveFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(archiveFilePath);

        this.FilePath = archiveFilePath;
        this.Entries = [];
        this.ReadEntries();
    }

    public string FilePath { get; }

    public Collection<SevenZipEntry> Entries { get; }

    public static void CreateOrAppend(string archiveFilePath, string itemFilePath, string itemName)
    {
        ArgumentException.ThrowIfNullOrEmpty(archiveFilePath);
        ArgumentException.ThrowIfNullOrEmpty(itemFilePath);
        ArgumentException.ThrowIfNullOrEmpty(itemName);

        var compressor = new SevenZipCompressor
        {
            CompressionMode = File.Exists(archiveFilePath) ? CompressionMode.Append : CompressionMode.Create,
            CompressionMethod = CompressionMethod.Copy,
            ArchiveFormat = OutArchiveFormat.SevenZip,
            DefaultItemName = itemName,
        };

        using var inStream = new FileStream(itemFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var input = new Dictionary<string, Stream>
        {
            [itemName] = inStream,
        };

        compressor.CompressStreamDictionary(input, archiveFilePath);
    }

    public SevenZipEntry? GetEntry(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return this.Entries.SingleOrDefault(entry => entry.Name == name);
    }

    public void Extract(SevenZipEntry entry, string targetFilePath)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);

        using (var extractor = new SevenZipExtractor(this.FilePath))
        using (var outputStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
        {
            extractor.ExtractFile(entry.Index, outputStream);
        }
    }

    private void ReadEntries()
    {
        using (var extractor = new SevenZipExtractor(this.FilePath))
        {
            foreach (var data in extractor.ArchiveFileData)
            {
                var entry = new SevenZipEntry
                {
                    Index = data.Index,
                    Name = data.FileName,
                    Size = data.Size,
                    CRC32 = data.Crc,
                    LastWriteTime = data.LastWriteTime,
                    CreationTime = data.CreationTime,
                    LastAccessTime = data.LastAccessTime,
                    IsDirectory = data.IsDirectory,
                    IsEncrypted = data.Encrypted,
                    Comment = data.Comment,
                    Method = data.Method,
                };

                this.Entries.Add(entry);
            }
        }
    }
}
