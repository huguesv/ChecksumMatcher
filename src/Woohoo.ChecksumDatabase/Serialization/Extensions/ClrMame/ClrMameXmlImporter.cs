// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Woohoo.ChecksumDatabase.Model;

public class ClrMameXmlImporter : IDatabaseImporter
{
    public bool CanImport(string text)
    {
        Requires.NotNull(text);

        if (text.StartsWith("<?xml version=\"1.0\"", StringComparison.OrdinalIgnoreCase))
        {
            if (text.Contains("<datafile"))
            {
                return true;
            }
        }

        return false;
    }

    public RomDatabase Import(string text, string workingFolderPath)
    {
        Requires.NotNull(text);
        Requires.NotNull(workingFolderPath);

        var db = new RomDatabase();

        var settings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true,
            DtdProcessing = DtdProcessing.Ignore,
        };

        using (var input = new StringReader(text))
        {
            var reader = XmlReader.Create(input, settings);
            _ = reader.MoveToContent();
            if (reader.Name == "datafile")
            {
                ReadDatafile(reader, db);
            }
        }

        return db;
    }

    private static void ReadDatafile(XmlReader reader, RomDatabase db)
    {
        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "datafile")
            {
                switch (reader.Name)
                {
                    case "header":
                        ReadHeader(reader, db);
                        break;
                    case "game":
                        ReadGame(reader, db);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }

    private static void ReadHeader(XmlReader reader, RomDatabase db)
    {
        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "header")
            {
                switch (reader.Name)
                {
                    case "name":
                        db.Name = reader.ReadElementContentAsString();
                        break;
                    case "description":
                        db.Description = reader.ReadElementContentAsString();
                        break;
                    case "category":
                        db.Category = reader.ReadElementContentAsString();
                        break;
                    case "version":
                        db.Version = reader.ReadElementContentAsString();
                        break;
                    case "date":
                        db.Date = reader.ReadElementContentAsString();
                        break;
                    case "author":
                        db.Author = reader.ReadElementContentAsString();
                        break;
                    case "email":
                        db.Email = reader.ReadElementContentAsString();
                        break;
                    case "homepage":
                        db.Homepage = reader.ReadElementContentAsString();
                        break;
                    case "url":
                        db.Url = reader.ReadElementContentAsString();
                        break;
                    case "comment":
                        db.Comment = reader.ReadElementContentAsString();
                        break;
                    case "clrmamepro":
                        ReadClrmamepro(reader);
                        break;
                    case "romcenter":
                        ReadRomcenter(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }

    private static void ReadClrmamepro(XmlReader reader)
    {
        reader.Skip();
    }

    private static void ReadRomcenter(XmlReader reader)
    {
        reader.Skip();
    }

    private static void ReadGame(XmlReader reader, RomDatabase db)
    {
        var game = new RomGame(db)
        {
            Name = reader.GetAttribute("name") ?? string.Empty,
        };

        var sourceFile = reader.GetAttribute("sourcefile");
        if (!string.IsNullOrEmpty(sourceFile))
        {
            game.SourceFile = sourceFile;
        }

        var isBios = reader.GetAttribute("isbios");
        if (!string.IsNullOrEmpty(isBios))
        {
            game.IsBios = ParseYesNo(isBios);
        }

        var cloneOf = reader.GetAttribute("cloneof");
        if (!string.IsNullOrEmpty(cloneOf))
        {
            game.CloneOf = cloneOf;
        }

        var romOf = reader.GetAttribute("romof");
        if (!string.IsNullOrEmpty(romOf))
        {
            game.RomOf = romOf;
        }

        var sampleOf = reader.GetAttribute("sampleof");
        if (!string.IsNullOrEmpty(sampleOf))
        {
            game.SampleOf = sampleOf;
        }

        var board = reader.GetAttribute("board");
        if (!string.IsNullOrEmpty(board))
        {
            game.Board = board;
        }

        var rebuildTo = reader.GetAttribute("rebuildto");
        if (!string.IsNullOrEmpty(rebuildTo))
        {
            game.RebuildTo = rebuildTo;
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "game")
            {
                switch (reader.Name)
                {
                    case "category":
                        _ = reader.ReadElementContentAsString();
                        break;
                    case "comment":
                        game.Comments.Add(reader.ReadElementContentAsString());
                        break;
                    case "description":
                        game.Description = reader.ReadElementContentAsString();
                        break;
                    case "year":
                        game.Year = reader.ReadElementContentAsString();
                        break;
                    case "manufacturer":
                        game.Manufacturer = reader.ReadElementContentAsString();
                        break;
                    case "release":
                        ReadRelease(reader, game);
                        break;
                    case "biosset":
                        ReadBiosset(reader, game);
                        break;
                    case "rom":
                        ReadRom(reader, game);
                        break;
                    case "disk":
                        ReadDisk(reader, game);
                        break;
                    case "sample":
                        ReadSample(reader, game);
                        break;
                    case "archive":
                        ReadArchive(reader, game);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.ReadEndElement();
        }

        db.Games.Add(game);
    }

    private static void ReadRelease(XmlReader reader, RomGame game)
    {
        var release = new RomRelease(game)
        {
            Name = reader.GetAttribute("name") ?? string.Empty,
            Region = reader.GetAttribute("region") ?? string.Empty,
        };

        var language = reader.GetAttribute("language");
        if (!string.IsNullOrEmpty(language))
        {
            release.Language = language;
        }

        var date = reader.GetAttribute("date");
        if (!string.IsNullOrEmpty(date))
        {
            release.Date = date;
        }

        var isDefault = reader.GetAttribute("default");
        if (!string.IsNullOrEmpty(isDefault))
        {
            release.IsDefault = ParseYesNo(isDefault);
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }

        game.Releases.Add(release);
    }

    private static void ReadBiosset(XmlReader reader, RomGame game)
    {
        var biosSet = new RomBiosSet(game)
        {
            Name = reader.GetAttribute("name") ?? string.Empty,
            Description = reader.GetAttribute("description") ?? string.Empty,
        };

        var isDefault = reader.GetAttribute("default");
        if (!string.IsNullOrEmpty(isDefault))
        {
            biosSet.IsDefault = ParseYesNo(isDefault);
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }

        game.BiosSets.Add(biosSet);
    }

    private static void ReadRom(XmlReader reader, RomGame game)
    {
        var name = reader.GetAttribute("name");
        var size = reader.GetAttribute("size");

        var rom = new RomFile(game)
        {
            Name = name?.Trim('.') ?? string.Empty,
            Size = long.Parse(string.IsNullOrEmpty(size) ? "0" : size, CultureInfo.InvariantCulture),
        };

        var crc = reader.GetAttribute("crc");
        if (crc == "-")
        {
            rom.CRC32 = new byte[0];
            rom.Status = RomStatus.NoDump;
        }
        else if (!string.IsNullOrEmpty(crc))
        {
            rom.CRC32 = ChecksumConversion.ToByteArray(crc);
        }

        var sha256 = reader.GetAttribute("sha256");
        if (!string.IsNullOrEmpty(sha256))
        {
            rom.SHA256 = ChecksumConversion.ToByteArray(sha256);
        }

        var sha1 = reader.GetAttribute("sha1");
        if (!string.IsNullOrEmpty(sha1))
        {
            rom.SHA1 = ChecksumConversion.ToByteArray(sha1);
        }

        var md5 = reader.GetAttribute("md5");
        if (!string.IsNullOrEmpty(md5))
        {
            rom.MD5 = ChecksumConversion.ToByteArray(md5);
        }

        var merge = reader.GetAttribute("merge");
        if (!string.IsNullOrEmpty(merge))
        {
            rom.Merge = merge;
        }

        var status = reader.GetAttribute("status");
        if (!string.IsNullOrEmpty(status))
        {
            rom.Status = ParseStatus(status);
        }

        var date = reader.GetAttribute("date");
        if (!string.IsNullOrEmpty(date))
        {
            rom.Date = date;
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }

        game.Roms.Add(rom);
    }

    private static void ReadDisk(XmlReader reader, RomGame game)
    {
        var disk = new RomDisk(game)
        {
            Name = reader.GetAttribute("name") ?? string.Empty,
        };

        var sha1 = reader.GetAttribute("sha1");
        if (!string.IsNullOrEmpty(sha1))
        {
            disk.SHA1 = ChecksumConversion.ToByteArray(sha1);
        }

        var md5 = reader.GetAttribute("md5");
        if (!string.IsNullOrEmpty(md5))
        {
            disk.MD5 = ChecksumConversion.ToByteArray(md5);
        }

        var merge = reader.GetAttribute("merge");
        if (!string.IsNullOrEmpty(merge))
        {
            disk.Merge = merge;
        }

        var status = reader.GetAttribute("status");
        if (!string.IsNullOrEmpty(status))
        {
            disk.Status = ParseStatus(status);
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }

        game.Disks.Add(disk);
    }

    private static void ReadSample(XmlReader reader, RomGame game)
    {
        var sample = new RomSample(game)
        {
            Name = reader.GetAttribute("name") ?? string.Empty,
        };

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }

        game.Samples.Add(sample);
    }

    private static void ReadArchive(XmlReader reader, RomGame game)
    {
        var archive = new RomArchive(game)
        {
            Name = reader.GetAttribute("name") ?? string.Empty,
        };

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }

        game.Archives.Add(archive);
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

    private static bool ParseYesNo(string text)
    {
        switch (text)
        {
            case "yes":
                return true;
            case "no":
                return false;
        }

        throw new NotSupportedException();
    }
}
