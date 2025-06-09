// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization.Extensions.Mame;

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.Mame.Model;

public sealed class MameImporter : IDatabaseImporter
{
    public bool CanImport(string text)
    {
        Requires.NotNull(text);

        if (text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) && text.Contains("DOCTYPE mame", StringComparison.InvariantCulture))
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

        var serializer = new XmlSerializer(typeof(mame));
        using var reader = XmlReader.Create(new StringReader(text), settings);
        var actual = (mame?)serializer.Deserialize(reader);
        if (actual is not null)
        {
            db.Name = actual.build;
            db.Description = string.Empty;

            foreach (var machine in actual.machine)
            {
                var game = new RomGame(db)
                {
                    Name = machine.name,
                    CloneOf = machine.cloneof ?? string.Empty,
                    Description = machine.description ?? string.Empty,
                    Year = machine.year ?? string.Empty,
                    Manufacturer = machine.manufacturer ?? string.Empty,
                    RomOf = machine.romof ?? string.Empty,
                    IsBios = machine.isbios == machineIsbios.yes,
                };

                if (machine.rom is not null)
                {
                    foreach (var rom in machine.rom)
                    {
                        if (string.IsNullOrEmpty(rom.name) || rom.status == romStatus.nodump)
                        {
                            continue;
                        }

                        if (game.Roms.Any(r => string.Equals(r.Name, rom.name, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Duplicate ROM entry, skip it.
                            continue;
                        }

                        var romFile = new RomFile(game)
                        {
                            Name = rom.name ?? string.Empty,
                            Size = rom.size != null && rom.size.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ?
                                long.Parse(rom.size[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture) :
                                (rom.size != null ? long.Parse(rom.size, CultureInfo.InvariantCulture) : 0),
                            Status = ConvertRomStatus(rom.status),
                            CRC32 = rom.crc != null ? ChecksumConversion.ToByteArray(rom.crc) : [],
                            SHA1 = rom.sha1 != null ? ChecksumConversion.ToByteArray(rom.sha1) : [],
                            Merge = rom.merge ?? string.Empty,
                        };

                        game.Roms.Add(romFile);
                    }
                }

                if (machine.disk is not null)
                {
                    foreach (var disk in machine.disk)
                    {
                        if (string.IsNullOrEmpty(disk.name) || string.IsNullOrEmpty(disk.sha1) || disk.status == diskStatus.nodump)
                        {
                            continue;
                        }

                        var romDisk = new RomDisk(game)
                        {
                            Name = disk.name ?? string.Empty,
                            Status = ConvertDiskStatus(disk.status),
                            SHA1 = disk.sha1 != null ? ChecksumConversion.ToByteArray(disk.sha1) : [],
                        };

#if false
                        game.Disks.Add(romDisk);
#endif
                    }
                }

                db.Games.Add(game);
            }
        }

        return db;

        static ChecksumDatabase.Model.RomStatus ConvertRomStatus(romStatus status)
        {
            switch (status)
            {
                case romStatus.baddump:
                    return ChecksumDatabase.Model.RomStatus.BadDump;
                case romStatus.nodump:
                    return ChecksumDatabase.Model.RomStatus.NoDump;
                case romStatus.good:
                    return ChecksumDatabase.Model.RomStatus.Good;
                default:
                    throw new NotSupportedException();
            }
        }

        static ChecksumDatabase.Model.RomStatus ConvertDiskStatus(diskStatus status)
        {
            switch (status)
            {
                case diskStatus.baddump:
                    return ChecksumDatabase.Model.RomStatus.BadDump;
                case diskStatus.nodump:
                    return ChecksumDatabase.Model.RomStatus.NoDump;
                case diskStatus.good:
                    return ChecksumDatabase.Model.RomStatus.Good;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
