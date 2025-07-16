// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

public class DatabaseExporterProvider
{
    private readonly IDatabaseExporter[] exporters;

    public DatabaseExporterProvider()
    {
        this.exporters =
        [
            new ClrMameXmlExporter(),
            new ClrMameExporter(),
        ];
    }

    public string[] GetFilters()
    {
        return this.exporters.Select(x => x.Filter).ToArray();
    }

    public string Save(RomDatabase db, string filter)
    {
        Requires.NotNull(db);
        Requires.NotNull(filter);

        var exporter = this.exporters.FirstOrDefault(x => x.Filter == filter);
        return exporter?.Export(db) ?? string.Empty;
    }
}
