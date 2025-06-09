// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning;

using System;

internal sealed record class EffectiveOnlineFolderSetting
{
    public required string FolderPath { get; init; }

    public bool Equals(EffectiveOnlineFolderSetting? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.FolderPath.Equals(other.FolderPath, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return this.FolderPath.ToLowerInvariant().GetHashCode();
    }
}
