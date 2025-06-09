// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.TorrentSevenZip;

using System;
using System.IO;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Zip;
using Woohoo.IO.Compression.TorrentSevenZip;

internal class ZipToTorrentSevenZipCopier : ZipToZipCopier
{
    protected override bool Compress => false;

    public override int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        if (string.Compare(Path.GetExtension(file.ContainerAbsolutePath), ".zip", StringComparison.OrdinalIgnoreCase) == 0)
        {
            if (targetContainerType == KnownContainerTypes.TorrentSevenZip)
            {
                return 10;
            }
        }

        return 0;
    }

    public override string GetTargetContainerPath(string targetFolderPath, string containerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetFolderPath);
        ArgumentException.ThrowIfNullOrEmpty(containerName);

        return Path.Combine(targetFolderPath, containerName + ".7z");
    }

    protected override async Task PostProcessAsync(string targetArchiveFilePath, string[] expectedTargetFiles, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        if (await SharpZipContainer.IsCompleteAsync(targetArchiveFilePath, expectedTargetFiles))
        {
            await TorrentSevenZipper.TorrentZipAsync(targetArchiveFilePath, ct);
        }
    }
}
