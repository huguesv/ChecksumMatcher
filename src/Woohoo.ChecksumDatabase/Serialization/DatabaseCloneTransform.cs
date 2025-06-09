// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Woohoo.ChecksumDatabase.Model;

internal sealed class DatabaseCloneTransform
{
    public static RomDatabase? TransformDatabase(RomDatabase? db, CloneMode cloneMode)
    {
        if (db is null)
        {
            return null;
        }

        switch (cloneMode)
        {
            case CloneMode.Split:
                return SplitClone(db);
            case CloneMode.NonMerge:
                return NonMergeClone(db);
            case CloneMode.Merge:
                return MergeClone(db, cloneInChildFolder: false);
            case CloneMode.MergeCloneInChildFolder:
                return MergeClone(db, cloneInChildFolder: true);
            default:
                Debug.Assert(false, "Unexpected CloneMode");
                return db;
        }
    }

    private static RomDatabase SplitClone(RomDatabase db)
    {
        // TODO: Implement split clone handling (remove Merge entries)
        return db;
    }

    private static RomDatabase NonMergeClone(RomDatabase db)
    {
        // TODO: Implement non-merge clone handling
        return db;
    }

    private static RomDatabase MergeClone(RomDatabase oldDatabase, bool cloneInChildFolder)
    {
        var newDatabase = new RomDatabase
        {
            Name = oldDatabase.Name,
            Author = oldDatabase.Author,
            Category = oldDatabase.Category,
            Comment = oldDatabase.Comment,
            Date = oldDatabase.Date,
            Description = oldDatabase.Description,
            Email = oldDatabase.Email,
            EmulatorName = oldDatabase.EmulatorName,
            EmulatorVersion = oldDatabase.EmulatorVersion,
            Homepage = oldDatabase.Homepage,
            Url = oldDatabase.Url,
            Version = oldDatabase.Version,
        };

        Dictionary<string, RomGame> newGames = [];
        Dictionary<RomGame, List<MergedRom>> newRoms = [];
        Dictionary<RomGame, List<MergedDisk>> newDisks = [];

        var oldDatabaseIndex = new DatabaseIndex(oldDatabase);
        foreach (var game in oldDatabaseIndex.AllGames)
        {
            _ = ProcessGame(newDatabase, newGames, newRoms, newDisks, game, oldDatabaseIndex);
        }

        foreach (var game in newGames)
        {
            if (newRoms.TryGetValue(game.Value, out var mergedRoms))
            {
                foreach (var mergedRom in mergedRoms)
                {
                    if (mergedRom.ShouldSkip)
                    {
                        continue;
                    }

                    bool hasConflictingNameWithDifferentChecksums = false;

                    var identicalNames = mergedRoms.Where(r => r.Name == mergedRom.Name && !r.ShouldSkip).ToList();
                    if (identicalNames.Count > 1)
                    {
                        // Are there identical names with different content?
                        if (identicalNames.Any(r => !IsRomContentIdentical(r, mergedRom)))
                        {
                            hasConflictingNameWithDifferentChecksums = true;
                        }
                        else
                        {
                            // Multiple roms all with the same name, just keep this one and mark the others to be skipped
                            foreach (var r in identicalNames)
                            {
                                if (!ReferenceEquals(r, mergedRom))
                                {
                                    r.ShouldSkip = true;
                                }
                            }
                        }
                    }

                    var identicalContent = mergedRoms.Where(r => IsRomContentIdentical(r, mergedRom) && !r.ShouldSkip).ToList();
                    if (identicalContent.Count > 1)
                    {
                        // Multiple roms all with the same content but different names
                        // Priority is non-merge, no subfolder, then alphanumeric order.
                        identicalContent.Sort(MergedRomNameComparer.Instance.Compare);

                        var romToKeep = identicalContent
                            .Where(r => string.IsNullOrEmpty(r.Merge))
                            .FirstOrDefault() ?? identicalContent.First();

                        foreach (var r in identicalContent)
                        {
                            if (!ReferenceEquals(r, romToKeep))
                            {
                                r.ShouldSkip = true;
                            }
                        }
                    }

                    // This may have been updated above, so check it again
                    if (mergedRom.ShouldSkip)
                    {
                        continue;
                    }

                    var romIsFromClone = mergedRom.OriginalParentGameName != game.Value.Name;

                    var romName = romIsFromClone && (cloneInChildFolder || hasConflictingNameWithDifferentChecksums)
                        ? Path.Combine(mergedRom.OriginalParentGameName, mergedRom.Name)
                        : mergedRom.Name;

                    game.Value.Roms.Add(new RomFile(game.Value)
                    {
                        Name = romName,
                        Size = mergedRom.Size,
                        CRC32 = mergedRom.CRC32,
                        MD5 = mergedRom.MD5,
                        SHA1 = mergedRom.SHA1,
                        SHA256 = mergedRom.SHA256,
                        Date = mergedRom.Date,
                        Merge = mergedRom.Merge,
                        Status = mergedRom.Status,
                    });
                }
            }

            if (newDisks.TryGetValue(game.Value, out var mergedDisks))
            {
                foreach (var mergedDisk in mergedDisks)
                {
                    game.Value.Disks.Add(new RomDisk(game.Value)
                    {
                        Name = cloneInChildFolder ? Path.Combine(mergedDisk.OriginalParentGameName, mergedDisk.Name) : mergedDisk.Name,
                        MD5 = mergedDisk.MD5,
                        SHA1 = mergedDisk.SHA1,
                        Merge = mergedDisk.Merge,
                        Status = mergedDisk.Status,
                    });
                }
            }

            if (game.Value.Roms.Count > 0 || game.Value.Disks.Count > 0)
            {
                newDatabase.Games.Add(game.Value);
            }
        }

        return newDatabase;
    }

#if false
    private static bool IsDiskContentIdentical(MergedDisk a, MergedDisk b)
    {
        if (!a.MD5.SequenceEqual(b.MD5))
        {
            return false;
        }

        if (!a.SHA1.SequenceEqual(b.SHA1))
        {
            return false;
        }

        return true;
    }
#endif
    private static bool IsRomContentIdentical(MergedRom a, MergedRom b)
    {
        if (a.Size != b.Size)
        {
            return false;
        }

        if (!a.CRC32.SequenceEqual(b.CRC32))
        {
            return false;
        }

        if (!a.MD5.SequenceEqual(b.MD5))
        {
            return false;
        }

        if (!a.SHA1.SequenceEqual(b.SHA1))
        {
            return false;
        }

        if (!a.SHA256.SequenceEqual(b.SHA256))
        {
            return false;
        }

        return true;
    }

