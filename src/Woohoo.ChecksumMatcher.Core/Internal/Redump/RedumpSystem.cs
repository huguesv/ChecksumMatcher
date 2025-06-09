// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Redump;

internal sealed class RedumpSystem
{
    public required string Name { get; init; }

    public required string Id { get; init; }

    public bool HasCues { get; init; }

    public bool HasDat { get; init; }

    public bool HasDkeys { get; init; }

    public bool HasGdi { get; init; }

    public bool HasKeys { get; init; }

    public bool HasLsd { get; init; }

    public bool HasSbi { get; init; }

    public bool IsPrivate { get; init; }

    public int ArtifactCount =>
        (this.HasCues ? 1 : 0) +
        (this.HasDat ? 1 : 0) +
        (this.HasDkeys ? 1 : 0) +
        (this.HasGdi ? 1 : 0) +
        (this.HasKeys ? 1 : 0) +
        (this.HasLsd ? 1 : 0) +
        (this.HasSbi ? 1 : 0);
}
