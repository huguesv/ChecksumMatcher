// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public class RebuildEventArgs : DatabaseEventArgs
{
    public int ProgressPercentage { get; init; }

    public required DatabaseRebuildResults Results { get; init; }

    public required RebuildStatus Status { get; init; }

    public FileMoniker? HashingFile { get; init; }

    public RomMoniker? BuildingRom { get; init; }
}
