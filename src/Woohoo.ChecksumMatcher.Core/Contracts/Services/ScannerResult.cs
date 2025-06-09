// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System.Collections.ObjectModel;

public class ScannerResult
{
    public Collection<ScannerMissingRomResult> MissingRoms { get; } = new Collection<ScannerMissingRomResult>();

    public Collection<ScannerUnusedFileResult> UnusedFiles { get; } = new Collection<ScannerUnusedFileResult>();

    public Collection<ScannerMatchedRomResult> MatchedRoms { get; } = new Collection<ScannerMatchedRomResult>();

    public Collection<ScannerWrongNameRomResult> WrongNameRoms { get; } = new Collection<ScannerWrongNameRomResult>();
}
