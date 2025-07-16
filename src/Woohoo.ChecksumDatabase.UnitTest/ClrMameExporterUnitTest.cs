// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.UnitTest;

using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

public class ClrMameExporterUnitTest
{
    [Fact]
    public void ImportExportTosec()
    {
        ImportExport(Databases.Sega_Dreamcast___Games____GDI___TOSEC_v2007_12_30_CM_);
    }

    [Fact]
    public void ImportExportNoIntro()
    {
        ImportExport(Databases.Nintendo___Virtual_Boy__20071103_);
    }

    [Fact]
    public void ImportExportReimportReexportTosec()
    {
        ImportExportReimportReexport(Databases.Sega_Dreamcast___Games____GDI___TOSEC_v2007_12_30_CM_);
    }

    [Fact]
    public void ImportExportReimportReexportNoIntro()
    {
        ImportExportReimportReexport(Databases.Nintendo___Virtual_Boy__20071103_);
    }

    private static void ImportExport(string expected)
    {
        var db = new ClrMameImporter().Import(expected, string.Empty);

        var actual = new ClrMameExporter().Export(db);
        actual.Trim().Should().Be(expected.Trim());
    }

    private static void ImportExportReimportReexport(string expected)
    {
        var db = new ClrMameImporter().Import(expected, string.Empty);

        var actual = new ClrMameExporter().Export(db);
        actual.Trim().Should().Be(expected.Trim());

        var db2 = new ClrMameImporter().Import(actual, string.Empty);
        var actual2 = new ClrMameExporter().Export(db2);
        actual2.Trim().Should().Be(expected.Trim());
    }
}
