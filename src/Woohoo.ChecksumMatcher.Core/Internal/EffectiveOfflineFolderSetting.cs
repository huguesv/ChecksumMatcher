// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal;

using System;

internal sealed record class EffectiveOfflineFolderSetting
{
    public required string DiskName { get; init; }

    public required string FolderPath { get; init; }

    public bool Equals(EffectiveOfflineFolderSetting? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.DiskName.Equals(other.DiskName, StringComparison.OrdinalIgnoreCase) &&
               this.FolderPath.Equals(other.FolderPath, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.DiskName.ToLowerInvariant(), this.FolderPath.ToLowerInvariant());
    }
}
