// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.UnitTest;

using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

public class ClrMameXmlImporterUnitTest
{
    [Fact]
    public void ImportHartungGameMaster()
    {
        var db = Import(Databases.Hartung___Game_Master__20080716_);
        db.Games.Count.Should().Be(12);
    }

    private static RomDatabase Import(string expected)
    {
        return new ClrMameXmlImporter().Import(expected, string.Empty);
    }
}
