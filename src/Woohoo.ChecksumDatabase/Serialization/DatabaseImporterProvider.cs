// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.ChecksumDatabase.Serialization.Extensions.ClrMame;
using Woohoo.ChecksumDatabase.Serialization.Extensions.Mess;

public sealed class DatabaseImporterProvider
{
    private readonly IDatabaseImporter[] importers;

    public DatabaseImporterProvider()
    {
        this.importers =
        [
            new ClrMameXmlImporter(),
            new ClrMameImporter(),
            new MessSoftwareListImporter(),
        ];
    }

    public bool CanLoad(string text, string workingFolderPath)
    {
        Requires.NotNull(text);
        Requires.NotNull(workingFolderPath);

        return this.importers.Any(importer => importer.CanImport(text));
    }

    public RomDatabase Load(string text, string workingFolderPath)
    {
        Requires.NotNull(text);
        Requires.NotNull(workingFolderPath);

        foreach (var importer in this.importers)
        {
            if (importer.CanImport(text))
            {
                return importer.Import(text);
            }
        }

        throw new NotSupportedException("Unsupported file format.");
    }
}
