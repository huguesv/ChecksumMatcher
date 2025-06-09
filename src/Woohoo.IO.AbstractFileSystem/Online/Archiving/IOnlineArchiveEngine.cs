// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online.Archiving;

using System.Collections.Generic;
using System.IO;

internal interface IOnlineArchiveEngine
{
    int Priority { get; }

    bool IsSupportedExtension(string extension);

    IEnumerable<IArchiveEntry> EnumerateEntries(string archiveFilePath);

    void Extract(OnlineArchiveEntry entry, string destinationPath);

    Stream OpenRead(OnlineArchiveEntry entry);

    void Delete(OnlineArchiveEntry entry);
}
