// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Woohoo.ChecksumDatabase.Model;

public sealed class ClrMameXmlExporter : IDatabaseExporter
{
    public string Filter => "ClrMameXml Files (*.dat)|*.dat";

    public string Export(RomDatabase db)
    {
        Requires.NotNull(db);

        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            IndentChars = "\t",
        };

        var textWriter = new StringWriterWithEncoding(settings.Encoding);

        var writer = XmlWriter.Create(textWriter, settings);
        using (writer)
        {
            WriteDatafile(writer, db);
        }

        return textWriter.ToString();
    }

    private static void WriteDatafile(XmlWriter writer, RomDatabase db)
    {
        writer.WriteStartElement("datafile");

        WriteHeader(writer, db);

        foreach (var game in db.Games)
        {
            WriteGame(writer, game);
        }

        writer.WriteEndElement();
    }

    private static void WriteHeader(XmlWriter writer, RomDatabase db)
    {
        writer.WriteStartElement("header");

        writer.WriteElementString("name", db.Name);
        writer.WriteElementString("description", db.Description);

        if (db.Category.Length > 0)
        {
            writer.WriteElementString("category", db.Category);
        }

        writer.WriteElementString("version", db.Version);

        if (db.Date.Length > 0)
        {
            writer.WriteElementString("date", db.Date);
        }

        writer.WriteElementString("author", db.Author);

        if (db.Email.Length > 0)
        {
            writer.WriteElementString("email", db.Email);
        }

        if (db.Homepage.Length > 0)
        {
            writer.WriteElementString("homepage", db.Homepage);
        }

        if (db.Url.Length > 0)
        {
            writer.WriteElementString("url", db.Url);
        }

        if (db.Comment.Length > 0)
        {
            writer.WriteElementString("comment", db.Comment);
        }

        writer.WriteElementString("clrmamepro", string.Empty);

        writer.WriteEndElement();
    }

    private static void WriteGame(XmlWriter writer, RomGame game)
    {
        writer.WriteStartElement("game");

        writer.WriteAttributeString("name", game.Name);

        if (game.SourceFile.Length > 0)
        {
            writer.WriteAttributeString("sourcefile", game.SourceFile);
        }

        if (game.IsBios)
        {
            writer.WriteAttributeString("isbios", FormatYesNo(game.IsBios));
        }

        if (game.CloneOf.Length > 0)
        {
            writer.WriteAttributeString("cloneof", game.CloneOf);
        }

        if (game.RomOf.Length > 0)
        {
            writer.WriteAttributeString("romof", game.RomOf);
        }

        if (game.SampleOf.Length > 0)
        {
            writer.WriteAttributeString("sampleof", game.SampleOf);
        }

        if (game.Board.Length > 0)
        {
            writer.WriteAttributeString("board", game.Board);
        }

        if (game.RebuildTo.Length > 0)
        {
            writer.WriteAttributeString("rebuildto", game.RebuildTo);
        }

        foreach (var comment in game.Comments)
        {
            writer.WriteElementString("comment", comment);
        }

        writer.WriteElementString("description", game.Description);

        if (game.Year.Length > 0)
        {
            writer.WriteElementString("year", game.Year);
        }

        if (game.Manufacturer.Length > 0)
        {
            writer.WriteElementString("manufacturer", game.Manufacturer);
        }

        if (game.Details.Count > 0)
        {
            writer.WriteStartElement("details");
            foreach (var pair in game.Details)
            {
                writer.WriteElementString(pair.Key, pair.Value);
            }

            writer.WriteEndElement();
        }

        foreach (var release in game.Releases)
        {
            WriteRelease(writer, release);
        }

        foreach (var biosSet in game.BiosSets)
        {
            WriteBiosSet(writer, biosSet);
        }

        foreach (var rom in game.Roms)
        {
            WriteRom(writer, rom);
        }

        foreach (var disk in game.Disks)
        {
            WriteDisk(writer, disk);
        }

        foreach (var sample in game.Samples)
        {
            WriteSample(writer, sample);
        }

        foreach (var archive in game.Archives)
        {
            WriteArchive(writer, archive);
        }

        writer.WriteEndElement();
    }

    private static void WriteSample(XmlWriter writer, RomSample sample)
    {
        writer.WriteStartElement("sample");

        writer.WriteAttributeString("name", sample.Name);

        writer.WriteEndElement();
    }

    private static void WriteArchive(XmlWriter writer, RomArchive archive)
    {
        writer.WriteStartElement("archive");

        writer.WriteAttributeString("name", archive.Name);

        writer.WriteEndElement();
    }

    private static void WriteDisk(XmlWriter writer, RomDisk disk)
    {
        writer.WriteStartElement("disk");

        writer.WriteAttributeString("name", disk.Name);

        if (disk.SHA1.Length > 0)
        {
            writer.WriteAttributeString("sha1", ChecksumConversion.ToHex(disk.SHA1));
        }

        if (disk.MD5.Length > 0)
        {
            writer.WriteAttributeString("md5", ChecksumConversion.ToHex(disk.MD5));
        }

        if (disk.Merge.Length > 0)
        {
            writer.WriteAttributeString("merge", disk.Merge);
        }

        if (disk.Status != RomStatus.Good)
        {
            writer.WriteAttributeString("status", FormatStatus(disk.Status));
        }

        writer.WriteEndElement();
    }

    private static void WriteBiosSet(XmlWriter writer, RomBiosSet biosSet)
    {
        writer.WriteStartElement("biosset");

        writer.WriteAttributeString("name", biosSet.Name);
        writer.WriteAttributeString("description", biosSet.Description);

        if (biosSet.IsDefault)
        {
            writer.WriteAttributeString("default", FormatYesNo(biosSet.IsDefault));
        }

        writer.WriteEndElement();
    }

    private static void WriteRelease(XmlWriter writer, RomRelease release)
    {
        writer.WriteStartElement("rom");

        writer.WriteAttributeString("name", release.Name);
        writer.WriteAttributeString("region", release.Region);

        if (release.Language.Length > 0)
        {
            writer.WriteAttributeString("language", release.Language);
        }

        if (release.Date.Length > 0)
        {
            writer.WriteAttributeString("date", release.Date);
        }

        if (release.IsDefault)
        {
            writer.WriteAttributeString("default", FormatYesNo(release.IsDefault));
        }

        writer.WriteEndElement();
    }

    private static void WriteRom(XmlWriter writer, RomFile rom)
    {
        writer.WriteStartElement("rom");

        writer.WriteAttributeString("name", rom.Name);
        writer.WriteAttributeString("size", rom.Size.ToString(CultureInfo.InvariantCulture));

        if (rom.CRC32.Length > 0)
        {
            writer.WriteAttributeString("crc", ChecksumConversion.ToHex(rom.CRC32));
        }

        if (rom.MD5.Length > 0)
        {
            writer.WriteAttributeString("md5", ChecksumConversion.ToHex(rom.MD5));
        }

        if (rom.SHA1.Length > 0)
        {
            writer.WriteAttributeString("sha1", ChecksumConversion.ToHex(rom.SHA1));
        }

        if (rom.SHA256.Length > 0)
        {
            writer.WriteAttributeString("sha256", ChecksumConversion.ToHex(rom.SHA256));
        }

        if (rom.Merge.Length > 0)
        {
            writer.WriteAttributeString("merge", rom.Merge);
        }

        if (rom.Status != RomStatus.Good)
        {
            writer.WriteAttributeString("status", FormatStatus(rom.Status));
        }

        if (rom.Date.Length > 0)
        {
            writer.WriteAttributeString("date", rom.Date);
        }

        writer.WriteEndElement();
    }

    private static string FormatStatus(RomStatus val)
    {
        switch (val)
        {
            case RomStatus.Good:
                return "good";
            case RomStatus.BadDump:
                return "baddump";
            case RomStatus.NoDump:
                return "nodump";
        }

        throw new NotSupportedException();
    }

    private static string FormatYesNo(bool val)
    {
        return val ? "yes" : "no";
    }

    internal sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding => this.encoding;
    }
}
