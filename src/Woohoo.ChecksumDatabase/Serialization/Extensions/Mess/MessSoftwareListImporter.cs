// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess;

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Woohoo.ChecksumDatabase.Model;

public sealed class MessSoftwareListImporter : IDatabaseImporter
{
    public bool CanImport(string text)
    {
        Requires.NotNull(text);

        if (text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) && text.Contains("softwarelist.dtd"))
        {
            return true;
        }

        return false;
    }

    public RomDatabase Import(string text)
    {
        Requires.NotNull(text);

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
            if (reader.Name == "softwarelist")
            {
                ReadSoftwareList(reader, db);
            }
        }

        return db;
    }

    private static void ReadSoftwareList(XmlReader reader, RomDatabase db)
    {
        db.Name = reader.GetAttribute("name") ?? string.Empty;
        var description = reader.GetAttribute("description");
        if (description != null)
        {
            db.Description = description;
        }

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "softwarelist")
            {
                switch (reader.Name)
                {
                    case "software":
                        ReadSoftware(reader, db);
                        break;
                    case "notes":
                        _ = reader.ReadElementContentAsString();
                        break;
                    default:
                        Debug.Assert(false, "Unexpected xml element.");
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }

    private static void ReadSoftware(XmlReader reader, RomDatabase db)
    {
        var game = new RomGame(db)
        {
            Name = reader.GetAttribute("name") ?? string.Empty,
            CloneOf = reader.GetAttribute("cloneof") ?? string.Empty,
        };

        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "software")
            {
                switch (reader.Name)
                {
                    case "description":
                        game.Description = reader.ReadElementContentAsString();
                        break;
                    case "year":
                        game.Year = reader.ReadElementContentAsString();
                        break;
                    case "publisher":
                        game.Manufacturer = reader.ReadElementContentAsString();
                        break;
                    case "part":
                        ReadPart(reader, game);
                        break;
                    case "notes":
                        game.Comments.Add(reader.ReadElementContentAsString());
                        break;
                    case "sharedfeat":
                        _ = reader.ReadElementContentAsString();
                        break;
                    case "info":
                        _ = reader.ReadElementContentAsString();
                        break;
                    default:
                        Debug.Assert(false, "Unexpected xml element.");
                        break;
                }
            }

            reader.ReadEndElement();
        }

        if (game.Roms.Count > 0)
        {
            db.Games.Add(game);
        }
    }

    private static void ReadPart(XmlReader reader, RomGame game)
    {
        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "part")
            {
                switch (reader.Name)
                {
                    case "feature":
                        ReadFeature(reader);
                        break;
                    case "dataarea":
                        ReadDataArea(reader, game);
                        break;
                    case "dipswitch":
                        ReadDipSwitch(reader);
                        break;
                    case "diskarea":
                        _ = reader.ReadOuterXml();
                        break;
                    default:
                        Debug.Assert(false, "Unexpected xml element.");
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }

    private static void ReadFeature(XmlReader reader)
    {
        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }
    }

    private static void ReadDataArea(XmlReader reader, RomGame game)
    {
        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "dataarea")
            {
                switch (reader.Name)
                {
                    case "rom":
                        ReadRom(reader, game);
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }

    private static void ReadRom(XmlReader reader, RomGame game)
    {
        var rom = new RomFile(game);

        var name = reader.GetAttribute("name");
        if (!string.IsNullOrEmpty(name))
        {
            rom.Name = name;
        }

        var size = reader.GetAttribute("size");
        if (!string.IsNullOrEmpty(size))
        {
            if (size.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                rom.Size = long.Parse(size[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            else
            {
                rom.Size = long.Parse(size, CultureInfo.InvariantCulture);
            }
        }

        var crc = reader.GetAttribute("crc");
        if (!string.IsNullOrEmpty(crc))
        {
            rom.CRC32 = ChecksumConversion.ToByteArray(crc);
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

        var offset = reader.GetAttribute("offset");
        var offsetVal = 0;

#if false
        if (!string.IsNullOrEmpty(offset))
        {
            offsetVal = int.Parse(offset);
            if (offsetVal != 0)
            {
                throw new NotSupportedException(string.Format("Rom '{0}' has non-zero offset {1}.", rom.Name, offsetVal));
            }
        }
#endif

        var status = reader.GetAttribute("status");
        if (!string.IsNullOrEmpty(status))
        {
            rom.Status = ParseStatus(status);
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

        if (offsetVal == 0 && name != null)
        {
            game.Roms.Add(rom);
        }
    }

    private static void ReadDipSwitch(XmlReader reader)
    {
        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();

            while (reader.Name != "dipswitch")
            {
                switch (reader.Name)
                {
                    case "dipvalue":
                        ReadDipValue(reader);
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }

    private static void ReadDipValue(XmlReader reader)
    {
        if (reader.IsEmptyElement)
        {
            _ = reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.ReadEndElement();
        }
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
        }

        throw new NotSupportedException();
    }
}
