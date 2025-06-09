// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.UnitTest;

using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;
using Woohoo.ChecksumDatabase.UnitTest.Infrastructure;

#pragma warning disable SA1027

public class ClrMameXmlExporterUnitTest
{
    [Fact]
    public void Export()
    {
        // Arrange
        var db = new DatabaseBuilder()
            .WithName("Test Database")
            .WithHeader("Test Description", "Test Author", "Test@email.com", "1.00", "2025-01-01", "url@author.com", "Test Comment", "https://database.com")
            .WithGame(g => g.WithName("Game One")
                .WithRomFile("game1.rom", 6007, "df026bc7", "658f701f85d4958db9f05012831e095b", "e07533592582b3ae630614096f493675031074ca", "74ed1723ef5ef4262b51c5b27cafc7d805ffee27d7fde4a8ddce82efbca4e07a"))
            .WithGame(g => g.WithName("Game Two & Half")
                .WithRomFile("game2&a.rom", 21800, "13651561", "f62fbcb2876b99e8a8c079cfafbb7823", "9f5ccd28541ca1bb4626539aa870670b7afc3753", string.Empty)
                .WithRomFile("game2&b.rom", 14609, "eac2f95f", "091377a704c0d1adb65cd6b852d380e4", "0beb99d8ba4d27cfdf13ee988889aa81aa75a7b1", string.Empty))
            .Build();

        // Act
        var actual = new ClrMameXmlExporter().Export(db);

        // Assert
        var expected = """
<?xml version="1.0" encoding="utf-8"?>
<datafile>
	<header>
		<name>Test Database</name>
		<description>Test Description</description>
		<version>1.00</version>
		<date>2025-01-01</date>
		<author>Test Author</author>
		<email>Test@email.com</email>
		<homepage>https://database.com</homepage>
		<url>url@author.com</url>
		<comment>Test Comment</comment>
		<clrmamepro />
	</header>
	<game name="Game One">
		<description>Game One</description>
		<rom name="game1.rom" size="6007" crc="df026bc7" md5="658f701f85d4958db9f05012831e095b" sha1="e07533592582b3ae630614096f493675031074ca" sha256="74ed1723ef5ef4262b51c5b27cafc7d805ffee27d7fde4a8ddce82efbca4e07a" />
	</game>
	<game name="Game Two &amp; Half">
		<description>Game Two &amp; Half</description>
		<rom name="game2&amp;a.rom" size="21800" crc="13651561" md5="f62fbcb2876b99e8a8c079cfafbb7823" sha1="9f5ccd28541ca1bb4626539aa870670b7afc3753" />
		<rom name="game2&amp;b.rom" size="14609" crc="eac2f95f" md5="091377a704c0d1adb65cd6b852d380e4" sha1="0beb99d8ba4d27cfdf13ee988889aa81aa75a7b1" />
	</game>
</datafile>
""";
        actual.Should().Be(expected);
    }
}

#pragma warning restore SA1027
