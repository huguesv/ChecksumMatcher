// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public sealed class RomAndFileMoniker
{
    public RomAndFileMoniker(RomMoniker romMoniker, FileMoniker fileMoniker)
    {
        ArgumentNullException.ThrowIfNull(romMoniker);
        ArgumentNullException.ThrowIfNull(fileMoniker);

        this.Rom = romMoniker;
        this.File = fileMoniker;
    }

    public RomMoniker Rom { get; }

    public FileMoniker File { get; }
}
