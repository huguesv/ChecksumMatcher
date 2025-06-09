// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using Woohoo.ChecksumDatabase.Model;
using Woohoo.IO.AbstractFileSystem;

public class ScannerMatchedRomResult
{
    public ScannerMatchedRomResult(RomFile rom, FileInformation file)
    {
        this.Rom = rom;
        this.File = file;
    }

    public RomFile Rom { get; }

    public FileInformation File { get; }
}
