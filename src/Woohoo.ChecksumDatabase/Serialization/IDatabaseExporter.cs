// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumDatabase.Serialization;

using Woohoo.ChecksumDatabase.Model;

public interface IDatabaseExporter
{
    string Filter { get; }

    string Export(RomDatabase db);
}
