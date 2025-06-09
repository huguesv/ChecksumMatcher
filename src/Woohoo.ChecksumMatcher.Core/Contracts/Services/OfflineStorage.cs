// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using Woohoo.IO.AbstractFileSystem.Offline.Models;

public class OfflineStorage
{
    public required OfflineDisk Disk { get; set; }

    public string FolderPath { get; set; } = string.Empty;
}
