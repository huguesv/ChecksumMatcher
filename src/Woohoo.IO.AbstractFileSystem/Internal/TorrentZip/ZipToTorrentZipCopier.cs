// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Internal.TorrentZip;

using System;
using System.IO;
using Woohoo.IO.AbstractFileSystem.Internal.Zip;
using Woohoo.IO.Compression.TorrentZip;

internal class ZipToTorrentZipCopier : ZipToZipCopier
{
    protected override bool Compress => false;

    public override int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);

        if (string.Compare(Path.GetExtension(file.ContainerAbsolutePath), ".zip", StringComparison.OrdinalIgnoreCase) == 0)
        {
            if (targetContainerType == "torrentzip")
            {
                return 10;
            }
        }

        return 0;
    }

    protected override bool PostProcess(string targetArchiveFilePath, string[] expectedTargetFiles)
    {
        return TorrentZipper.Torrentzip(targetArchiveFilePath, expectedTargetFiles, SharpZipContainer.IsComplete);
    }
}
