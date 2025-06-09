// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.TorrentSevenZip;

using System;
using System.IO;
using Woohoo.IO.AbstractFileSystem.Internal.Zip;
using Woohoo.IO.Compression.TorrentSevenZip;

internal class ZipToTorrentSevenZipCopier : ZipToZipCopier
{
    protected override bool Compress => false;

    public override int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        Requires.NotNull(file);
        Requires.NotNullOrEmpty(targetContainerType);

        if (string.Compare(Path.GetExtension(file.ContainerAbsolutePath), ".zip", StringComparison.OrdinalIgnoreCase) == 0)
        {
            if (targetContainerType == "torrent7z")
            {
                return 10;
            }
        }

        return 0;
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
