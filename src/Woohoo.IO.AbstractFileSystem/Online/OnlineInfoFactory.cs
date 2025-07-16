// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem.Online;

using System;

internal static class OnlineInfoFactory
{
    public static IFileSystemInfo CreateFileSystemInfo(FileSystemInfo innerInfo)
    {
        ArgumentNullException.ThrowIfNull(innerInfo);

        return innerInfo switch
        {
            DirectoryInfo directoryInfo => new OnlineDirectoryInfo(directoryInfo),
            FileInfo fileInfo => new OnlineFileInfo(fileInfo),
            _ => throw new NotSupportedException($"Unsupported FileSystemInfo type: {innerInfo.GetType()}"),
        };
    }
}