    private static RomGame? ProcessGame(RomDatabase newDatabase, Dictionary<string, RomGame> newGames, Dictionary<RomGame, List<MergedRom>> newRoms, Dictionary<RomGame, List<MergedDisk>> newDisks, RomGame oldGame, DatabaseIndex oldDatabaseIndex)
    {
        if (newGames.TryGetValue(oldGame.Name, out var existingGame))
        {
            return existingGame;
        }

        if (!string.IsNullOrEmpty(oldGame.CloneOf))
        {
            var oldParentGame = oldDatabaseIndex.FindGameByName(oldGame.CloneOf);
            if (oldParentGame is not null)
            {
                var newParentGame = ProcessGame(newDatabase, newGames, newRoms, newDisks, oldParentGame, oldDatabaseIndex);
                if (newParentGame is not null)
                {
                    if (newRoms.TryGetValue(newParentGame, out var parentRoms))
                    {
                        foreach (var rom in oldGame.Roms.Where(r => string.IsNullOrEmpty(r.Merge)))
                        {
                            parentRoms.Add(CreateMergedRom(rom));
                        }
                    }

                    if (newDisks.TryGetValue(newParentGame, out var parentDisks))
                    {
                        foreach (var disk in oldGame.Disks.Where(d => string.IsNullOrEmpty(d.Merge)))
                        {
                            parentDisks.Add(CreateMergedDisk(disk));
                        }
                    }
                }

                return newParentGame;
            }

            return null;
        }

        if (!string.IsNullOrEmpty(oldGame.RomOf))
        {
            var oldParentGame = oldDatabaseIndex.FindGameByName(oldGame.RomOf);
            if (oldParentGame is not null && !oldParentGame.IsBios)
            {
                var newParentGame = ProcessGame(newDatabase, newGames, newRoms, newDisks, oldParentGame, oldDatabaseIndex);
                if (newParentGame is not null)
                {
                    if (newRoms.TryGetValue(newParentGame, out var parentRoms))
                    {
                        foreach (var rom in oldGame.Roms.Where(r => string.IsNullOrEmpty(r.Merge)))
                        {
                            parentRoms.Add(CreateMergedRom(rom));
                        }
                    }

                    if (newDisks.TryGetValue(newParentGame, out var parentDisks))
                    {
                        foreach (var disk in oldGame.Disks.Where(d => string.IsNullOrEmpty(d.Merge)))
                        {
                            parentDisks.Add(CreateMergedDisk(disk));
                        }
                    }
                }

                return newParentGame;
            }
        }

        var newGame = CreateGame(newDatabase, oldGame);
        newGames[newGame.Name] = newGame;

        List<MergedRom> mergedRoms = [];
        newRoms[newGame] = mergedRoms;
        foreach (var oldRom in oldGame.Roms.Where(r => string.IsNullOrEmpty(r.Merge)))
        {
            mergedRoms.Add(CreateMergedRom(oldRom));
        }

        List<MergedDisk> mergedDisks = [];
        newDisks[newGame] = mergedDisks;
        foreach (var oldDisk in oldGame.Disks.Where(d => string.IsNullOrEmpty(d.Merge)))
        {
            mergedDisks.Add(CreateMergedDisk(oldDisk));
        }

        return newGame;
    }

