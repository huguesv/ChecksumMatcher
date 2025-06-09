// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public class ScanOfflineFolderSetting
{
    public bool IsIncluded { get; init; }

    public required string DiskName { get; init; }

    public required string FolderPath { get; init; }
}
