// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System;
using System.Collections.Immutable;
using System.IO;
using Woohoo.IO.AbstractFileSystem.Internal;
using Woohoo.IO.AbstractFileSystem.Internal.SevenZip;
using Woohoo.IO.AbstractFileSystem.Internal.Zip;

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
        return new FolderContainer();
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
