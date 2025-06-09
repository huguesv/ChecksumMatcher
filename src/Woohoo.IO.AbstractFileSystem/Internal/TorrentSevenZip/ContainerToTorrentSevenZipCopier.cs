// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.TorrentSevenZip;

using System.IO;
using Woohoo.IO.AbstractFileSystem.Internal.Zip;
using Woohoo.IO.Compression.TorrentSevenZip;

internal class ContainerToTorrentSevenZipCopier : ContainerToZipCopier
{
    protected override bool Compress => false;

    public override int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);

        return targetContainerType == "torrent7z" ? 1 : 0;
    }

    public override string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        return Path.Combine(targetFolderPath, containerName + ".7z");
    }

    protected override bool PostProcess(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        return TorrentSevenZipper.Torrentzip(targetArchiveFilePath, expectedTargetFiles, SharpZipContainer.IsComplete);
    }
}
