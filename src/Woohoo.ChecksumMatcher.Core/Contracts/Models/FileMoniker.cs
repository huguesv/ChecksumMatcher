// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public class FileMoniker(string containerPath, string containerName, string romRelativeFilePath)
{
    public string ContainerPath { get; } = containerPath;

    public string ContainerName { get; } = containerName;

    public string RomRelativeFilePath { get; } = romRelativeFilePath;
}
