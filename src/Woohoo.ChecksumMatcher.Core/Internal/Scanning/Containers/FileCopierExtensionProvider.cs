// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;

using System.Collections.Immutable;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Folder;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.SevenZip;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.TorrentSevenZip;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.TorrentZip;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Zip;

internal static class FileCopierExtensionProvider
{
    private static readonly ImmutableArray<IFileCopier> Copiers =
    [
        new ContainerToFolderCopier(),
        new ContainerToZipCopier(),
        new ContainerToSevenZipCopier(),
        new FolderToZipCopier(),
        new FolderToSevenZipCopier(),
        new ZipToZipCopier(),
        new ContainerToTorrentSevenZipCopier(),
        new FolderSingleFileToTorrentSevenZipCopier(),
        new FolderToTorrentSevenZipCopier(),
        new ZipToTorrentSevenZipCopier(),
        new ContainerToTorrentZipCopier(),
        new FolderToTorrentZipCopier(),
        new ZipToTorrentZipCopier(),
    ];

    public static IFileCopier? GetCopier(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(targetContainerType);

        var priority = 0;
        IFileCopier? bestCopier = null;

        foreach (var currentCopier in Copiers)
        {
            var currentPriority = currentCopier.CanCopy(file, targetContainerType, expectedTargetFiles);
            if (currentPriority > priority)
            {
                bestCopier = currentCopier;
                priority = currentPriority;
            }
        }

        return bestCopier;
    }
}
