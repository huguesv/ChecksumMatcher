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

public class Rebuilder : IRebuilder
{
    private bool cancel;

    public event EventHandler<RebuilderProgressEventArgs>? Progress;

    public void Cancel()
    {
        this.cancel = true;
    }

    public RebuilderResult Rebuild(RomDatabase db, string sourceFolderPath, string targetFolderPath, RebuildOptions options)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentException.ThrowIfNullOrEmpty(sourceFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentNullException.ThrowIfNull(options);

        if (db.Header.Rules.Count > 0)
        {
            throw new NotSupportedException("Database contains headers, which are not supported.");
        }

        var rebuilderResult = new RebuilderResult();

        this.cancel = false;

        var roms = db.GetAllRoms();

        var all = this.EnumerateFiles(sourceFolderPath).ToList();

        if (this.cancel)
        {
            this.OnProgress(null, null, ScannerProgress.Canceled);
            return rebuilderResult;
        }

        foreach (var file in all)
        {
            if (this.cancel)
            {
                this.OnProgress(null, null, ScannerProgress.Canceled);
                return rebuilderResult;
            }

            this.CalculateChecksums(file, options.ForceCalculateChecksums, db.Header);

            if (this.cancel)
            {
                this.OnProgress(null, null, ScannerProgress.Canceled);

                return rebuilderResult;
            }

            var matchedRoms = roms.Where(rom => Scanner.DoesChecksumMatch(rom, file, !options.ForceCalculateChecksums, db.Header)).ToArray();
            if (matchedRoms.Length > 0)
            {
                Directory.CreateDirectory(targetFolderPath);
                this.RebuildFile(targetFolderPath, options.RemoveSource, options.TargetContainerType, file, matchedRoms);
            }
            else
            {
                this.OnProgress(null, file, ScannerProgress.Unused);
            }
        }

        if (this.cancel)
        {
            this.OnProgress(null, null, ScannerProgress.Canceled);

            return rebuilderResult;
        }

        this.OnProgress(rebuilderResult, ScannerProgress.Finished);

        return rebuilderResult;
    }

    private static string[] GetGameRomFiles(RomGame game)
    {
        var expectedTargetFiles = new List<string>();
        foreach (var rom in game.Roms)
        {
            expectedTargetFiles.Add(rom.Name);
        }

        return expectedTargetFiles.ToArray();
    }

    private FileInformation[] EnumerateFiles(string sourceFolderPath)
    {
        this.OnProgress(null, null, ScannerProgress.EnumeratingFilesStart);

        var files = FileSystem.GetAllFiles(sourceFolderPath, SearchOption.AllDirectories);

        this.OnProgress(null, null, ScannerProgress.EnumeratingFilesEnd);

        return files;
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

    private void RebuildFile(string targetFolderPath, bool removeSource, string targetContainerType, FileInformation file, RomFile[] matchedRoms)
    {
        var sourceContainer = ContainerExtensionProvider.GetContainer(file.ContainerAbsolutePath);
        if (sourceContainer != null)
        {
            foreach (var matchedRom in matchedRoms)
            {
                if (sourceContainer.Exists(file))
                {
                    var expectedTargetFiles = GetGameRomFiles(matchedRom.ParentGame);
                    var copier = FileCopierExtensionProvider.GetCopier(file, targetContainerType, expectedTargetFiles);
                    if (copier != null)
                    {
                        this.OnProgress(matchedRom, file, ScannerProgress.RebuildingRomStart);

                        _ = copier.Copy(file, targetFolderPath, removeSource, matchedRoms.Length == 1, matchedRom.ParentGame.Name, matchedRom.Name, expectedTargetFiles);

                        this.OnProgress(matchedRom, file, ScannerProgress.RebuildingRomEnd);
                    }
                }
            }

            if (removeSource && sourceContainer.Exists(file))
            {
                sourceContainer.Remove(file);
            }
        }
    }

    private void OnProgress(RomFile? rom, FileInformation? file, ScannerProgress type)
    {
        if (this.Progress != null)
        {
            var args = new RebuilderProgressEventArgs
            {
                Rom = rom,
                File = file,
                Status = type,
            };

            this.Progress(this, args);
        }
    }

    private void OnProgress(RebuilderResult result, ScannerProgress type)
    {
        if (this.Progress != null)
        {
            var args = new RebuilderProgressEventArgs
            {
                Result = result,
                Status = type,
            };

            this.Progress(this, args);
        }
    }
}
