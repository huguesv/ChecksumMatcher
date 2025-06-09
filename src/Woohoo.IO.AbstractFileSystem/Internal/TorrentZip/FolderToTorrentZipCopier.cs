// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.TorrentZip;

using System.IO;
using Woohoo.IO.AbstractFileSystem.Internal.Zip;
using Woohoo.IO.Compression.TorrentZip;

internal class FolderToTorrentZipCopier : FolderToZipCopier
{
    protected override bool Compress => false;

    public override int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        Requires.NotNull(file);
        Requires.NotNullOrEmpty(targetContainerType);

        if (Directory.Exists(file.ContainerAbsolutePath))
        {
            if (targetContainerType == "torrentzip")
            {
                return 5;
            }
        }

        return 0;
    }

    protected override bool PostProcess(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        return TorrentZipper.Torrentzip(targetArchiveFilePath, expectedTargetFiles, SharpZipContainer.IsComplete);
    }
}
