// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

public enum ScannerProgress
{
    EnumeratingFilesStart,
    EnumeratingFilesEnd,
    CalculatingChecksumsStart,
    CalculatingChecksumsEnd,
    RebuildingRomStart,
    RebuildingRomEnd,
    Finished,
    Canceled,
    Unused,
    PerfectMatch,
    IncorrectContainerOrFileName,
    Missing,
}
