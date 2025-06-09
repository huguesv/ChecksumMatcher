// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using Woohoo.ChecksumDatabase.Model;

public interface IDatabaseImporter
{
    bool CanImport(string text);

    RomDatabase Import(string text, string workingFolderPath);
}
