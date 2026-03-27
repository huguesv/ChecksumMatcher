// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Woohoo.ChecksumDatabase.Model;

internal sealed class DatabaseCleanTransform
{
    public static RomDatabase? TransformDatabase(RomDatabase? db, RomNameConflictFixMode fixMode, INotificationSink sink)
    {
        if (db is null)
        {
            return null;
        }

        db = DetectDuplicates(db, fixMode, sink);

        return db;
    }

    private static RomDatabase DetectDuplicates(RomDatabase db, RomNameConflictFixMode fixMode, INotificationSink sink)
    {
        var gamesToRemove = new List<RomGame>();
        foreach (var gameGroup in db.Games.GroupBy(game => game.Name))
        {
            if (gameGroup.Count() > 1)
            {
                sink.Warning(gameGroup.First().Name, Notifications.Warnings.DuplicateGameName);

                int index = 0;
                foreach (var game in gameGroup.Skip(1))
                {
                    game.Name = $"{game.Name}_{index}";
                    index++;
                }
            }
        }

        foreach (var game in gamesToRemove)
        {
            db.Games.Remove(game);
        }

        foreach (var game in db.Games)
        {
            DetectRomDuplicates(game, fixMode, sink);
        }

        return db;
    }

    private static void DetectRomDuplicates(RomGame game, RomNameConflictFixMode fixMode, INotificationSink sink)
    {
        var romsToRemove = new List<RomFile>();
        foreach (var romGroup in game.Roms.GroupBy(rom => rom.Name))
        {
            if (romGroup.Count() > 1)
            {
                switch (fixMode)
                {
                    case RomNameConflictFixMode.None:
                        break;
                    case RomNameConflictFixMode.AppendInteger:
                        FixDuplicatesAppendIndex(sink, romsToRemove, romGroup);
                        break;
                    case RomNameConflictFixMode.AppendCrc32:
                        FixDuplicatesAppendCrc32(sink, romsToRemove, romGroup);
                        break;
                    default:
                        Debug.Assert(false, $"Unsupported fix mode: {fixMode}");
                        break;
                }
            }
        }

        foreach (var rom in romsToRemove)
        {
            game.Roms.Remove(rom);
        }
    }

    private static void FixDuplicatesAppendIndex(INotificationSink sink, List<RomFile> romsToRemove, IGrouping<string, RomFile> romGroup)
    {
        // Append index to every conflict starting with the second rom.
        // First rom wins, it keeps its name.
        // This is the way that RomVault handles duplicate rom names.
        // The file extension of the renamed rom(s) is unchanged.
        // This matches RomVault behavior.
        var firstRom = romGroup.First();
        int index = 0;
        foreach (var rom in romGroup.Skip(1))
        {
            if (IsRomContentIdentical(firstRom, rom, out var toRemove))
            {
                sink.Warning(toRemove.Name, Notifications.Warnings.DuplicateRomName);
                romsToRemove.Add(toRemove);
            }
            else
            {
                sink.Warning(firstRom.Name, Notifications.Warnings.DuplicateRomNameDifferentContent);

                string ext = Path.GetExtension(rom.Name);
                if (string.IsNullOrEmpty(ext))
                {
                    rom.Name = $"{rom.Name}_{index}";
                }
                else
                {
                    string name = rom.Name.Substring(0, rom.Name.Length - ext.Length);
                    rom.Name = $"{name}_{index}{ext}";
                }

                index++;
            }
        }
    }

    private static void FixDuplicatesAppendCrc32(INotificationSink sink, List<RomFile> romsToRemove, IGrouping<string, RomFile> romGroup)
    {
        // Append CRC32 to every conflict starting with the first rom.
        // Last rom wins, it keeps its name.
        // The file extension of the renamed rom(s) is changed.
        // This matches ClrMamePro behavior.
        var lastRom = romGroup.Last();
        foreach (var rom in romGroup.SkipLast(1))
        {
            if (IsRomContentIdentical(lastRom, rom, out var toRemove))
            {
                sink.Warning(toRemove.Name, Notifications.Warnings.DuplicateRomName);
                romsToRemove.Add(toRemove);
            }
            else
            {
                sink.Warning(lastRom.Name, Notifications.Warnings.DuplicateRomNameDifferentContent);
                rom.Name = $"{rom.Name}_{ChecksumConversion.ToHex(rom.CRC32)}";
            }
        }
    }

    private static bool IsRomContentIdentical(RomFile a, RomFile b, [NotNullWhen(true)] out RomFile? toRemove)
    {
        toRemove = null;

        if (a.Size != b.Size)
        {
            return false;
        }

        if (a.CRC32.Length > 0 && b.CRC32.Length > 0 && !a.CRC32.SequenceEqual(b.CRC32))
        {
            return false;
        }

        if (a.MD5.Length > 0 && b.MD5.Length > 0 && !a.MD5.SequenceEqual(b.MD5))
        {
            return false;
        }

        if (a.SHA1.Length > 0 && b.SHA1.Length > 0 && !a.SHA1.SequenceEqual(b.SHA1))
        {
            return false;
        }

        if (a.SHA256.Length > 0 && b.SHA256.Length > 0 && !a.SHA256.SequenceEqual(b.SHA256))
        {
            return false;
        }

        // They are considered identical, but one may be better than the other
        // If one has more checksums type specified, keep that one.
        // Otherwise, remove the second one.
        int countA =
            (a.CRC32.Length > 0 ? 1 : 0) +
            (a.MD5.Length > 0 ? 1 : 0) +
            (a.SHA1.Length > 0 ? 1 : 0) +
            (a.SHA256.Length > 0 ? 1 : 0);

        int countB =
            (b.CRC32.Length > 0 ? 1 : 0) +
            (b.MD5.Length > 0 ? 1 : 0) +
            (b.SHA1.Length > 0 ? 1 : 0) +
            (b.SHA256.Length > 0 ? 1 : 0);

        toRemove = countA >= countB ? b : a;

        return true;
    }
}
