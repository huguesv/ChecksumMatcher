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

        var serializer = new XmlSerializer(typeof(SoftwareList));
        using var reader = XmlReader.Create(new StringReader(text), settings);
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
                            if (string.IsNullOrEmpty(rom.Name))
                            {
                                continue;
                            }

                            var romFile = new RomFile(game)
                            {
                                Name = rom.Name ?? string.Empty,
                                Size = rom.Size != null && rom.Size.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ?
                                    long.Parse(rom.Size[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture) :
                                    (rom.Size != null ? long.Parse(rom.Size, CultureInfo.InvariantCulture) : 0),
                                Status = ConvertRomStatus(rom.Status),
                                CRC32 = rom.Crc != null ? ChecksumConversion.ToByteArray(rom.Crc) : [],
                                SHA1 = rom.Sha1 != null ? ChecksumConversion.ToByteArray(rom.Sha1) : [],
                            };

                            game.Roms.Add(romFile);
                        }
                    }

                    foreach (var diskArea in part.DiskArea)
                    {
                        foreach (var disk in diskArea.Disk)
                        {
                            if (string.IsNullOrEmpty(disk.Name))
                            {
                                continue;
                            }

                            var romDisk = new RomDisk(game)
                            {
                                Name = disk.Name ?? string.Empty,
                                Status = ConvertDiskStatus(disk.Status),
                                SHA1 = disk.Sha1 != null ? ChecksumConversion.ToByteArray(disk.Sha1) : [],
                            };

                            game.Disks.Add(romDisk);
                        }
                    }
                }

                db.Games.Add(game);
            }
        }

        return db;

        static ChecksumDatabase.Model.RomStatus ConvertRomStatus(Model.RomStatus status)
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

        static ChecksumDatabase.Model.RomStatus ConvertDiskStatus(Model.DiskStatus status)
        {
            switch (status)
            {
                case DiskStatus.BadDump:
                    return ChecksumDatabase.Model.RomStatus.BadDump;
                case DiskStatus.NoDump:
                    return ChecksumDatabase.Model.RomStatus.NoDump;
                case DiskStatus.Good:
                    return ChecksumDatabase.Model.RomStatus.Good;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
