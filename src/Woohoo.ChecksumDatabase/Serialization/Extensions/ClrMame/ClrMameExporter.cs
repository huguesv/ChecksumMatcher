// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

using System;
using System.Diagnostics;
using System.Text;
using Woohoo.ChecksumDatabase.Model;

public class ClrMameExporter : IDatabaseExporter
{
    public string Filter => "ClrMame Files (*.dat)|*.dat";

    public string Export(RomDatabase db)
    {
        Requires.NotNull(db);

        var text = new StringBuilder();

        WriteHeader(text, db);

        foreach (var game in db.Games)
        {
            WriteGame(text, game);
        }

        return text.ToString();
    }

    private static void WriteHeader(StringBuilder text, RomDatabase db)
    {
        Debug.Assert(text != null, "Null text.");
        Debug.Assert(db != null, "Null db.");

        _ = text.AppendLine("clrmamepro (");
        _ = text.AppendFormat("\tname {0}\r\n", Quote(db.Name));
        _ = text.AppendFormat("\tdescription {0}\r\n", Quote(db.Description));
        _ = text.AppendFormat("\tcategory {0}\r\n", Quote(db.Category));
        _ = text.AppendFormat("\tversion {0}\r\n", Quote(db.Version));
        _ = text.AppendFormat("\tauthor {0}\r\n", Quote(db.Author));
        _ = text.AppendLine(")");
        _ = text.AppendLine();
    }

    private static void WriteGame(StringBuilder text, RomGame game)
    {
        Debug.Assert(text != null, "Null text.");
        Debug.Assert(game != null, "Null game.");

        _ = text.AppendLine("game (");
        _ = text.AppendFormat("\tname {0}\r\n", Quote(game.Name));
        _ = text.AppendFormat("\tdescription {0}\r\n", Quote(game.Description));

        if (game.Year.Length > 0)
        {
            _ = text.AppendFormat("\tyear {0}\r\n", Quote(game.Year));
        }

        if (game.Manufacturer.Length > 0)
        {
            _ = text.AppendFormat("\tmanufacturer {0}\r\n", Quote(game.Manufacturer));
        }

        if (game.RomOf.Length > 0)
        {
            _ = text.AppendFormat("\tromof {0}\r\n", Quote(game.RomOf));
        }

        if (game.CloneOf.Length > 0)
        {
            _ = text.AppendFormat("\tcloneof {0}\r\n", Quote(game.CloneOf));
        }

        foreach (var rom in game.Roms)
        {
            WriteRom(text, rom);
        }

        _ = text.AppendLine(")");
        _ = text.AppendLine();
    }

    private static void WriteRom(StringBuilder text, RomFile rom)
    {
        Debug.Assert(text != null, "Null text.");
        Debug.Assert(rom != null, "Null rom.");

        _ = text.Append("\trom ( ");
        _ = text.AppendFormat("name {0} ", Quote(rom.Name));
        _ = text.AppendFormat("size {0} ", rom.Size);

        if (rom.CRC32.Length > 0)
        {
            _ = text.AppendFormat("crc {0} ", Hex.ByteArrayToText(rom.CRC32));
        }

        if (rom.MD5.Length > 0)
        {
            _ = text.AppendFormat("md5 {0} ", Hex.ByteArrayToText(rom.MD5));
        }

        if (rom.SHA1.Length > 0)
        {
            _ = text.AppendFormat("sha1 {0} ", Hex.ByteArrayToText(rom.SHA1));
        }

        _ = text.AppendLine(")");
    }

    private static string Quote(string text)
    {
        if (text.Contains(" "))
        {
            text = "\"" + text + "\"";
        }

        return text;
    }
}
