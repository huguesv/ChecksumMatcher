// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System;
using Woohoo.ChecksumDatabase.Model;

public interface IScanner
{
    event EventHandler<ScannerProgressEventArgs> Progress;

    void Cancel();

    ScannerResult Scan(RomDatabase db, Storage storage, ScanOptions options);
}
