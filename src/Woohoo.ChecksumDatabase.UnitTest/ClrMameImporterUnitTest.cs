// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.UnitTest;

using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;
using Woohoo.ChecksumDatabase.UnitTest.Infrastructure;

#pragma warning disable SA1027

public class ClrMameImporterUnitTest
{
    [Fact]
    public void Import()
    {
        // Arrange
        var xml = """
clrmamepro (
	name "Test Database"
	description "Test Description"
	category 
	version 1.00
	author "Test Author"
)

game (
	name "Game One"
	description "Game One"
	rom ( name game1.rom size 6007 crc df026bc7 md5 658f701f85d4958db9f05012831e095b sha1 e07533592582b3ae630614096f493675031074ca )
)

game (
	name "Game Two & Half"
	description "Game Two & Half"
	rom ( name game2&a.rom size 21800 crc 13651561 md5 f62fbcb2876b99e8a8c079cfafbb7823 sha1 9f5ccd28541ca1bb4626539aa870670b7afc3753 )
	rom ( name game2&b.rom size 14609 crc eac2f95f md5 091377a704c0d1adb65cd6b852d380e4 sha1 0beb99d8ba4d27cfdf13ee988889aa81aa75a7b1 )
)


""";

        // Act
        var actual = new ClrMameImporter().Import(xml);

        // Assert
        var expected = new DatabaseBuilder()
            .WithName("Test Database")
            .WithHeader("Test Description", "Test Author", email: string.Empty, "1.00", date: string.Empty, url: string.Empty, comment: string.Empty, homepage: string.Empty)
            .WithGame(g => g.WithName("Game One")
                .WithRomFile("game1.rom", 6007, "df026bc7", "658f701f85d4958db9f05012831e095b", "e07533592582b3ae630614096f493675031074ca", string.Empty))
            .WithGame(g => g.WithName("Game Two & Half")
                .WithRomFile("game2&a.rom", 21800, "13651561", "f62fbcb2876b99e8a8c079cfafbb7823", "9f5ccd28541ca1bb4626539aa870670b7afc3753", string.Empty)
                .WithRomFile("game2&b.rom", 14609, "eac2f95f", "091377a704c0d1adb65cd6b852d380e4", "0beb99d8ba4d27cfdf13ee988889aa81aa75a7b1", string.Empty))
            .Build();

        actual.Should().BeEquivalentTo(expected, options =>
            options
                .Excluding(db => db.Games));
        actual.Games.Should().BeEquivalentTo(expected.Games, options =>
            options
                .Excluding(game => game.ParentDatabase)
                .For(game => game.Roms)
                .Exclude(rom => rom.ParentGame));
    }
}

#pragma warning restore SA1027
