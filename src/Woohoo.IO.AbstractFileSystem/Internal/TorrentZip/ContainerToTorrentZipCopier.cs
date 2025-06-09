// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.TorrentZip;

using Woohoo.IO.AbstractFileSystem.Internal.Zip;
using Woohoo.IO.Compression.TorrentZip;

internal class ContainerToTorrentZipCopier : ContainerToZipCopier
{
    protected override bool Compress => false;

    public override int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);

        return targetContainerType == "torrentzip" ? 1 : 0;
    }

    protected override bool PostProcess(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        return TorrentZipper.Torrentzip(targetArchiveFilePath, expectedTargetFiles, SharpZipContainer.IsComplete);
    }
}
