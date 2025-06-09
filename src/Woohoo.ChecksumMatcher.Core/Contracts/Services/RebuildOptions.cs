// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

public class RebuildOptions
{
    public bool ForceCalculateChecksums { get; set; }

    public bool RemoveSource { get; set; }

    public string TargetContainerType { get; set; } = "folder";
}
