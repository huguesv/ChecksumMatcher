// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Redump;

internal sealed class RedumpSystem
{
    public required string Name { get; init; }

    public required string Id { get; init; }

    public bool HasCuePack { get; init; }

    public bool HasDat { get; init; } = true;

    public bool HasDkeyPack { get; init; }

    public bool HasGdiPack { get; init; }

    public bool HasKeysPack { get; init; }

    public bool HasSbiPack { get; init; }

    public bool IsPrivate { get; init; }

    public int ArtifactCount =>
        (this.HasCuePack ? 1 : 0) +
        (this.HasDat ? 1 : 0) +
        (this.HasDkeyPack ? 1 : 0) +
        (this.HasGdiPack ? 1 : 0) +
        (this.HasKeysPack ? 1 : 0) +
        (this.HasSbiPack ? 1 : 0);
}
