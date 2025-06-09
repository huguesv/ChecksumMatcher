// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;
using Woohoo.ChecksumDatabase.Serialization.Extensions.Mess;

public class DatabaseImporterProvider
{
    private readonly IDatabaseImporter[] importers;

    public DatabaseImporterProvider(IDatabaseHeaderLoader headerLoader)
    {
        this.importers = new IDatabaseImporter[]
        {
            new ClrMameXmlImporter(),
            new ClrMameImporter(headerLoader),
            new MessSoftwareListImporter(),
        };
    }

    public bool CanLoad(string text, string workingFolderPath)
    {
        Requires.NotNull(text);
        Requires.NotNull(workingFolderPath);

        foreach (var importer in this.importers)
        {
            if (importer.CanImport(text))
            {
                return true;
            }
        }

        return false;
    }

    public RomDatabase Load(string text, string workingFolderPath)
    {
        Requires.NotNull(text);
        Requires.NotNull(workingFolderPath);

        foreach (var importer in this.importers)
        {
            if (importer.CanImport(text))
            {
                return importer.Import(text, workingFolderPath);
            }
        }

        throw new NotSupportedException("Unsupported file format.");
    }
}
