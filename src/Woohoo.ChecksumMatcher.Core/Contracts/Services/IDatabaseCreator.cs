// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ChecksumMatcher.Core.Contracts.Services;

using System;

public interface IDatabaseCreator
{
    event EventHandler<ScannerProgressEventArgs> Progress;

    void Cancel();

    void Build(string sourceFolderPath, string targetFilePath, DatabaseCreatorOptions options);
}
