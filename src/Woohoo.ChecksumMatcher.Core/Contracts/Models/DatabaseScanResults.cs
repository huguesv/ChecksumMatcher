// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

using System.Collections.Immutable;

public class DatabaseScanResults
{
    public ImmutableArray<RomAndFileMoniker> Matched { get; init; } = [];

    public ImmutableArray<RomAndFileMoniker> WrongNamed { get; init; } = [];

    public ImmutableArray<RomMoniker> Missing { get; init; } = [];

    public ImmutableArray<FileMoniker> Unused { get; init; } = [];
}
