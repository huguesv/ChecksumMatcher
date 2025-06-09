// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.UnitTest.Infrastructure;

using System.Text;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;
using Woohoo.IO.AbstractFileSystem;

internal class DatabaseBuilder
{
    private readonly RomDatabase database = new();

    public DatabaseBuilder WithName(string name)
    {
        this.database.Name = name;
        return this;
    }

    public DatabaseBuilder WithGame(Action<RomGameBuilder> gameBuilderAction)
    {
        var gameBuilder = new RomGameBuilder(this.database);
        gameBuilderAction(gameBuilder);
        this.database.Games.Add(gameBuilder.Build());
        return this;
    }

    public void Build(string filePath)
    {
        var xml = new ClrMameXmlExporter().Export(this.database);
        File.WriteAllText(filePath, xml, Encoding.UTF8);
    }

    internal class RomGameBuilder
    {
        private readonly RomDatabase database;
        private readonly RomGame game;

        public RomGameBuilder(RomDatabase database)
        {
            this.database = database;
            this.game = new RomGame(database);
        }

        public RomGameBuilder WithName(string name)
        {
            this.game.Name = name;
            this.game.Description = name;
            return this;
        }

        public RomGameBuilder WithRomFile(string name, long size, string crc32, string md5, string sha1, string sha256)
        {
            var rom = new RomFile(this.game)
            {
                Name = name,
                Size = size,
                CRC32 = ChecksumConversion.ToByteArray(crc32),
                MD5 = ChecksumConversion.ToByteArray(md5),
                SHA1 = ChecksumConversion.ToByteArray(sha1),
                SHA256 = ChecksumConversion.ToByteArray(sha256),
            };

            this.game.Roms.Add(rom);
            return this;
        }

        public RomGameBuilder WithRomFile(string name, long size, byte[] crc32, byte[] md5, byte[] sha1, byte[] sha256)
        {
            var rom = new RomFile(this.game)
            {
                Name = name,
                Size = size,
                CRC32 = crc32,
                MD5 = md5,
                SHA1 = sha1,
                SHA256 = sha256,
            };

            this.game.Roms.Add(rom);
            return this;
        }

        public RomGameBuilder WithRomFile(string name, RandomFile randomFile)
        {
            var rom = new RomFile(this.game)
            {
                Name = name,
                Size = randomFile.Size,
                CRC32 = randomFile.Checksums["crc32"],
                MD5 = randomFile.Checksums["md5"],
                SHA1 = randomFile.Checksums["sha1"],
                SHA256 = randomFile.Checksums["sha256"],
            };

            this.game.Roms.Add(rom);
            return this;
        }

        public RomGame Build()
        {
            return this.game;
        }
    }
}
