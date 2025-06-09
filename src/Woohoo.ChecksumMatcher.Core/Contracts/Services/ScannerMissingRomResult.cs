// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using Woohoo.ChecksumDatabase.Model;

public class ScannerMissingRomResult
{
    public ScannerMissingRomResult(RomFile rom)
    {
        this.Rom = rom;
    }

    public RomFile Rom { get; }
}
