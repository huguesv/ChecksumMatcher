// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

using System.Collections.Immutable;

public sealed class DatabaseFolderScanSettings
{
    public bool UseOnlineFolders { get; init; } = true;

    public bool UseOfflineFolders { get; init; } = true;

    public ImmutableArray<ScanOnlineFolderSetting> ScanOnlineFolders { get; init; } = [];

    public ImmutableArray<ScanOfflineFolderSetting> ScanOfflineFolders { get; init; } = [];
}
