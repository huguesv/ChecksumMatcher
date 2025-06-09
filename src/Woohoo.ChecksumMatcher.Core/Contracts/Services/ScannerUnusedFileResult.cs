// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using Woohoo.IO.AbstractFileSystem;

public class ScannerUnusedFileResult
{
    public ScannerUnusedFileResult(FileInformation file)
    {
        this.File = file;
    }

    public FileInformation File { get; }
}
