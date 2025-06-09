// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System;
using System.Collections.Generic;
using System.IO;
using Woohoo.IO.AbstractFileSystem.Internal;
using Woohoo.IO.AbstractFileSystem.Internal.SevenZip;
using Woohoo.IO.AbstractFileSystem.Internal.Zip;

public static class ContainerExtensionProvider
{
    private static readonly FolderContainer FolderContainer = new();

    private static readonly ContainerComposer Composer = new();

    public static IContainer GetFolderContainer()
    {
        return new FolderContainer();
    }

    public static IContainer? GetContainer(string absoluteFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(absoluteFilePath);

        var container = GetCompressedContainer(absoluteFilePath);
        if (container == null)
        {
            if (Directory.Exists(absoluteFilePath))
            {
                container = FolderContainer;
            }
        }

        return container;
    }

    internal static IContainer? GetCompressedContainer(string absoluteFilePath)
    {
        foreach (var container in Composer.CompressedContainers)
        {
            if (string.Compare(container.FileExtension, Path.GetExtension(absoluteFilePath), StringComparison.OrdinalIgnoreCase) == 0)
            {
                return container;
            }
        }

        return null;
    }

    internal class ContainerComposer
    {
        public IEnumerable<IContainer> CompressedContainers { get; } = new IContainer[]
        {
            new SharpZipContainer(),
            new SevenZipContainer(),
        };
    }
}
