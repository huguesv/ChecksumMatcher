// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System.Collections.Generic;

public interface IArchive
{
    bool IsSupportedArchiveFile(string path);

    IEnumerable<IArchiveEntry> EnumerateEntries(string path);

    IArchiveEntry[] GetEntries(string path);
}
