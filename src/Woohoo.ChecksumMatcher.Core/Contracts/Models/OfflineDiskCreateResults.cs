// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

using System;

public sealed class OfflineDiskCreateResults
{
    public long FolderCount { get; init; }

    public long FileCount { get; init; }

    public long ArchiveItemCount { get; init; }

    public TimeSpan? TimeSpan { get; init; }
}
