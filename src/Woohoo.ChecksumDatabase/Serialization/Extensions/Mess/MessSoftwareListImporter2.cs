// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mess;

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

public sealed class MessSoftwareListImporter2 : IDatabaseImporter
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

        XmlSerializer serializer = new XmlSerializer(typeof(SoftwareList));
        using XmlReader reader = XmlReader.Create(new StringReader(text), settings);
        var actual = (SoftwareList?)serializer.Deserialize(reader);
        if (actual is not null)
        {
            db.Name = actual.Name;
            db.Description = actual.Description ?? string.Empty;
            foreach (var software in actual.Software)
            {
                var game = new RomGame(db)
                {
                    Name = software.Name,
                    CloneOf = software.Cloneof ?? string.Empty,
                    Description = software.Description ?? string.Empty,
                    Year = software.Year ?? string.Empty,
                    Manufacturer = software.Publisher ?? string.Empty,
                };

                foreach (var part in software.Part)
                {
                    foreach (var dataArea in part.DataArea)
                    {
                        foreach (var rom in dataArea.Rom)
                        {
                            var romFile = new RomFile(game)
                            {
                                Name = rom.Name ?? string.Empty,
                                Size = rom.Size != null && rom.Size.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ?
                                    long.Parse(rom.Size[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture) :
                                    (rom.Size != null ? long.Parse(rom.Size, CultureInfo.InvariantCulture) : 0),
                                Status = ConvertStatus(rom.Status),
                                CRC32 = rom.Crc != null ? ChecksumConversion.ToByteArray(rom.Crc) : Array.Empty<byte>(),
                                SHA1 = rom.Sha1 != null ? ChecksumConversion.ToByteArray(rom.Sha1) : Array.Empty<byte>(),
                            };
                            if (!string.IsNullOrEmpty(romFile.Name))
                            {
                                game.Roms.Add(romFile);
                            }
                        }
                    }
                }

                db.Games.Add(game);
            }
        }

        return db;

        static ChecksumDatabase.Model.RomStatus ConvertStatus(Model.RomStatus status)
        {
            switch (status)
            {
                case Model.RomStatus.BadDump:
                    return ChecksumDatabase.Model.RomStatus.BadDump;
                case Model.RomStatus.NoDump:
                    return ChecksumDatabase.Model.RomStatus.NoDump;
                case Model.RomStatus.Good:
                    return ChecksumDatabase.Model.RomStatus.Good;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
