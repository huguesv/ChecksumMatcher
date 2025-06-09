// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.UnitTest;

using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

public class ClrMameXmlExporterUnitTest
{
    [Fact]
    public void ImportExportHartungGameMaster()
    {
        ImportExport(Databases.Hartung___Game_Master__20080716_);
    }

    [Fact]
    public void ImportExportReimportHartungGameMaster()
    {
        ImportExportReimportReexport(Databases.Hartung___Game_Master__20080716_);
    }

    private static void ImportExport(string expected)
    {
        var db = new ClrMameXmlImporter().Import(expected, string.Empty);
        _ = new ClrMameXmlExporter().Export(db);
    }

    private static void ImportExportReimportReexport(string expected)
    {
        var db = new ClrMameXmlImporter().Import(expected, string.Empty);

        var actual = new ClrMameXmlExporter().Export(db);

        var db2 = new ClrMameXmlImporter().Import(actual, string.Empty);
        _ = new ClrMameXmlExporter().Export(db2);

        db2.Games.Count.Should().Be(db.Games.Count);
    }
}
