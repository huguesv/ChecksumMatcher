// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumMatcher.Core.Contracts.Services;
using Woohoo.ChecksumMatcher.Core.Helpers;
using Woohoo.IO.AbstractFileSystem;

public class Scanner : IScanner
{
    private bool cancel;

    public event EventHandler<ScannerProgressEventArgs>? Progress;

    public static bool DoesChecksumMatch(RomFile rom, FileInformation file, bool useReportedCrcIfAvailable, RomHeader header)
    {
        ArgumentNullException.ThrowIfNull(rom);
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(header);

        var matchCrc = false;
        var matchMd5 = false;
        var matchSha1 = false;
        var matchSha256 = false;

        if (useReportedCrcIfAvailable && header.Rules.Count == 0)
        {
            if (rom.CRC32.Length > 0 && file.ReportedCRC32.Length > 0)
            {
                return ByteArrayUtility.AreEqual(rom.CRC32, file.ReportedCRC32);
            }
        }

        if (rom.MD5.Length > 0 && file.MD5.Length > 0)
        {
            if (!ByteArrayUtility.AreEqual(rom.MD5, file.MD5))
            {
                return false;
            }

            matchMd5 = true;
        }

        if (rom.SHA1.Length > 0 && file.SHA1.Length > 0)
        {
            if (!ByteArrayUtility.AreEqual(rom.SHA1, file.SHA1))
            {
                return false;
            }

            matchSha1 = true;
        }

        if (rom.SHA256.Length > 0 && file.SHA256.Length > 0)
        {
            if (!ByteArrayUtility.AreEqual(rom.SHA256, file.SHA256))
            {
                return false;
            }

            matchSha256 = true;
        }

        if (rom.CRC32.Length > 0 && file.CRC32.Length > 0)
        {
            if (!ByteArrayUtility.AreEqual(rom.CRC32, file.CRC32))
            {
                return false;
            }

            matchCrc = true;
        }

        return matchCrc || matchMd5 || matchSha1 || matchSha256;
    }

    public void Cancel()
    {
        this.cancel = true;
    }

    public ScannerResult Scan(RomDatabase db, Storage storage, ScanOptions options)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(options);

        if (db.Header.Rules.Count > 0)
        {
            throw new NotSupportedException("Database contains headers, which are not supported.");
        }

        var scannerResult = new ScannerResult();
        var usedButIncorrectName = new List<FileInformation>();

        this.cancel = false;

        // Enumerate all files on disk, this can take a long time
        var files = new List<FileInformation>(this.EnumerateFiles(storage));

        if (this.cancel)
        {
            this.OnProgress(null, null, ScannerProgress.Canceled);
            return scannerResult;
        }

        // If roms have headers, then we have to calculate checksums first, this will take care
        // of updating the data block size, which is different from the file size when roms have headers.
        if (db.Header.Rules.Count > 0)
        {
            foreach (var file in files)
            {
                this.CalculateChecksums(file, options.ForceCalculateChecksums, db.Header);
            }
        }

        foreach (var rom in db.GetAllRoms())
        {
            if (this.cancel)
            {
                this.OnProgress(null, null, ScannerProgress.Canceled);
                return scannerResult;
            }

            // Find all files that match the rom size
            var sizeMatches = files.Where(file => file.DataBlockSize == rom.Size).ToArray();
            if (sizeMatches.Length > 0)
            {
                if (db.Header.Rules.Count == 0)
                {
                    // Calculate checksums of all files that match the rom size, this can take a long time
                    foreach (var sizeMatch in sizeMatches)
                    {
                        this.CalculateChecksums(sizeMatch, options.ForceCalculateChecksums, db.Header);
                    }
                }

                if (this.cancel)
                {
                    this.OnProgress(null, null, ScannerProgress.Canceled);
                    return scannerResult;
                }

                // Find all files that match the checksums
                var checksumMatches = sizeMatches.Where(file => DoesChecksumMatch(rom, file, !options.ForceCalculateChecksums, db.Header)).ToArray();
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

                            scannerResult.MatchedRoms.Add(new ScannerMatchedRomResult(rom, gameMatch));

                            this.OnProgress(rom, gameMatch, ScannerProgress.PerfectMatch);
                        }
                        else
                        {
                            // Wrong file name, but correct container name
                            scannerResult.WrongNameRoms.Add(new ScannerWrongNameRomResult(rom, gameMatch));

                            usedButIncorrectName.Add(gameMatch);

                            this.OnProgress(rom, gameMatch, ScannerProgress.IncorrectContainerOrFileName);
                        }
                    }
                    else
                    {
                        // Wrong container name (there may be multiple files matching size and crc, we'll pick the first one)
                        scannerResult.WrongNameRoms.Add(new ScannerWrongNameRomResult(rom, checksumMatches[0]));

                        usedButIncorrectName.AddRange(checksumMatches);

                        this.OnProgress(rom, checksumMatches[0], ScannerProgress.IncorrectContainerOrFileName);
                    }
                }
                else
                {
                    scannerResult.MissingRoms.Add(new ScannerMissingRomResult(rom));

                    this.OnProgress(rom, null, ScannerProgress.Missing);
                }
            }
            else
            {
                scannerResult.MissingRoms.Add(new ScannerMissingRomResult(rom));

                this.OnProgress(rom, null, ScannerProgress.Missing);
            }
        }

        var unused = files.Where(result => !usedButIncorrectName.Contains(result)).ToArray();
        foreach (var result in unused)
        {
            scannerResult.UnusedFiles.Add(new ScannerUnusedFileResult(result));
        }

        this.OnProgress(scannerResult, ScannerProgress.Finished);

        return scannerResult;
    }

    private FileInformation[] EnumerateFiles(Storage storage)
    {
        this.OnProgress(null, null, ScannerProgress.EnumeratingFilesStart);

        var files = new List<FileInformation>();

        foreach (var folderPath in storage.FolderPaths)
        {
            files.AddRange(FileSystem.GetAllFiles(folderPath, SearchOption.AllDirectories));
        }

        foreach (var indexedStorage in storage.OfflineFolders)
        {
            files.AddRange(FileSystem.GetAllFiles(indexedStorage, SearchOption.AllDirectories));
        }

        this.OnProgress(null, null, ScannerProgress.EnumeratingFilesEnd);

        return files.ToArray();
    }

    private void CalculateChecksums(FileInformation file, bool forceCalculateChecksums, RomHeader header)
    {
        if (!file.AllChecksumsCalculated)
        {
            var container = ContainerExtensionProvider.GetContainer(file.ContainerAbsolutePath);
            if (container != null)
            {
                if (forceCalculateChecksums || file.ReportedCRC32.Length == 0 || header.Rules.Count > 0)
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

    private void OnProgress(ScannerResult result, ScannerProgress type)
    {
        if (this.Progress != null)
        {
            var args = new ScannerProgressEventArgs
            {
                Result = result,
                Status = type,
            };

            this.Progress(this, args);
        }
    }
}
