// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Offline.Scanning;

public class IndexerProgressEventArgs : EventArgs
{
    public IndexerProgressEventArgs(long folderCount, long fileCount, long archiveItemCount, TimeSpan timeSpan)
    {
        this.FolderCount = folderCount;
        this.FileCount = fileCount;
        this.ArchiveItemCount = archiveItemCount;
        this.TimeSpan = timeSpan;
    }

    public long FolderCount { get; }

    public long FileCount { get; }

    public long ArchiveItemCount { get; }

    public TimeSpan TimeSpan { get; }
}
