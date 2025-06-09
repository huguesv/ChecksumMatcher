// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System;

public interface IFileSystemInfo
{
    DateTime CreationTime { get; set; }

    DateTime CreationTimeUtc { get; set; }

    bool Exists { get; }

    string Extension { get; }

    string FullName { get; }

    DateTime LastAccessTime { get; set; }

    DateTime LastAccessTimeUtc { get; set; }

    DateTime LastWriteTime { get; set; }

    DateTime LastWriteTimeUtc { get; set; }

    string Name { get; }

    void Delete();

    void Refresh();
}
