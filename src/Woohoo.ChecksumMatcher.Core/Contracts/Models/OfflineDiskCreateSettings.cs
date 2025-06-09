// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public class OfflineDiskCreateSettings
{
    public bool CalculateChecksums { get; init; } = false;

    public bool IndexArchiveContent { get; init; } = true;
}
