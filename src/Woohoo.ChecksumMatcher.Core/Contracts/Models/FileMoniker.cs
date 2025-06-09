// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public sealed class FileMoniker(string containerPath, string containerName, string romRelativeFilePath, bool isFromOfflineStorage)
{
    public string ContainerPath { get; } = containerPath;

    public string ContainerName { get; } = containerName;

    public string RomRelativeFilePath { get; } = romRelativeFilePath;

    public bool IsFromOfflineStorage { get; set; } = isFromOfflineStorage;
}
