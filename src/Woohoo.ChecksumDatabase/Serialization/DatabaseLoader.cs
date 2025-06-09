// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using System;
using System.IO.Compression;
using System.Linq;
using Woohoo.ChecksumDatabase.Model;

public sealed class DatabaseLoader
{
    private readonly DatabaseImporterProvider importerProvider;

    public DatabaseLoader()
    {
        this.importerProvider = new DatabaseImporterProvider();
    }

    public RomDatabase? TryLoad(string filePath, CloneMode cloneMode)
    {
        var db = this.TryLoadFrom(filePath);
        return DatabaseCloneTransform.TransformDatabase(db, cloneMode);
    }

    private RomDatabase? TryLoadFrom(string filePath)
    {
        try
        {
            if (string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                using var zipArchive = new ZipArchive(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                if (zipArchive.Entries.Count != 1)
                {
                    return null;
                }

                var entry = zipArchive.Entries.FirstOrDefault();
                if (entry != null)
                {
                    if (entry.Name.EndsWith(".dat", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using var stream = entry.Open();
                        var reader = new StreamReader(stream);
                        var text = reader.ReadToEnd();
                        return this.importerProvider.Load(text, Path.GetDirectoryName(filePath) ?? string.Empty);
                    }
                }
            }
            else if (string.Equals(Path.GetExtension(filePath), ".dat", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(Path.GetExtension(filePath), ".xml", StringComparison.InvariantCultureIgnoreCase))
            {
                return this.importerProvider.Load(File.ReadAllText(filePath), Path.GetDirectoryName(filePath) ?? string.Empty);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