    private static MergedRom CreateMergedRom(RomFile rom)
    {
        return new MergedRom
        {
            OriginalParentGameName = rom.ParentGame.Name,
            Name = rom.Name,
            Size = rom.Size,
            CRC32 = rom.CRC32,
            MD5 = rom.MD5,
            SHA1 = rom.SHA1,
            SHA256 = rom.SHA256,
            Date = rom.Date,
            Merge = rom.Merge,
            Status = rom.Status,
        };
    }

    private static MergedDisk CreateMergedDisk(RomDisk disk)
    {
        return new MergedDisk
        {
            OriginalParentGameName = disk.ParentGame.Name,
            Name = disk.Name,
            MD5 = disk.MD5,
            SHA1 = disk.SHA1,
            Merge = disk.Merge,
            Status = disk.Status,
        };
    }

    private static RomGame CreateGame(RomDatabase newDatabase, RomGame oldGame)
    {
        var newGame = new RomGame(newDatabase)
        {
            Name = oldGame.Name,
            Description = oldGame.Description,
            Year = oldGame.Year,
            Manufacturer = oldGame.Manufacturer,
            IsBios = oldGame.IsBios,
            SourceFile = oldGame.SourceFile,
            RomOf = oldGame.RomOf,
            CloneOf = oldGame.CloneOf,
            SampleOf = oldGame.SampleOf,
            Board = oldGame.Board,
            RebuildTo = oldGame.RebuildTo,
        };

        foreach (var comment in oldGame.Comments)
        {
            newGame.Comments.Add(comment);
        }

        foreach (var detail in oldGame.Details)
        {
            newGame.Details[detail.Key] = detail.Value;
        }

        foreach (var release in oldGame.Releases)
        {
            newGame.Releases.Add(new RomRelease(newGame)
            {
                Name = release.Name,
                Region = release.Region,
                Language = release.Language,
                Date = release.Date,
                IsDefault = release.IsDefault,
            });
        }

        foreach (var biosSet in oldGame.BiosSets)
        {
            newGame.BiosSets.Add(new RomBiosSet(newGame)
            {
                Name = biosSet.Name,
                Description = biosSet.Description,
                IsDefault = biosSet.IsDefault,
            });
        }

        return newGame;
    }

#if false
        public static RomDatabase TransformMergeClone(RomDatabase db, bool cloneInChildFolder)
        {
            var gamesToRemove = new List<RomGame>();

            foreach (var game in db.Games)
            {
                RemoveMergeRoms(game);

                var cloneOfGames = db.Games.Where(g => string.Equals(g.CloneOf, game.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                var romOfGames = db.Games.Where(g => string.Equals(g.RomOf, game.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                // Clone Game Name, Clone Rom Name, New RomFile (with unmodified Rom Name)
                List<(string, string, RomFile)> romsCandidatesToAdd = [];

                foreach (var cloneOfGame in cloneOfGames)
                {
                    foreach (var cloneOfRom in cloneOfGame.Roms.Where(r => string.IsNullOrEmpty(r.Merge)))
                    {
                        romsCandidatesToAdd.Add((cloneOfGame.Name, cloneOfRom.Name, CreateRomFile(game, cloneOfRom)));
                    }

                    gamesToRemove.Add(cloneOfGame);
                }

                if (!game.IsBios)
                {
                    foreach (var romOfGame in romOfGames)
                    {
                        foreach (var romOfRom in romOfGame.Roms.Where(r => string.IsNullOrEmpty(r.Merge)))
                        {
                            romsCandidatesToAdd.Add((romOfGame.Name, romOfRom.Name, CreateRomFile(game, romOfRom)));
                        }

                        gamesToRemove.Add(romOfGame);
                    }
                }

                // Add all the roms to the game, if there are files with duplicate names but different size and sha1, keep both but change the rom Name to prepend the clone game name as a folder path
                // If there are files with the same name, size and sha1, keep only one (the first one)
                List<RomFile> romsToAdd = [];
                var groups = romsCandidatesToAdd.GroupBy(r => r.Item2, StringComparer.OrdinalIgnoreCase);
                foreach (var group in groups)
                {
                    var first = group.First();
                    if (group.All(g => IsSameRomSizeAndChecksums(g.Item3, first.Item3)))
                    {
                        if (cloneInChildFolder)
                        {
                            first.Item3.Name = Path.Combine(group.Key, first.Item3.Name);
                        }

                        // All roms are identical, keep only one
                        romsToAdd.Add(first.Item3);
                    }
                    else
                    {
                        // Roms are different, keep all but change the Name to prepend the clone game name as a folder path
                        foreach (var item in group)
                        {
                            item.Item3.Name = Path.Combine(group.Key, first.Item3.Name);
                            romsToAdd.Add(item.Item3);
                        }
                    }
                }

                foreach (var rom in romsToAdd)
                {
                    var existingRom = game.Roms.FirstOrDefault(r => IsSameRomSizeAndChecksums(r, rom));
                    if (existingRom is not null)
                    {
                        if (existingRom.Name.Length > rom.Name.Length)
                        {
                            // Replace with the shorter name
                            game.Roms.Remove(existingRom);
                            game.Roms.Add(rom);
                        }
                    }
                    else
                    {
                        game.Roms.Add(rom);
                    }
                }
            }

            foreach (var g in gamesToRemove)
            {
                db.Games.Remove(g);
            }

            return db;

            static bool IsSameRomSizeAndChecksums(RomFile romA, RomFile romB)
            {
                return romA.Size == romB.Size &&
                    romA.CRC32.SequenceEqual(romB.CRC32) &&
                    romA.MD5.SequenceEqual(romB.MD5) &&
                    romA.SHA1.SequenceEqual(romB.SHA1) &&
                    romA.SHA256.SequenceEqual(romB.SHA256);
            }

            static void RemoveMergeRoms(RomGame game)
            {
                var roms = game.Roms.Where(r => !string.IsNullOrEmpty(r.Merge)).ToList();
                foreach (var rom in roms)
                {
                    game.Roms.Remove(rom);
                }
            }

            static RomFile CreateRomFile(RomGame parentGame, RomFile sourceRom)
            {
                return new RomFile(parentGame)
                {
                    Name = sourceRom.Name,
                    Size = sourceRom.Size,
                    CRC32 = sourceRom.CRC32,
                    MD5 = sourceRom.MD5,
                    SHA1 = sourceRom.SHA1,
                    SHA256 = sourceRom.SHA256,
                    Date = sourceRom.Date,
                    Status = sourceRom.Status,
                };
            }
        }

        public static RomDatabase TransformMergeClone2(RomDatabase db, bool cloneInChildFolder)
        {
            var biosNames = db.Games.Where(g => g.IsBios).Select(g => g.Name).ToHashSet();
            var cloneGames = db.Games.Where(g => IsClone(g, biosNames)).ToList();
            var nameToGameDict = db.Games.ToDictionary(g => g.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var clone in cloneGames)
            {
                string parentName = GetParentName(clone);
                if (nameToGameDict.TryGetValue(parentName, out var parentGame))
                {
                    foreach (var cloneRom in clone.Roms.Where(r => !string.IsNullOrEmpty(r.Merge)))
                    {
                        var newName = cloneInChildFolder ? Path.Combine(clone.Name, cloneRom.Name) : cloneRom.Name;

                        if (!parentGame.Roms.Any(r => r.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                        {
                            var parentRom = new RomFile(parentGame)
                            {
                                Name = newName,
                                Size = cloneRom.Size,
                                CRC32 = cloneRom.CRC32,
                                MD5 = cloneRom.MD5,
                                SHA1 = cloneRom.SHA1,
                                SHA256 = cloneRom.SHA256,
                                Date = cloneRom.Date,
                                Status = cloneRom.Status,
                            };

                            var existingParentRom = parentGame.Roms.FirstOrDefault(r => r.CRC32.SequenceEqual(cloneRom.CRC32) && r.SHA1.SequenceEqual(cloneRom.SHA1) && r.Size == cloneRom.Size);
                            if (existingParentRom is not null)
                            {
                                if (existingParentRom.Name.Length > parentRom.Name.Length)
                                {
                                    // Replace with the shorter name
                                    parentGame.Roms.Remove(existingParentRom);
                                    parentGame.Roms.Add(parentRom);
                                }
                            }
                            else
                            {
                                parentGame.Roms.Add(parentRom);
                            }
                        }
                    }
                }
            }

            foreach (var clone in cloneGames)
            {
                db.Games.Remove(clone);
            }

            // Any remaining RomOf entries have a parent bios, and may have duplicate roms already present in the bios
            // Remove those duplicate roms
            foreach (var game in db.Games.Where(g => !string.IsNullOrEmpty(g.RomOf)))
            {
                var mergeRoms = game.Roms.Where(r => !string.IsNullOrEmpty(r.Merge)).ToList();
                foreach (var mergeRom in mergeRoms)
                {
                    game.Roms.Remove(mergeRom);
                }
            }

            return db;

            static bool IsClone(RomGame game, HashSet<string> biosNames)
            {
                if (!string.IsNullOrEmpty(game.CloneOf))
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(game.RomOf))
                {
                    return !biosNames.Contains(game.RomOf);
                }

                return false;
            }

            static string GetParentName(RomGame game)
            {
                if (!string.IsNullOrEmpty(game.CloneOf))
                {
                    return game.CloneOf;
                }

                if (!string.IsNullOrEmpty(game.RomOf))
                {
                    return game.RomOf;
                }

                return string.Empty;
            }
        }
#endif
}
