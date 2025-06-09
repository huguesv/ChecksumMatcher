// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

using Woohoo.IO.AbstractFileSystem.Offline.Models;

public sealed class OfflineDiskFile
{
    public required string FilePath { get; init; }

    public required OfflineHeader Header { get; init; }
}
