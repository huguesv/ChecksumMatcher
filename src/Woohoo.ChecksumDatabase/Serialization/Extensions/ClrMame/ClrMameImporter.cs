// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

using System;
using System.Collections.Generic;
using System.Globalization;
using Woohoo.ChecksumDatabase.Model;

public class ClrMameImporter : IDatabaseImporter
{
    public ClrMameImporter()
    {
    }

    public bool CanImport(string text)
    {
        Requires.NotNull(text);

        var lines = text.Split('\n');
        var lineCount = lines.Length;
        var lineIndex = 0;
        while (lineIndex < lineCount)
        {
            if (lines[lineIndex].StartsWith("clrmamepro", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            lineIndex++;
        }

        return false;
    }

    public RomDatabase Import(string text, string workingFolderPath)
    {
        Requires.NotNull(text);
        Requires.NotNull(workingFolderPath);

        var db = new RomDatabase();

        var gameNames = new Dictionary<string, RomGame>();

        var lines = text.Split('\n');
        var lineCount = lines.Length;
        var lineIndex = 0;
        while (lineIndex < lineCount)
        {
            if (lines[lineIndex].StartsWith("clrmamepro", StringComparison.OrdinalIgnoreCase))
            {
                lineIndex = this.ReadHeader(workingFolderPath, db, lines, lineCount, lineIndex);
            }
            else if (lines[lineIndex].StartsWith("game", StringComparison.OrdinalIgnoreCase))
            {
                lineIndex = ReadGame(db, gameNames, lines, lineCount, lineIndex);
            }
            else
            {
                lineIndex++;
            }
        }

        return db;
    }

    private static int ReadGame(RomDatabase db, Dictionary<string, RomGame> gameNames, string[] lines, int lineCount, int lineIndex)
    {
        lineIndex++;

        var game = new RomGame(db);

        while (lineIndex < lineCount && !lines[lineIndex].StartsWith(")", StringComparison.OrdinalIgnoreCase))
        {
            var line = lines[lineIndex].Trim();
            if (line.StartsWith("name", StringComparison.OrdinalIgnoreCase))
            {
                game.Name = line[4..].Trim().Trim('"');
            }
            else if (line.StartsWith("description", StringComparison.OrdinalIgnoreCase))
            {
                game.Description = line[11..].Trim().Trim('"');
            }
            else if (line.StartsWith("year", StringComparison.OrdinalIgnoreCase))
            {
                game.Year = line[4..].Trim().Trim('"');
            }
            else if (line.StartsWith("manufacturer", StringComparison.OrdinalIgnoreCase))
            {
                game.Manufacturer = line[12..].Trim().Trim('"');
            }
            else if (line.StartsWith("cloneof", StringComparison.OrdinalIgnoreCase))
            {
                game.CloneOf = line[7..].Trim().Trim('"');
            }
            else if (line.StartsWith("romof", StringComparison.OrdinalIgnoreCase))
            {
                game.RomOf = line[5..].Trim().Trim('"');
            }
            else if (line.StartsWith("rom", StringComparison.OrdinalIgnoreCase))
            {
                _ = ReadRom(game, line[3..].Trim().Trim('"'));
            }

            lineIndex++;
        }

        if (gameNames.ContainsKey(game.Name))
        {
            throw new DatabaseImportException(string.Format(CultureInfo.CurrentCulture, "Game with name '{0}' is already in the database.", game.Name));
        }

        gameNames.Add(game.Name, game);
        db.Games.Add(game);

        return lineIndex;
    }

    private static string ReadRom(RomGame game, string entryVal)
    {
        var romIndex = 0;

        entryVal = entryVal.Trim().Trim('(').Trim(')').Trim();

        var rom = new RomFile(game);

        while (GetNextPair(entryVal, ref romIndex, out var romPairName, out var romPairVal))
        {
            switch (romPairName)
            {
                case "name":
                    rom.Name = romPairVal.Trim('"').Trim('.');
                    break;

                case "size":
                    rom.Size = long.Parse(romPairVal, CultureInfo.InvariantCulture);
                    break;

                case "crc":
                    if (romPairVal == "-")
                    {
                        rom.CRC32 = new byte[0];
                        rom.Status = RomStatus.NoDump;
                    }
                    else
                    {
                        rom.CRC32 = Hex.TextToByteArray(romPairVal);
                    }

                    break;

                case "md5":
                    rom.MD5 = Hex.TextToByteArray(romPairVal);
                    break;

                case "sha1":
                    rom.SHA1 = Hex.TextToByteArray(romPairVal);
                    break;

                case "sha256":
                    rom.SHA256 = Hex.TextToByteArray(romPairVal);
                    break;

                case "flags":
                    rom.Status = ParseStatus(romPairVal);
                    break;
            }
        }

        game.Roms.Add(rom);
        return entryVal;
    }

    private static bool GetNextPair(string text, ref int index, out string name, out string val)
    {
        name = string.Empty;
        val = string.Empty;

        if (index >= text.Length)
        {
            return false;
        }

        var endNameIndex = text.IndexOf(" ", index + 1, StringComparison.Ordinal);
        if (endNameIndex > index)
        {
            name = text[index..endNameIndex].Trim();

            int endValIndex;
            if (text[endNameIndex + 1] == '"')
            {
                endValIndex = text.IndexOf("\"", endNameIndex + 2, StringComparison.Ordinal);
            }
            else
            {
                endValIndex = text.IndexOf(" ", endNameIndex + 2, StringComparison.Ordinal);
                if (endValIndex < 0)
                {
                    endValIndex = text.Length;
                }
            }

            if (endValIndex > endNameIndex)
            {
                val = text.Substring(endNameIndex + 1, endValIndex - endNameIndex - 1).Trim();
                index = endValIndex + 1;
                return true;
            }
        }

        return false;
    }

    private static RomStatus ParseStatus(string text)
    {
        switch (text)
        {
            case "good":
                return RomStatus.Good;
            case "baddump":
                return RomStatus.BadDump;
            case "nodump":
                return RomStatus.NoDump;
            case "verified":
                return RomStatus.Verified;
        }

        throw new NotSupportedException();
    }

    private int ReadHeader(string workingFolderPath, RomDatabase db, string[] lines, int lineCount, int lineIndex)
    {
        lineIndex++;

        while (lineIndex < lineCount && !lines[lineIndex].StartsWith(")", StringComparison.OrdinalIgnoreCase))
        {
            var line = lines[lineIndex].Trim();
            if (line.StartsWith("name", StringComparison.OrdinalIgnoreCase))
            {
                db.Name = line[4..].Trim().Trim('"');
            }
            else if (line.StartsWith("description", StringComparison.OrdinalIgnoreCase))
            {
                db.Description = line[11..].Trim().Trim('"');
            }
            else if (line.StartsWith("category", StringComparison.OrdinalIgnoreCase))
            {
                db.Category = line[8..].Trim().Trim('"');
            }
            else if (line.StartsWith("author", StringComparison.OrdinalIgnoreCase))
            {
                db.Author = line[6..].Trim().Trim('"');
            }
            else if (line.StartsWith("version", StringComparison.OrdinalIgnoreCase))
            {
                db.Version = line[7..].Trim().Trim('"');
            }
            else if (line.StartsWith("comment", StringComparison.OrdinalIgnoreCase))
            {
                db.Comment = line[7..].Trim().Trim('"');
            }
            else if (line.StartsWith("header", StringComparison.OrdinalIgnoreCase))
            {
                var headerFileName = line[6..].Trim().Trim('"');
                if (headerFileName.Length > 0)
                {
                    // Unused
                }
            }

            lineIndex++;
        }

        return lineIndex;
    }
}
