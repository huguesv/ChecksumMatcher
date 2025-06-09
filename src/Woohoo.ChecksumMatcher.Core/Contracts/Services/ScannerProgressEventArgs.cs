// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System;
using Woohoo.ChecksumDatabase.Model;
using Woohoo.IO.AbstractFileSystem;

public class ScannerProgressEventArgs : EventArgs
{
    public RomFile? Rom { get; set; }

    public FileInformation? File { get; set; }

    public ScannerProgress Status { get; set; }

    public ScannerResult? Result { get; set; }
}
