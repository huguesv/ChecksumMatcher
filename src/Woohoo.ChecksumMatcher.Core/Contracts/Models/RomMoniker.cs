// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Models;

public class RomMoniker(string containerName, string romRelativeFilePath)
{
    public string ContainerName { get; } = containerName;

    public string RomRelativeFilePath { get; } = romRelativeFilePath;
}
