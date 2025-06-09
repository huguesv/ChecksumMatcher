// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using Woohoo.IO.AbstractFileSystem.Offline.Models;

public class OfflineDiskFindResult
{
    public OfflineDiskFindResult(string filePath, OfflineDisk? disk)
    {
        this.FilePath = filePath;
        this.Disk = disk;
    }

    public string FilePath { get; }

    public OfflineDisk? Disk { get; }
}
