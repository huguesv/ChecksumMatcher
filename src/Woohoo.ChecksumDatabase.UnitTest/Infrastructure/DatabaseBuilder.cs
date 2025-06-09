// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.UnitTest.Infrastructure;

using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization;

internal class DatabaseBuilder
{
    private readonly RomDatabase database = new();

    public DatabaseBuilder WithName(string name)
    {
        this.database.Name = name;
        return this;
    }

    public DatabaseBuilder WithHeader(string description, string author, string email, string version, string date, string url, string comment, string homepage)
    {
        this.database.Description = description;
        this.database.Author = author;
        this.database.Email = email;
        this.database.Version = version;
        this.database.Date = date;
        this.database.Url = url;
        this.database.Comment = comment;
        this.database.Homepage = homepage;
        return this;
    }

    public DatabaseBuilder WithGame(Action<RomGameBuilder> gameBuilderAction)
    {
        var gameBuilder = new RomGameBuilder(this.database);
        gameBuilderAction(gameBuilder);
        this.database.Games.Add(gameBuilder.Build());
        return this;
    }

    public RomDatabase Build()
    {
        return this.database;
    }

    internal class RomGameBuilder
    {
        private readonly RomGame game;

        public RomGameBuilder(RomDatabase database)
        {
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

        public RomGame Build()
        {
            return this.game;
        }
    }
}
