// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;

using System;
using System.Collections.Immutable;
using System.IO;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Folder;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Offline;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.SevenZip;
using Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers.Zip;
using Woohoo.IO.AbstractFileSystem.Offline.Models;

public static class ContainerExtensionProvider
{
    private static readonly FolderContainer FolderContainer = new();

    private static readonly ImmutableArray<IContainer> CompressedContainers =
    [
        new SharpZipContainer(),
        new SevenZipContainer(),
    ];

    public static IContainer GetFolderContainer()
    {
        return FolderContainer;
    }

    public static IContainer GetOfflineFolderContainer(OfflineDisk offlineDisk)
    {
        return new OfflineFolderContainer(offlineDisk);
    }

    public static IContainer? GetContainer(string absoluteFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(absoluteFilePath);

        var container = CompressedContainers.FirstOrDefault(container => string.Equals(container.FileExtension, Path.GetExtension(absoluteFilePath), StringComparison.OrdinalIgnoreCase));
        if (container == null)
        {
            if (Directory.Exists(absoluteFilePath))
            {
                container = FolderContainer;
            }
        }

        return container;
    }
}
