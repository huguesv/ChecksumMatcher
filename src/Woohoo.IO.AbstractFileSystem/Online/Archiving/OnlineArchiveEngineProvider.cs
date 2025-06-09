// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online.Archiving;

using System.Collections.Generic;
using System.Linq;

internal class OnlineArchiveEngineProvider
{
    private readonly IOnlineArchiveEngine[] engines;

    public OnlineArchiveEngineProvider()
        : this([new OnlineArchiveEngineSevenZip(), new OnlineArchiveEngineSharpZip()])
    {
    }

    public OnlineArchiveEngineProvider(params IEnumerable<IOnlineArchiveEngine> engines)
    {
        this.engines = engines.ToArray();
    }

    public IOnlineArchiveEngine? GetEngine(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        return this.engines
            .Where(engine => engine.IsSupportedExtension(extension))
            .OrderBy(engine => engine.Priority)
            .FirstOrDefault();
    }
}
