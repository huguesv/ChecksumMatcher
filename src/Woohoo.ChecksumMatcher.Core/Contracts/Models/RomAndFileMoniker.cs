// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public class RomAndFileMoniker
{
    public RomAndFileMoniker(RomMoniker romMoniker, FileMoniker fileMoniker)
    {
        this.Rom = romMoniker ?? throw new ArgumentNullException(nameof(romMoniker));
        this.File = fileMoniker ?? throw new ArgumentNullException(nameof(fileMoniker));
    }

    public RomMoniker Rom { get; }

    public FileMoniker File { get; }
}
