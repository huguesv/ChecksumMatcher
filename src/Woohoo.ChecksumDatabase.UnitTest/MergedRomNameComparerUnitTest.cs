// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.UnitTest;

using Woohoo.ChecksumDatabase.Serialization;

public class MergedRomNameComparerUnitTest
{
    [Theory]
    [InlineData("altbeastbl2", "al-1-27512-a.ic53", "altbeastbl2", "al-1-27512-a.ic53", 0)]
    [InlineData("altbeastbl", "1.bin", "altbeastbl2", "al-1-27512-a.ic53", 1)]
    [InlineData("", "1.bin", "altbeastbl2", "al-1-27512-a.ic53", -1)]
    [InlineData("altbeastbl", "1.bin", "", "al-1-27512-a.ic53", 1)]
    [InlineData("", "abc.bin", "", "def.bin", -1)]
    [InlineData("", "def.bin", "", "abc.bin", 1)]
    [InlineData("bj92", "10.c2.ic85", "bj92", "5.c2.ic75", 1)]
    [InlineData("bj92", "5.c2.ic75", "bj92", "10.c2.ic85", -1)]
    public void Compare(string parentName1, string romName1, string parentName2, string romName2, int expected)
    {
        var comparer = MergedRomNameComparer.Instance;

        var rom1 = new MergedRom
        {
            OriginalParentGameName = parentName1,
            Name = romName1,
        };

        var rom2 = new MergedRom
        {
            OriginalParentGameName = parentName2,
            Name = romName2,
        };

        var actual = comparer.Compare(rom1, rom2);
        actual.Should().Be(expected);
    }
}
