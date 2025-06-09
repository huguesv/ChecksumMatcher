// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

using System;

public sealed class OfflineDiskCreateProgressEventArgs(string operationId, OfflineDiskCreateStatus status, string diskName, long folderCount, long fileCount, long archiveItemCount, TimeSpan? timeSpan)
    : EventArgs
{
    public string OperationId { get; } = operationId;

    public OfflineDiskCreateStatus Status { get; } = status;

    public string DiskName { get; } = diskName;

    public long FolderCount { get; } = folderCount;

    public long FileCount { get; } = fileCount;

    public long ArchiveItemCount { get; } = archiveItemCount;

    public TimeSpan? TimeSpan { get; } = timeSpan;
}
