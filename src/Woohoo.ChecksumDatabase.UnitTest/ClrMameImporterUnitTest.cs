// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.UnitTest;

using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

public class ClrMameImporterUnitTest
{
    [Fact]
    public void ImportTosecSegaDreamcast()
    {
        var db = Import(Databases.Sega_Dreamcast___Games____GDI___TOSEC_v2007_12_30_CM_);
        db.Games.Count.Should().Be(284);
    }

    [Fact]
    public void ImportTosecAtariST()
    {
        var db = Import(Databases.Atari_ST___Compilations___Games____ST___TOSEC_v2006_10_22_CM_);
        db.Games.Count.Should().Be(7573);
    }

    [Fact]
    public void ImportNoIntro()
    {
        var db = Import(Databases.Nintendo___Virtual_Boy__20071103_);
        db.Games.Count.Should().Be(24);
    }

    private static RomDatabase Import(string expected)
    {
        return new ClrMameImporter(null).Import(expected, string.Empty);
    }
}
