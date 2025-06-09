// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.WinUI.Models;

using System;

public sealed class OfflineDiskFolder
{
    public OfflineDiskFolder(string diskName, string folderPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(diskName, nameof(diskName));
        ArgumentException.ThrowIfNullOrEmpty(folderPath, nameof(folderPath));

        this.DiskName = diskName;
        this.FolderPath = folderPath;
    }

    public string DiskName { get; set; }

    public string FolderPath { get; set; }
}
