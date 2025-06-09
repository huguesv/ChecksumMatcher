// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.SevenZip;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using global::SevenZip;

public class SevenZipFile
{
    static SevenZipFile()
    {
        var currentFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NotSupportedException();
        var binaryFilePath = Path.Combine(currentFolderPath, Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
        SevenZipBase.SetLibraryPath(binaryFilePath);
    }

    public SevenZipFile(string archiveFilePath)
    {
        Requires.NotNullOrEmpty(archiveFilePath);

        this.FilePath = archiveFilePath;
        this.Entries = new Collection<SevenZipEntry>();
        this.ReadEntries();
    }

    public string FilePath { get; }

    public Collection<SevenZipEntry> Entries { get; }

    public SevenZipEntry? GetEntry(string name)
    {
        Requires.NotNull(name);

        return this.Entries.SingleOrDefault(entry => entry.Name == name);
    }

    public void Extract(SevenZipEntry entry, string targetFilePath)
    {
        Requires.NotNull(entry);
        Requires.NotNullOrEmpty(targetFilePath);

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
