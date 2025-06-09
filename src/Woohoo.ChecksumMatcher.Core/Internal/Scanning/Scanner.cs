// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;
using Woohoo.IO.AbstractFileSystem.Offline.Models;
using Woohoo.IO.Compression.TorrentSevenZip;
using Woohoo.IO.Compression.TorrentZip;

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
                                reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Hashing, Results = new DatabaseScanResults(), HashingFile = new FileMoniker(sizeMatch.ContainerAbsolutePath, sizeMatch.ContainerName, sizeMatch.FileRelativePath, sizeMatch.IsFromOfflineStorage) });

                                await container.CalculateChecksumsAsync(sizeMatch, ct);

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
                                new FileMoniker(gameMatch.ContainerAbsolutePath, gameMatch.ContainerName, gameMatch.FileRelativePath, gameMatch.IsFromOfflineStorage));
                            matched.Add(result);

                            reportProgress(new ScanEventArgs { DatabaseFile = file, Database = db, ProgressPercentage = 0, Status = ScanStatus.Scanning, Results = new DatabaseScanResults() { Matched = [result] } });
                        }
                        else
                        {
                            // Wrong file name, but correct container name
                            var result = new RomAndFileMoniker(
                                new RomMoniker(rom.ParentGame.Name, rom.Name),
                                new FileMoniker(gameMatch.ContainerAbsolutePath, gameMatch.ContainerName, gameMatch.FileRelativePath, gameMatch.IsFromOfflineStorage));
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
                            new FileMoniker(checksumMatches[0].ContainerAbsolutePath, checksumMatches[0].ContainerName, checksumMatches[0].FileRelativePath, checksumMatches[0].IsFromOfflineStorage));
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
            var result = new FileMoniker(unusedFile.ContainerAbsolutePath, unusedFile.ContainerName, unusedFile.FileRelativePath, unusedFile.IsFromOfflineStorage);
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
                var onlineFiles = await FolderContainer.GetAllFilesAsync(folder.FolderPath, SearchOption.AllDirectories, ct);
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
                var offlineFiles = await container.GetAllFilesAsync(folder.FolderPath, SearchOption.AllDirectories, ct);
                result.AddRange(offlineFiles);
            }

            return result;
        }
    }

    public static async Task<DatabaseRebuildResults> RebuildAsync(
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

        var sourceFiles = await GetAllFilesAsync(rebuildSettings.SourceFolderPath, SearchOption.AllDirectories, ct);
        var rebuiltContainers = new HashSet<string>();

        foreach (var sourceFile in sourceFiles)
        {
            ct.ThrowIfCancellationRequested();

            await EnsureChecksumsAsync(reportProgress, file, sourceFile, rebuildSettings, ct);

            var matchedRoms = roms.Where(rom => Scanner.DoesChecksumMatch(rom, sourceFile, !rebuildSettings.ForceCalculateChecksums)).ToArray();
            if (matchedRoms.Length > 0)
            {
                Directory.CreateDirectory(rebuildSettings.TargetFolderPath);
                await RebuildFileAsync(reportProgress, file, sourceFile, matchedRoms, rebuildSettings.TargetFolderPath, rebuildSettings.TargetContainerType, rebuildSettings.RemoveSource, ct);

                foreach (var matchedRom in matchedRoms)
                {
                    rebuiltContainers.Add(matchedRom.ParentGame.Name);
                }
            }
            else
            {
                var result = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath, sourceFile.IsFromOfflineStorage);
                rebuildResults.Unused.Add(result);

                reportProgress(new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 0, Status = RebuildStatus.Scanning, Results = new DatabaseRebuildResults() { Unused = [result] } });
            }
        }

        if (rebuildSettings.FindMissingCueFiles)
        {
            await CheckForMissingCueFilesAsync(reportProgress, file, db, rebuildSettings, cueFolders, ct);
        }

        // Note: This may rename incomplete containers (from .zip to .7z)
        if (rebuildSettings.TorrentZipIncomplete && (rebuildSettings.TargetContainerType == KnownContainerTypes.TorrentZip || rebuildSettings.TargetContainerType == KnownContainerTypes.TorrentSevenZip))
        {
            await TorrentzipIncompleteContainersAsync(
                [.. db.Games.Where(g => rebuiltContainers.Contains(g.Name))],
                rebuildSettings,
                ct);
        }

        // Enumerate again the incomplete containers (they may have been renamed
        // in the previous step) and move them to the incomplete folder.
        if (!string.IsNullOrEmpty(rebuildSettings.TargetIncompleteFolderPath))
        {
            await MoveIncompleteContainersAsync(
                [.. db.Games.Where(g => rebuiltContainers.Contains(g.Name))],
                rebuildSettings,
                ct);
        }

        reportProgress(new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 100, Status = RebuildStatus.Completed, Results = new DatabaseRebuildResults() });

        return rebuildResults;

        static async Task<Tuple<RomGame, string[]>[]> GetIncompleteContainersAsync(RomGame[] games, RebuildSettings rebuildSettings, CancellationToken ct)
        {
            var result = new List<Tuple<RomGame, string[]>>();
            FileInformation[]? rebuiltFiles = null;

            foreach (var game in games)
            {
                ct.ThrowIfCancellationRequested();

                rebuiltFiles ??= await GetAllFilesAsync(rebuildSettings.TargetFolderPath, SearchOption.AllDirectories, ct);

                var rebuiltGameFiles = rebuiltFiles.Where(fi => fi.ContainerName == game.Name).ToArray();
                if (rebuiltGameFiles.Length == 0)
                {
                    // No files found for this game, unusual since
                    // we only process games that have had a rom rebuilt.
                    Debug.Assert(false, $"No files found for game {game.Name} in target folder {rebuildSettings.TargetFolderPath}.");
                    continue;
                }

                var allRomsFound = game.Roms.All(rom => rebuiltGameFiles.Any(gf => gf.FileRelativePath == rom.Name));
                if (allRomsFound)
                {
                    // This game is complete, so no need to process it further.
                    continue;
                }

                var containerAbsolutePaths = rebuiltGameFiles.Select(rom => rom.ContainerAbsolutePath).Distinct().ToArray();
                result.Add(Tuple.Create(game, containerAbsolutePaths));
            }

            return [.. result];
        }

        static async Task TorrentzipIncompleteContainersAsync(RomGame[] games, RebuildSettings rebuildSettings, CancellationToken ct)
        {
            var incompleteContainers = await GetIncompleteContainersAsync(games, rebuildSettings, ct);
            foreach (var incompleteContainer in incompleteContainers)
            {
                if (incompleteContainer.Item2.Length > 1)
                {
                    // There's more than one container for this game, leave them as is.
                    continue;
                }

                if (rebuildSettings.TargetContainerType == KnownContainerTypes.TorrentSevenZip)
                {
                    foreach (var containerAbsolutePath in incompleteContainer.Item2)
                    {
                        if (Path.GetExtension(containerAbsolutePath).Equals(".zip", StringComparison.OrdinalIgnoreCase) &&
                            File.Exists(Path.ChangeExtension(containerAbsolutePath, ".7z")))
                        {
                            // We have both a .zip and an existing .7z file, so we cannot
                            // convert this .zip into a .7z torrent7z file.
                            continue;
                        }

                        await TorrentSevenZipper.TorrentZipAsync(containerAbsolutePath, ct);
                    }
                }
                else if (rebuildSettings.TargetContainerType == KnownContainerTypes.TorrentZip)
                {
                    foreach (var containerAbsolutePath in incompleteContainer.Item2)
                    {
                        await TorrentZipper.TorrentZipAsync(containerAbsolutePath, ct);
                    }
                }
            }
        }

        static async Task MoveIncompleteContainersAsync(RomGame[] games, RebuildSettings rebuildSettings, CancellationToken ct)
        {
            var incompleteContainers = await GetIncompleteContainersAsync(games, rebuildSettings, ct);
            foreach (var incompleteContainer in incompleteContainers)
            {
                foreach (var containerAbsolutePath in incompleteContainer.Item2)
                {
                    ct.ThrowIfCancellationRequested();

                    if (Directory.Exists(containerAbsolutePath))
                    {
                        Directory.CreateDirectory(rebuildSettings.TargetIncompleteFolderPath);
                        Directory.Move(containerAbsolutePath, Path.Combine(rebuildSettings.TargetIncompleteFolderPath, Path.GetFileName(containerAbsolutePath)));
                    }
                    else if (File.Exists(containerAbsolutePath))
                    {
                        Directory.CreateDirectory(rebuildSettings.TargetIncompleteFolderPath);
                        var targetFilePath = Path.Combine(rebuildSettings.TargetIncompleteFolderPath, Path.GetFileName(containerAbsolutePath));
                        File.Move(containerAbsolutePath, targetFilePath);
                    }
                    else
                    {
                        Debug.Assert(false, $"Container {containerAbsolutePath} does not exist, cannot move to incomplete folder.");
                    }
                }
            }
        }

        static async Task RebuildFileAsync(Action<RebuildEventArgs> reportProgress, DatabaseFile databaseFile, FileInformation sourceFile, RomFile[] matchedRoms, string targetFolderPath, string targetContainerType, bool removeSource, CancellationToken ct)
        {
            var fileMoniker = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath, sourceFile.IsFromOfflineStorage);

            var sourceContainer = ContainerExtensionProvider.GetContainer(sourceFile.ContainerAbsolutePath);
            if (sourceContainer != null)
            {
                foreach (var matchedRom in matchedRoms)
                {
                    ct.ThrowIfCancellationRequested();

                    if (await sourceContainer.ExistsAsync(sourceFile, ct))
                    {
                        var expectedTargetFiles = matchedRom.ParentGame.Roms.Select(rom => rom.Name).ToArray();

                        var copier = FileCopierExtensionProvider.GetCopier(sourceFile, targetContainerType, expectedTargetFiles);
                        if (copier != null)
                        {
                            var romMoniker = new RomMoniker(matchedRom.ParentGame.Name, matchedRom.Name);

                            reportProgress(new RebuildEventArgs { DatabaseFile = databaseFile, ProgressPercentage = 0, Status = RebuildStatus.Building, Results = new DatabaseRebuildResults(), BuildingRom = romMoniker });

                            await copier.CopyAsync(sourceFile, targetFolderPath, removeSource, matchedRoms.Length == 1, matchedRom.ParentGame.Name, matchedRom.Name, expectedTargetFiles, ct);

                            var result = new RomAndFileMoniker(romMoniker, fileMoniker);
                            reportProgress(new RebuildEventArgs { DatabaseFile = databaseFile, ProgressPercentage = 0, Status = RebuildStatus.Scanning, Results = new DatabaseRebuildResults() { Matched = [result] } });
                        }
                    }
                }

                ct.ThrowIfCancellationRequested();

                if (removeSource && await sourceContainer.ExistsAsync(sourceFile, ct))
                {
                    await sourceContainer.RemoveAsync(sourceFile, ct);
                }
            }
        }

        static async Task CheckForMissingCueFilesAsync(Action<RebuildEventArgs> reportProgress, DatabaseFile file, RomDatabase db, RebuildSettings rebuildSettings, string[] cueFolders, CancellationToken ct)
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

                rebuiltFiles ??= await GetAllFilesAsync(rebuildSettings.TargetFolderPath, SearchOption.AllDirectories, ct);
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
                cueSourceFiles ??= await GetAllCueFilesAsync(cueFolders, ct);

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
                    await EnsureChecksumsAsync(reportProgress, file, cueSourceFile, rebuildSettings, ct);
                    if (!Scanner.DoesChecksumMatch(cueRom, cueSourceFile, !rebuildSettings.ForceCalculateChecksums))
                    {
                        // Cue file checksum does not match, we cannot rebuild it
                        continue;
                    }

                    // We can rebuild the cue file
                    Directory.CreateDirectory(rebuildSettings.TargetFolderPath);
                    await RebuildFileAsync(reportProgress, file, cueSourceFile, [cueRom], rebuildSettings.TargetFolderPath, rebuildSettings.TargetContainerType, removeSource: false, ct: ct);
                }
            }
        }

        static async Task<FileInformation[]> GetAllCueFilesAsync(string[] cueFolders, CancellationToken ct)
        {
            var result = new List<FileInformation>();
            foreach (var cueFolder in cueFolders)
            {
                var children = await GetAllFilesAsync(cueFolder, SearchOption.AllDirectories, ct);
                result.AddRange(children.Where(file => file.FileRelativePath.EndsWith(".cue", StringComparison.OrdinalIgnoreCase)));
            }

            return [.. result];
        }

        static async Task EnsureChecksumsAsync(Action<RebuildEventArgs> reportProgress, DatabaseFile file, FileInformation sourceFile, RebuildSettings rebuildSettings, CancellationToken ct)
        {
            if (!sourceFile.AllChecksumsCalculated)
            {
                var container = ContainerExtensionProvider.GetContainer(sourceFile.ContainerAbsolutePath);
                if (container != null)
                {
                    if (rebuildSettings.ForceCalculateChecksums || sourceFile.ReportedCRC32.Length == 0)
                    {
                        reportProgress(new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 0, Status = RebuildStatus.Hashing, Results = new DatabaseRebuildResults(), HashingFile = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath, sourceFile.IsFromOfflineStorage) });

                        await container.CalculateChecksumsAsync(sourceFile, ct);

                        reportProgress(new RebuildEventArgs { DatabaseFile = file, ProgressPercentage = 0, Status = RebuildStatus.Scanning, Results = new DatabaseRebuildResults() });
                    }
                }

                ct.ThrowIfCancellationRequested();
            }
        }
    }

    public static async Task CreateDatabaseAsync(
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

            var children = await GetAllFilesAsync(folderInfo.FullName, SearchOption.AllDirectories, ct);
            foreach (var fileInformation in children)
            {
                ct.ThrowIfCancellationRequested();

                await AddRomToGameAsync(reportProgress, fileInformation, game, settings.ForceCalculateChecksums, ct);
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

                var children = await compressedContainer.GetAllFilesAsync(fileInfo.FullName, SearchOption.AllDirectories, ct);
                foreach (var fileInformation in children)
                {
                    ct.ThrowIfCancellationRequested();

                    await AddRomToGameAsync(reportProgress, fileInformation, game, settings.ForceCalculateChecksums, ct);
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

        static async Task AddRomToGameAsync(Action<DatabaseCreateEventArgs> reportProgress, FileInformation sourceFile, RomGame game, bool forceCalculateChecksums, CancellationToken ct)
        {
            await EnsureChecksumsAsync(reportProgress, sourceFile, forceCalculateChecksums, ct);

            var file = new RomFile(game)
            {
                Name = sourceFile.FileRelativePath,
                Size = sourceFile.Size,
                CRC32 = sourceFile.CRC32,
                MD5 = sourceFile.MD5,
                SHA1 = sourceFile.SHA1,
            };
            game.Roms.Add(file);

            var fileMoniker = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath, sourceFile.IsFromOfflineStorage);
            var romMoniker = new RomMoniker(game.Name, file.Name);
            var result = new RomAndFileMoniker(romMoniker, fileMoniker);

            reportProgress(new DatabaseCreateEventArgs { ProgressPercentage = 0, Status = DatabaseCreateStatus.Scanning, Results = new DatabaseCreateResults() { Added = [result] } });
        }

        static async Task EnsureChecksumsAsync(Action<DatabaseCreateEventArgs> reportProgress, FileInformation sourceFile, bool forceCalculateChecksums, CancellationToken ct)
        {
            if (!sourceFile.AllChecksumsCalculated)
            {
                var container = ContainerExtensionProvider.GetContainer(sourceFile.ContainerAbsolutePath);
                if (container != null)
                {
                    if (forceCalculateChecksums || sourceFile.ReportedCRC32.Length == 0)
                    {
                        reportProgress(new DatabaseCreateEventArgs { ProgressPercentage = 0, Status = DatabaseCreateStatus.Hashing, Results = new DatabaseCreateResults(), HashingFile = new FileMoniker(sourceFile.ContainerAbsolutePath, sourceFile.ContainerName, sourceFile.FileRelativePath, sourceFile.IsFromOfflineStorage) });

                        await container.CalculateChecksumsAsync(sourceFile, ct);

                        reportProgress(new DatabaseCreateEventArgs { ProgressPercentage = 0, Status = DatabaseCreateStatus.Scanning, Results = new DatabaseCreateResults() });
                    }
                }
            }
        }
    }

    private static Task<FileInformation[]> GetAllFilesAsync(string folderPath, SearchOption option, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(folderPath);

        return FolderContainer.GetAllFilesAsync(folderPath, option, ct);
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
