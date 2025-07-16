// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;
using Woohoo.IO.AbstractFileSystem.Offline.Models;
using static Woohoo.ChecksumMatcher.Core.Services.DatabaseService;

internal static class Scanner
{
    private static readonly IContainer FolderContainer = ContainerExtensionProvider.GetFolderContainer();

    public static async Task<DatabaseScanResults> ScanAsync(
        Action<ScanEventArgs> reportProgress,
        DatabaseFile file,
        RomDatabase db,
        EffectiveScanSettings scanSettings,
        Func<string, CancellationToken, Task<OfflineDisk?>> findOfflineDisk,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(reportProgress);
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(db);

        var usedButIncorrectName = new List<FileInformation>();
        var matched = new List<RomAndFileMoniker>();
        var wrongNamed = new List<RomAndFileMoniker>();
        var missing = new List<RomMoniker>();
        var unused = new List<FileMoniker>();

        ct.ThrowIfCancellationRequested();

        var files = await GetFilesAsync(findOfflineDisk, scanSettings.ScanOnlineFolders, scanSettings.ScanOfflineFolders, ct);

        foreach (var rom in db.GetAllRoms())
        {
            ct.ThrowIfCancellationRequested();

            // Find all files that match the rom size
            var sizeMatches = files.Where(file => file.DataBlockSize == rom.Size).ToArray();
            if (sizeMatches.Length > 0)
            {
                // Calculate checksums of all files that match the rom size, this can take a long time
                foreach (var sizeMatch in sizeMatches)
                {
                    ct.ThrowIfCancellationRequested();

                    if (!sizeMatch.AllChecksumsCalculated)
                    {
                        var container = ContainerExtensionProvider.GetContainer(sizeMatch.ContainerAbsolutePath);
                        if (container != null)
                        {
                            if (scanSettings.ForceCalculateChecksums || sizeMatch.ReportedCRC32.Length == 0)
                            {
                                reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Hashing, Results = new DatabaseScanResults(), HashingFile = new FileMoniker(sizeMatch.ContainerAbsolutePath, sizeMatch.ContainerName, sizeMatch.FileRelativePath) });

                                container.CalculateChecksums(sizeMatch, ct);

                                reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Scanning, Results = new DatabaseScanResults() });
                            }
                        }
                    }
                }

                // Find all files that match the checksums
                var checksumMatches = sizeMatches.Where(file => Scanner.DoesChecksumMatch(rom, file, !scanSettings.ForceCalculateChecksums)).ToArray();
                if (checksumMatches.Length > 0)
                {
                    // Find the file that has the correct container name (there can only be one)
                    var gameMatch = checksumMatches.FirstOrDefault(result => result.ContainerName == rom.ParentGame.Name);
                    if (gameMatch != null)
                    {
                        if (gameMatch.FileRelativePath == rom.Name)
                        {
                            // Perfect match of size, crc, container name and file name
                            // Let's remove it from the list of candidates now that it has been matched
                            _ = files.Remove(gameMatch);

                            var result = new RomAndFileMoniker(
                                new RomMoniker(rom.ParentGame.Name, rom.Name),
                                new FileMoniker(gameMatch.ContainerAbsolutePath, gameMatch.ContainerName, gameMatch.FileRelativePath));
                            matched.Add(result);

                            reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Scanning, Results = new DatabaseScanResults() { Matched = [result] } });
                        }
                        else
                        {
                            // Wrong file name, but correct container name
                            var result = new RomAndFileMoniker(
                                new RomMoniker(rom.ParentGame.Name, rom.Name),
                                new FileMoniker(gameMatch.ContainerAbsolutePath, gameMatch.ContainerName, gameMatch.FileRelativePath));
                            wrongNamed.Add(result);

                            reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Scanning, Results = new DatabaseScanResults() { WrongNamed = [result] } });

                            usedButIncorrectName.Add(gameMatch);
                        }
                    }
                    else
                    {
                        // Wrong container name (there may be multiple files matching size and crc, we'll pick the first one)
                        var result = new RomAndFileMoniker(
                            new RomMoniker(rom.ParentGame.Name, rom.Name),
                            new FileMoniker(checksumMatches[0].ContainerAbsolutePath, checksumMatches[0].ContainerName, checksumMatches[0].FileRelativePath));
                        wrongNamed.Add(result);

                        reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Scanning, Results = new DatabaseScanResults() { WrongNamed = [result] } });

                        usedButIncorrectName.AddRange(checksumMatches);
                    }
                }
                else
                {
                    var result = new RomMoniker(rom.ParentGame.Name, rom.Name);
                    missing.Add(result);

                    reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Scanning, Results = new DatabaseScanResults() { Missing = [result] } });
                }
            }
            else
            {
                var result = new RomMoniker(rom.ParentGame.Name, rom.Name);
                missing.Add(result);

                reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Scanning, Results = new DatabaseScanResults() { Missing = [result] } });
            }
        }

        var unusedFiles = files.Where(result => !usedButIncorrectName.Contains(result)).ToArray();
        foreach (var unusedFile in unusedFiles)
        {
            var result = new FileMoniker(unusedFile.ContainerAbsolutePath, unusedFile.ContainerName, unusedFile.FileRelativePath);
            unused.Add(result);
        }

        reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Scanning, Results = new DatabaseScanResults() { Unused = [.. unused] } });

        reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 100, Status = ScanStatus.Completed, Results = new DatabaseScanResults() });

        return new DatabaseScanResults
        {
            Matched = [.. matched],
            WrongNamed = [.. wrongNamed],
            Missing = [.. missing],
            Unused = [.. unused],
        };

        static async Task<List<FileInformation>> GetFilesAsync(Func<string, CancellationToken, Task<OfflineDisk?>> findOfflineDisk, List<EffectiveOnlineFolderSetting> scanOnlineFolders, List<EffectiveOfflineFolderSetting> scanOfflineFolders, CancellationToken ct)
        {
            var result = new List<FileInformation>();

            foreach (var folder in scanOnlineFolders)
            {
                var onlineFiles = FolderContainer.GetAllFiles(folder.FolderPath, SearchOption.AllDirectories, ct);
                result.AddRange(onlineFiles);
            }

            foreach (var folder in scanOfflineFolders)
            {
                var disk = await findOfflineDisk(folder.DiskName, ct);
                if (disk is null)
                {
                    continue;
                }

                IContainer container = ContainerExtensionProvider.GetOfflineFolderContainer(disk);
                var offlineFiles = container.GetAllFiles(folder.FolderPath, SearchOption.AllDirectories, ct);
                result.AddRange(offlineFiles);
            }

            return result;
        }
    }

    public static Task<DatabaseRebuildResults> RebuildAsync(
        Action<RebuildEventArgs> reportProgress,
        DatabaseFile file,
        RomDatabase db,
        RebuildSettings rebuildSettings,
        string[] cueFolders,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(reportProgress);
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(rebuildSettings);
        ArgumentNullException.ThrowIfNull(cueFolders);

        DatabaseRebuildResults rebuildResults = new();

        reportProgress(new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 0, Status = RebuildStatus.Started, Results = new DatabaseRebuildResults() });

        var roms = db.GetAllRoms();

        var sourceFiles = GetAllFiles(rebuildSettings.SourceFolderPath, SearchOption.AllDirectories, ct);

        foreach (var sourceFile in sourceFiles)
        {
            ct.ThrowIfCancellationRequested();

            EnsureChecksums(reportProgress, file, sourceFile, rebuildSettings, ct);

            var matchedRoms = roms.Where(rom => Scanner.DoesChecksumMatch(rom, sourceFile, !rebuildSettings.ForceCalculateChecksums)).ToArray();
            if (matchedRoms.Length > 0)
            {
                Directory.CreateDirectory(rebuildSettings.TargetFolderPath);
                RebuildFile(reportProgress, file, sourceFile, matchedRoms, rebuildSettings.TargetFolderPath, rebuildSettings.TargetContainerType, rebuildSettings.RemoveSource, ct);
            }
            else
            {
                var result = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath);
                rebuildResults.Unused.Add(result);

                reportProgress(new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 0, Status = RebuildStatus.Scanning, Results = new DatabaseRebuildResults() { Unused = [result] } });
            }
        }

        if (rebuildSettings.FindMissingCueFiles)
        {
            CheckForMissingCueFiles(reportProgress, file, db, rebuildSettings, cueFolders, ct);
        }

        reportProgress(new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 100, Status = RebuildStatus.Completed, Results = new DatabaseRebuildResults() });

        return Task.FromResult(rebuildResults);

        static void RebuildFile(Action<RebuildEventArgs> reportProgress, DatabaseFile databaseFile, FileInformation sourceFile, RomFile[] matchedRoms, string targetFolderPath, string targetContainerType, bool removeSource, CancellationToken ct)
        {
            var fileMoniker = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath);

            var sourceContainer = ContainerExtensionProvider.GetContainer(sourceFile.ContainerAbsolutePath);
            if (sourceContainer != null)
            {
                foreach (var matchedRom in matchedRoms)
                {
                    ct.ThrowIfCancellationRequested();

                    if (sourceContainer.Exists(sourceFile))
                    {
                        var expectedTargetFiles = matchedRom.ParentGame.Roms.Select(rom => rom.Name).ToArray();

                        var copier = FileCopierExtensionProvider.GetCopier(sourceFile, targetContainerType, expectedTargetFiles);
                        if (copier != null)
                        {
                            var romMoniker = new RomMoniker(matchedRom.ParentGame.Name, matchedRom.Name);

                            reportProgress(new RebuildEventArgs { DatabaseFile = databaseFile, ProgressPercentage = 0, Status = RebuildStatus.Building, Results = new DatabaseRebuildResults(), BuildingRom = romMoniker });

                            _ = copier.Copy(sourceFile, targetFolderPath, removeSource, matchedRoms.Length == 1, matchedRom.ParentGame.Name, matchedRom.Name, expectedTargetFiles);

                            var result = new RomAndFileMoniker(romMoniker, fileMoniker);
                            reportProgress(new RebuildEventArgs { DatabaseFile = databaseFile, ProgressPercentage = 0, Status = RebuildStatus.Scanning, Results = new DatabaseRebuildResults() { Matched = [result] } });
                        }
                    }
                }

                ct.ThrowIfCancellationRequested();

                if (removeSource && sourceContainer.Exists(sourceFile))
                {
                    sourceContainer.Remove(sourceFile);
                }
            }
        }

        static void CheckForMissingCueFiles(Action<RebuildEventArgs> reportProgress, DatabaseFile file, RomDatabase db, RebuildSettings rebuildSettings, string[] cueFolders, CancellationToken ct)
        {
            FileInformation[]? rebuiltFiles = null;
            FileInformation[]? cueSourceFiles = null;
            foreach (var game in db.Games)
            {
                ct.ThrowIfCancellationRequested();

                var cueRoms = game.Roms.Where(r => r.Name.EndsWith(".cue", StringComparison.OrdinalIgnoreCase)).ToArray();
                if (cueRoms.Length == 0)
                {
                    continue;
                }

                rebuiltFiles ??= GetAllFiles(rebuildSettings.TargetFolderPath, SearchOption.AllDirectories, ct);
                var rebuiltGameFiles = rebuiltFiles.Where(fi => fi.ContainerName == game.Name);

                var missingCueRoms = cueRoms.Where(cr => !rebuiltGameFiles.Any(gf => gf.FileRelativePath == cr.Name)).ToArray();
                if (missingCueRoms.Length == 0)
                {
                    continue;
                }

                var nonCueRoms = game.Roms.Where(r => !r.Name.EndsWith(".cue", StringComparison.OrdinalIgnoreCase)).ToArray();
                var allNonCueFound = nonCueRoms.All(ncr => rebuiltGameFiles.Any(gf => gf.FileRelativePath == ncr.Name));
                if (!allNonCueFound)
                {
                    continue;
                }

                // We have all non-cue files but not all cue files
                cueSourceFiles ??= GetAllCueFiles(cueFolders, ct);

                foreach (var cueRom in missingCueRoms)
                {
                    // First check for a match by exact name
                    var cueSourceFile = cueSourceFiles.FirstOrDefault(csf => csf.FileRelativePath == cueRom.Name && cueRom.ParentGame.Name == cueRom.ParentGame.Name);
                    if (cueSourceFile is null)
                    {
                        // No source file found for the cue file, we cannot rebuild it
                        continue;
                    }

                    // We have a name match, now check if the checksums match
                    EnsureChecksums(reportProgress, file, cueSourceFile, rebuildSettings, ct);
                    if (!Scanner.DoesChecksumMatch(cueRom, cueSourceFile, !rebuildSettings.ForceCalculateChecksums))
                    {
                        // Cue file checksum does not match, we cannot rebuild it
                        continue;
                    }

                    // We can rebuild the cue file
                    Directory.CreateDirectory(rebuildSettings.TargetFolderPath);
                    RebuildFile(reportProgress, file, cueSourceFile, [cueRom], rebuildSettings.TargetFolderPath, rebuildSettings.TargetContainerType, removeSource: false, ct: ct);
                }
            }
        }

        static FileInformation[] GetAllCueFiles(string[] cueFolders, CancellationToken ct)
        {
            return cueFolders
                .SelectMany(cueFolder => GetAllFiles(cueFolder, SearchOption.AllDirectories, ct))
                .Where(file => file.FileRelativePath.EndsWith(".cue", StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        static void EnsureChecksums(Action<RebuildEventArgs> reportProgress, DatabaseFile file, FileInformation sourceFile, RebuildSettings rebuildSettings, CancellationToken ct)
        {
            if (!sourceFile.AllChecksumsCalculated)
            {
                var container = ContainerExtensionProvider.GetContainer(sourceFile.ContainerAbsolutePath);
                if (container != null)
                {
                    if (rebuildSettings.ForceCalculateChecksums || sourceFile.ReportedCRC32.Length == 0)
                    {
                        reportProgress(new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 0, Status = RebuildStatus.Hashing, Results = new DatabaseRebuildResults(), HashingFile = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath) });

                        container.CalculateChecksums(sourceFile, ct);

                        reportProgress(new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 0, Status = RebuildStatus.Scanning, Results = new DatabaseRebuildResults() });
                    }
                }

                ct.ThrowIfCancellationRequested();
            }
        }
    }

    public static Task CreateDatabaseAsync(
        Action<DatabaseCreateEventArgs> reportProgress,
        string sourceFolderPath,
        string targetFolderPath,
        string targetDatabaseFilePath,
        DatabaseCreateSettings settings,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(reportProgress);
        ArgumentException.ThrowIfNullOrEmpty(sourceFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(targetDatabaseFilePath);
        ArgumentNullException.ThrowIfNull(settings);

        var db = new RomDatabase
        {
            Name = settings.Name,
            Description = settings.Description,
            Category = settings.Category,
            Version = settings.Version,
            Date = settings.Date,
            Author = settings.Author,
            Email = settings.Email,
            Homepage = settings.Homepage,
            Url = settings.Url,
            Comment = settings.Comment,
        };

        reportProgress(new DatabaseCreateEventArgs { ProgressPercentage = 0, Status = DatabaseCreateStatus.Started, Results = new DatabaseCreateResults() });

        ct.ThrowIfCancellationRequested();

        // Look for uncompressed games
        foreach (var folderInfo in new DirectoryInfo(sourceFolderPath).EnumerateDirectories())
        {
            ct.ThrowIfCancellationRequested();

            var game = new RomGame(db)
            {
                Name = Path.GetFileName(folderInfo.FullName),
            };

            game.Description = game.Name;
            db.Games.Add(game);

            foreach (var fileInformation in GetAllFiles(folderInfo.FullName, SearchOption.AllDirectories, ct))
            {
                ct.ThrowIfCancellationRequested();

                AddRomToGame(reportProgress, fileInformation, game, settings.ForceCalculateChecksums, ct);
            }
        }

        // Look for compressed games
        foreach (var fileInfo in new DirectoryInfo(sourceFolderPath).EnumerateFiles())
        {
            ct.ThrowIfCancellationRequested();

            var compressedContainer = ContainerExtensionProvider.GetContainer(fileInfo.FullName);
            if (compressedContainer != null)
            {
                var game = new RomGame(db)
                {
                    Name = Path.GetFileNameWithoutExtension(fileInfo.FullName),
                };

                game.Description = game.Name;
                db.Games.Add(game);

                foreach (var fileInformation in compressedContainer.GetAllFiles(fileInfo.FullName, SearchOption.AllDirectories, ct))
                {
                    ct.ThrowIfCancellationRequested();

                    AddRomToGame(reportProgress, fileInformation, game, settings.ForceCalculateChecksums, ct);
                }
            }
        }

        ct.ThrowIfCancellationRequested();

        db.SortGames();

        ct.ThrowIfCancellationRequested();

        var xml = new ClrMameXmlExporter().Export(db);

        Directory.CreateDirectory(targetFolderPath);
        File.WriteAllText(targetDatabaseFilePath, xml, Encoding.UTF8);

        reportProgress(new DatabaseCreateEventArgs { ProgressPercentage = 100, Status = DatabaseCreateStatus.Completed, Results = new DatabaseCreateResults() });

        return Task.CompletedTask;

        static void AddRomToGame(Action<DatabaseCreateEventArgs> reportProgress, FileInformation sourceFile, RomGame game, bool forceCalculateChecksums, CancellationToken ct)
        {
            EnsureChecksums(reportProgress, sourceFile, forceCalculateChecksums, ct);

            var file = new RomFile(game)
            {
                Name = sourceFile.FileRelativePath,
                Size = sourceFile.Size,
                CRC32 = sourceFile.CRC32,
                MD5 = sourceFile.MD5,
                SHA1 = sourceFile.SHA1,
            };
            game.Roms.Add(file);

            var fileMoniker = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath);
            var romMoniker = new RomMoniker(game.Name, file.Name);
            var result = new RomAndFileMoniker(romMoniker, fileMoniker);

            reportProgress(new DatabaseCreateEventArgs { ProgressPercentage = 0, Status = DatabaseCreateStatus.Scanning, Results = new DatabaseCreateResults() { Added = [result] } });
        }

        static void EnsureChecksums(Action<DatabaseCreateEventArgs> reportProgress, FileInformation sourceFile, bool forceCalculateChecksums, CancellationToken ct)
        {
            if (!sourceFile.AllChecksumsCalculated)
            {
                var container = ContainerExtensionProvider.GetContainer(sourceFile.ContainerAbsolutePath);
                if (container != null)
                {
                    if (forceCalculateChecksums || sourceFile.ReportedCRC32.Length == 0)
                    {
                        reportProgress(new DatabaseCreateEventArgs { ProgressPercentage = 0, Status = DatabaseCreateStatus.Hashing, Results = new DatabaseCreateResults(), HashingFile = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath) });

                        container.CalculateChecksums(sourceFile, ct);

                        reportProgress(new DatabaseCreateEventArgs { ProgressPercentage = 0, Status = DatabaseCreateStatus.Scanning, Results = new DatabaseCreateResults() });
                    }
                }
            }
        }
    }

    private static FileInformation[] GetAllFiles(string folderPath, SearchOption option, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderPath);

        return FolderContainer.GetAllFiles(folderPath, option, ct);
    }

    private static bool DoesChecksumMatch(RomFile rom, FileInformation file, bool useReportedCrcIfAvailable)
    {
        ArgumentNullException.ThrowIfNull(rom);
        ArgumentNullException.ThrowIfNull(file);

        var matchCrc = false;
        var matchMd5 = false;
        var matchSha1 = false;
        var matchSha256 = false;

        if (useReportedCrcIfAvailable)
        {
            if (rom.CRC32.Length > 0 && file.ReportedCRC32.Length > 0)
            {
                return rom.CRC32.SequenceEqual(file.ReportedCRC32);
            }
        }

        if (rom.MD5.Length > 0 && file.MD5.Length > 0)
        {
            if (!rom.MD5.SequenceEqual(file.MD5))
            {
                return false;
            }

            matchMd5 = true;
        }

        if (rom.SHA1.Length > 0 && file.SHA1.Length > 0)
        {
            if (!rom.SHA1.SequenceEqual(file.SHA1))
            {
                return false;
            }

            matchSha1 = true;
        }

        if (rom.SHA256.Length > 0 && file.SHA256.Length > 0)
        {
            if (!rom.SHA256.SequenceEqual(file.SHA256))
            {
                return false;
            }

            matchSha256 = true;
        }

        if (rom.CRC32.Length > 0 && file.CRC32.Length > 0)
        {
            if (!rom.CRC32.SequenceEqual(file.CRC32))
            {
                return false;
            }

            matchCrc = true;
        }

        return matchCrc || matchMd5 || matchSha1 || matchSha256;
    }
}
