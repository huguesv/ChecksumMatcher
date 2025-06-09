// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline.Scanning;

public sealed class IndexerProgressEventArgs(long folderCount, long fileCount, long archiveItemCount, TimeSpan timeSpan)
    : EventArgs
{
    public long FolderCount { get; } = folderCount;

    public long FileCount { get; } = fileCount;

    public long ArchiveItemCount { get; } = archiveItemCount;

    public TimeSpan TimeSpan { get; } = timeSpan;
}
