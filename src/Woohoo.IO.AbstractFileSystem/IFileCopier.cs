// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

public interface IFileCopier
{
    int CanCopy(FileInformation file, string targetContainerType, string[] expectedTargetFiles);

    bool Copy(FileInformation file, string targetFolderPath, bool removeSource, bool allowContainerMove, string containerName, string fileName, string[] expectedTargetFiles);

    string GetTargetContainerPath(string targetFolderPath, string containerName);
}
