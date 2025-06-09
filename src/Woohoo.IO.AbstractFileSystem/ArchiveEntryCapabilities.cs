// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.AbstractFileSystem;

using System;

[Flags]
public enum ArchiveEntryCapabilities
{
    None = 0,
    CanRead = 1 << 0, // 1
    CanWrite = 1 << 1, // 2
    CanDelete = 1 << 2, // 4
    CanExtract = 1 << 3, // 8
    CanCompress = 1 << 4, // 16
    CanRename = 1 << 5, // 32
    CanListContents = 1 << 6, // 64
    All = CanRead | CanWrite | CanDelete | CanExtract | CanCompress | CanRename | CanListContents,
}
