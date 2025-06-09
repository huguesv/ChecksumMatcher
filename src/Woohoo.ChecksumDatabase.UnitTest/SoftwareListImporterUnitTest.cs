// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.UnitTest;

using System.Xml.Serialization;
using Woohoo.ChecksumDatabase.Serialization.Extensions.Mess.Model;

#pragma warning disable SA1027

public class SoftwareListImporterUnitTest
{
    [Fact]
    public void Import()
    {
        // Arrange
        var xml = """
<softwarelist name="abc80_flop" description="Luxor ABC 80 diskettes">

	<software name="cpm" supported="no">
		<description>CP/M BIOS 3.7</description>
		<year>19??</year>
		<publisher>Myab</publisher>

		<!-- Datadisc 80 format (8 sectors/track, 256 bytes/sector interleave, sector skew 5: 0 5 2 7 4 1 6 3) -->
		<part name="flop1" interface="floppy_5_25">
			<feature name="part_id" value="Side 0"/>
			<dataarea name="flop" size="81920">
				<rom name="cpmdisk_myab37_side0.img" size="81920" crc="7065fa91" sha1="f0ca7c2785bd92ed7ede46aaba8b8e2a2d826e47" loadflag="fill"/>
			</dataarea>
		</part>

		<part name="flop2" interface="floppy_5_25">
			<feature name="part_id" value="Side 1"/>
			<dataarea name="flop" size="81920">
				<rom name="cpmdisk_myab37_side1.img" size="81920" crc="062e88c7" sha1="2d3baa1f225129474839972b9735444120965e08"/>
			</dataarea>
		</part>
	</software>

	<software name="system10">
		<description>System disk v1.0</description>
		<year>19??</year>
		<publisher>Luxor</publisher>

		<part name="flop1" interface="floppy_5_25">
			<dataarea name="flop" size="47013">
				<rom name="system10.td0" size="47013" crc="be80a5ca" sha1="64bea62668e9b2604732810c0704f52f6c8c3f99"/>
			</dataarea>
		</part>
	</software>

	<!-- https://www.youtube.com/watch?v=5IVFEVL4oAg -->
	<software name="abcdemo">
		<description>ABCDemo</description>
		<year>2015</year>
		<publisher>Genesis Project</publisher>
		<info name="usage" value="RUN&quot;A.B&quot;" />

		<part name="flop1" interface="floppy_5_25">
			<dataarea name="flop" size="163840">
				<rom name="genesisproject_abcdemo.dsk" size="163840" crc="29059d3c" sha1="55b38436a2d3cfe1caf8e35e44775402a1511eec"/>
			</dataarea>
		</part>
	</software>

</softwarelist>
""";

        // Act
        XmlSerializer serializer = new XmlSerializer(typeof(SoftwareList));
        using StringReader reader = new StringReader(xml);
        var actual = (SoftwareList?)serializer.Deserialize(reader);

        // Assert
    }
}

#pragma warning restore SA1027
