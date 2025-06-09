// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using System;
using System.IO;
using System.Text;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.IO.AbstractFileSystem;

public class DatabaseCreator : IDatabaseCreator
{
    private bool cancel;

    public event EventHandler<ScannerProgressEventArgs>? Progress;

    public void Cancel()
    {
        this.cancel = true;
    }

    public void Build(string sourceFolderPath, string targetFilePath, DatabaseCreatorOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(targetFilePath);
        ArgumentNullException.ThrowIfNull(options);

        this.cancel = false;

        // Enumerate all files on disk, this can take a long time
        this.OnProgress(null, null, ScannerProgress.EnumeratingFilesStart);

        var db = new RomDatabase
        {
            Name = options.Name,
            Description = options.Description,
            Category = options.Category,
            Version = options.Version,
            Date = options.Date,
            Author = options.Author,
            Email = options.Email,
            Homepage = options.Homepage,
            Url = options.Url,
            Comment = options.Comment,
        };

        // Look for uncompressed games
        foreach (var folderInfo in new DirectoryInfo(sourceFolderPath).EnumerateDirectories())
        {
            var game = new RomGame(db)
            {
                Name = Path.GetFileName(folderInfo.FullName),
            };
            game.Description = game.Name;
            db.Games.Add(game);

            foreach (var fileInformation in FileSystem.GetAllFiles(folderInfo.FullName, SearchOption.AllDirectories))
            {
                if (this.cancel)
                {
                    this.OnProgress(ScannerProgress.Canceled);
                    return;
                }

                this.CalculateChecksums(fileInformation, options.ForceCalculateChecksums);

                var file = new RomFile(game)
                {
                    Name = fileInformation.FileRelativePath,
                    Size = fileInformation.Size,
                    CRC32 = fileInformation.CRC32,
                    MD5 = fileInformation.MD5,
                    SHA1 = fileInformation.SHA1,
                };
                game.Roms.Add(file);
            }
        }

        // Look for compressed games
        foreach (var fileInfo in new DirectoryInfo(sourceFolderPath).EnumerateFiles())
        {
            if (this.cancel)
            {
                this.OnProgress(ScannerProgress.Canceled);
                return;
            }

            var compressedContainer = ContainerExtensionProvider.GetContainer(fileInfo.FullName);
            if (compressedContainer != null)
            {
                var game = new RomGame(db)
                {
                    Name = Path.GetFileNameWithoutExtension(fileInfo.FullName),
                };
                game.Description = game.Name;
                db.Games.Add(game);

                foreach (var fileInformation in compressedContainer.GetAllFiles(fileInfo.FullName, SearchOption.AllDirectories))
                {
                    this.CalculateChecksums(fileInformation, options.ForceCalculateChecksums);

                    var file = new RomFile(game)
                    {
                        Name = fileInformation.FileRelativePath,
                        Size = fileInformation.Size,
                        CRC32 = fileInformation.CRC32,
                        MD5 = fileInformation.MD5,
                        SHA1 = fileInformation.SHA1,
                    };
                    game.Roms.Add(file);
                }
            }
        }

        db.SortGames();

        var exporter = new ClrMameXmlExporter();
        var xml = exporter.Export(db);
        File.WriteAllText(targetFilePath, xml, Encoding.UTF8);

        this.OnProgress(ScannerProgress.Finished);
    }

    private void CalculateChecksums(FileInformation file, bool forceCalculateChecksums)
    {
        if (!file.AllChecksumsCalculated)
        {
            var container = ContainerExtensionProvider.GetContainer(file.ContainerAbsolutePath);
            if (container != null)
            {
                if (forceCalculateChecksums || file.ReportedCRC32.Length == 0)
                {
                    this.OnProgress(null, file, ScannerProgress.CalculatingChecksumsStart);

                    container.CalculateChecksums(file);

                    this.OnProgress(null, file, ScannerProgress.CalculatingChecksumsEnd);
                }
            }
        }
    }

    private void OnProgress(RomFile? rom, FileInformation? file, ScannerProgress type)
    {
        if (this.Progress != null)
        {
            var args = new ScannerProgressEventArgs
            {
                Rom = rom,
                File = file,
                Status = type,
            };

            this.Progress(this, args);
        }
    }

    private void OnProgress(ScannerProgress type)
    {
        if (this.Progress != null)
        {
            var args = new ScannerProgressEventArgs
            {
                Status = type,
            };

            this.Progress(this, args);
        }
    }
}
