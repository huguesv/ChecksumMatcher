// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Internal.Scanning.Containers;

internal interface IFileCopier
{
    int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles);

    Task CopyAsync(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles, CancellationToken ct);

    string GetTargetContainerPath(string targetFolderPath, string containerName);
}
