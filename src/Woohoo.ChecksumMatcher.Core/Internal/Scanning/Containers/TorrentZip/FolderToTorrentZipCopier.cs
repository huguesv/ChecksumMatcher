// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.TorrentZip;

using System.IO;
using Woohoo.ChecksumMatcher.Core.Contracts.Models;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Zip;
using Woohoo.IO.Compression.TorrentZip;

internal class FolderToTorrentZipCopier : FolderToZipCopier
{
    protected override bool Compress => false;

    public override int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        if (Directory.Exists(file.ContainerAbsolutePath))
        {
            if (targetContainerType == KnownContainerTypes.TorrentZip)
            {
                return 5;
            }
        }

        return 0;
    }

    protected override async Task PostProcessAsync(string targetArchiveFilePath, string[] expectedTargetFiles, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetArchiveFilePath);
        ArgumentNullException.ThrowIfNull(expectedTargetFiles);

        if (await SharpZipContainer.IsCompleteAsync(targetArchiveFilePath, expectedTargetFiles))
        {
            await TorrentZipper.TorrentZipAsync(targetArchiveFilePath, ct);
        }
    }
}
