// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public sealed class RebuildSettings
{
    public bool ForceCalculateChecksums { get; init; }

    public bool RemoveSource { get; init; }

    public bool FindMissingCueFiles { get; init; }

    public bool TorrentZipIncomplete { get; init; }

    public string SourceFolderPath { get; init; } = string.Empty;

    public string TargetFolderPath { get; init; } = string.Empty;

    public string TargetIncompleteFolderPath { get; init; } = string.Empty;

    public string TargetContainerType { get; init; } = string.Empty;
}
