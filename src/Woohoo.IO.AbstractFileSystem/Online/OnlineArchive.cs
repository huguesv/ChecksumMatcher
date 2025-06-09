// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online;

using System.Collections.Generic;
using System.Linq;
using Woohoo.IO.AbstractFileSystem.Online.Archiving;

public class OnlineArchive : IArchive
{
    private readonly OnlineArchiveEngineProvider engineProvider = new();

    public bool IsSupportedArchiveFile(string path)
        => this.engineProvider.GetEngine(path) is not null;

    public IEnumerable<IArchiveEntry> EnumerateEntries(string path)
        => this.engineProvider.GetEngine(path)?.EnumerateEntries(path) ?? [];

    public IArchiveEntry[] GetEntries(string path)
        => this.EnumerateEntries(path).ToArray();
}
