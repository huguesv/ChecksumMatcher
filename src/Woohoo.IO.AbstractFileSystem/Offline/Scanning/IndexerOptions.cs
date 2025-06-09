// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline.Scanning;

public sealed record class IndexerOptions
{
    public bool CalculateChecksums { get; set; }

    public bool IndexArchiveContent { get; set; }
}
