// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System.Collections.Generic;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;

public class DatabaseExporterProvider
{
    private readonly IDatabaseExporter[] exporters;

    public DatabaseExporterProvider()
    {
        this.exporters = new IDatabaseExporter[]
        {
            new ClrMameXmlExporter(),
            new ClrMameExporter(),
        };
    }

    public string[] GetFilters()
    {
        var filters = new List<string>();

        foreach (var exporter in this.exporters)
        {
            filters.Add(exporter.Filter);
        }

        return filters.ToArray();
    }

    public string Save(RomDatabase db, string filter)
    {
        Requires.NotNull(db);
        Requires.NotNull(filter);

        foreach (var exporter in this.exporters)
        {
            if (exporter.Filter == filter)
            {
                return exporter.Export(db);
            }
        }

        return string.Empty;
    }
}
