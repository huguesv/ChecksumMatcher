// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System.Collections.Generic;
using Woohoo.IO.AbstractFileSystem.Internal;
using Woohoo.IO.AbstractFileSystem.Internal.TorrentSevenZip;
using Woohoo.IO.AbstractFileSystem.Internal.TorrentZip;
using Woohoo.IO.AbstractFileSystem.Internal.Zip;

public static class FileCopierExtensionProvider
{
    private static readonly CopierComposer Composer = new();

    public static IFileCopier? GetCopier(FileInformation file, string targetContainerType, string[] expectedTargetFiles)
    {
        Requires.NotNull(file);
        Requires.NotNullOrEmpty(targetContainerType);

        var priority = 0;
        IFileCopier? bestCopier = null;

        foreach (var currentCopier in Composer.Copiers)
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

    internal class CopierComposer
    {
        public IEnumerable<IFileCopier> Copiers { get; } = new IFileCopier[]
        {
            new ContainerToFolderCopier(),
            new ContainerToZipCopier(),
            new FolderToZipCopier(),
            new ZipToZipCopier(),
            new ContainerToTorrentSevenZipCopier(),
            new FolderSingleFileToTorrentSevenZipCopier(),
            new FolderToTorrentSevenZipCopier(),
            new ZipToTorrentSevenZipCopier(),
            new ContainerToTorrentZipCopier(),
            new FolderToTorrentZipCopier(),
            new ZipToTorrentZipCopier(),
        };
    }
}
